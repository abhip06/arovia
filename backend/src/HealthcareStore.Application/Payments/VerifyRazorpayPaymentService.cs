using HealthcareStore.Application.Abstractions.Payments;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Application.Payments.Models;
using HealthcareStore.Domain.Orders;
using HealthcareStore.Domain.Payments;

namespace HealthcareStore.Application.Payments;

public sealed class VerifyRazorpayPaymentService(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IOrderInventoryReservationService inventoryReservationService,
    ITransactionManager transactionManager,
    IPaymentGateway paymentGateway,
    TimeProvider timeProvider)
{
    public async Task<VerifyRazorpayPaymentResult> ExecuteAsync(
        Guid userId,
        VerifyRazorpayPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.ProviderOrderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.ProviderPaymentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Signature);

        if (request.PaymentAttemptId == Guid.Empty)
        {
            throw new ArgumentException(
                "Payment attempt ID cannot be empty.",
                nameof(request));
        }

        var paymentSnapshot =
            await paymentRepository.GetByProviderOrderIdAsync(
                request.ProviderOrderId,
                cancellationToken);

        if (paymentSnapshot is null ||
            !string.Equals(
                paymentSnapshot.ProviderOrderId,
                request.ProviderOrderId,
                StringComparison.Ordinal))
        {
            return new(VerifyRazorpayPaymentStatus.PaymentNotFound);
        }

        var attemptSnapshot =
            await paymentRepository.GetAttemptByIdAsync(
                paymentSnapshot.Id,
                request.PaymentAttemptId,
                cancellationToken);

        if (attemptSnapshot is null)
        {
            return new(VerifyRazorpayPaymentStatus.PaymentNotFound);
        }

        var customerOrder =
            await orderRepository.GetCustomerOrderByIdAsync(
                paymentSnapshot.OrderId,
                userId,
                cancellationToken);

        if (customerOrder is null)
        {
            return new(VerifyRazorpayPaymentStatus.OrderNotFound);
        }

        if (!paymentGateway.VerifyCheckoutSignature(
                paymentSnapshot.ProviderOrderId!,
                request.ProviderPaymentId,
                request.Signature))
        {
            return new(VerifyRazorpayPaymentStatus.InvalidSignature);
        }

        var gatewayPayment =
            await paymentGateway.GetPaymentAsync(
                request.ProviderPaymentId,
                cancellationToken);

        if (!MatchesExpectedPayment(
                paymentSnapshot,
                request,
                gatewayPayment,
                out var method))
        {
            return new(VerifyRazorpayPaymentStatus.InvalidPayment);
        }

        if (string.Equals(
                gatewayPayment.Status,
                "captured",
                StringComparison.OrdinalIgnoreCase) &&
            gatewayPayment.Captured)
        {
            return await PersistCapturedPaymentAsync(
                userId,
                paymentSnapshot.OrderId,
                paymentSnapshot.Id,
                request.PaymentAttemptId,
                request.ProviderOrderId,
                gatewayPayment,
                method,
                cancellationToken);
        }

        if (gatewayPayment.Captured)
        {
            return new(VerifyRazorpayPaymentStatus.InvalidPayment);
        }

        if (string.Equals(
                gatewayPayment.Status,
                "authorized",
                StringComparison.OrdinalIgnoreCase))
        {
            return await PersistNonCapturedStatusAsync(
                userId,
                paymentSnapshot.OrderId,
                paymentSnapshot.Id,
                request.PaymentAttemptId,
                request.ProviderOrderId,
                gatewayPayment,
                method,
                failed: false,
                cancellationToken);
        }

        if (string.Equals(
                gatewayPayment.Status,
                "failed",
                StringComparison.OrdinalIgnoreCase))
        {
            return await PersistNonCapturedStatusAsync(
                userId,
                paymentSnapshot.OrderId,
                paymentSnapshot.Id,
                request.PaymentAttemptId,
                request.ProviderOrderId,
                gatewayPayment,
                method,
                failed: true,
                cancellationToken);
        }

        return new(VerifyRazorpayPaymentStatus.PaymentPending);
    }

    private async Task<VerifyRazorpayPaymentResult>
        PersistCapturedPaymentAsync(
            Guid userId,
            Guid orderId,
            Guid paymentId,
            Guid paymentAttemptId,
            string providerOrderId,
            GatewayPaymentResult gatewayPayment,
            PaymentMethod method,
            CancellationToken cancellationToken)
    {
        await transactionManager.BeginAsync(cancellationToken);

        try
        {
            // Keep the lock order consistent across order workflows:
            // Order -> Payment -> PaymentAttempt.
            var order =
                await orderRepository.GetCustomerOrderForUpdateAsync(
                    orderId,
                    userId,
                    cancellationToken);

            if (order is null)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.OrderNotFound),
                    cancellationToken);
            }

            var payment =
                await paymentRepository.GetByOrderIdForUpdateAsync(
                    orderId,
                    cancellationToken);

            if (payment is null ||
                payment.Id != paymentId ||
                payment.ProviderOrderId != providerOrderId)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.InvalidPayment),
                    cancellationToken);
            }

            var attempt =
                await paymentRepository.GetAttemptByIdForUpdateAsync(
                    payment.Id,
                    paymentAttemptId,
                    cancellationToken);

            if (attempt is null)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.PaymentNotFound),
                    cancellationToken);
            }

            if (payment.Status == PaymentStatus.Captured &&
                attempt.Status == PaymentAttemptStatus.Captured &&
                attempt.ProviderPaymentId == gatewayPayment.Id)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.AlreadyCaptured),
                    cancellationToken);
            }

            if (payment.Status == PaymentStatus.Captured ||
                (attempt.Status == PaymentAttemptStatus.Captured &&
                 attempt.ProviderPaymentId != gatewayPayment.Id))
            {
                var duplicateAlreadyRecorded =
                    await paymentRepository.PaymentEventExistsAsync(
                        payment.Id,
                        gatewayPayment.Id,
                        PaymentEventType.DuplicatePaymentDetected,
                        cancellationToken);

                if (!duplicateAlreadyRecorded)
                {
                    await paymentRepository.AddAsync(
                        CreateEvent(
                            payment,
                            attempt,
                            PaymentEventType.DuplicatePaymentDetected,
                            PaymentEventSource.Reconciliation,
                            timeProvider.GetUtcNow(),
                            "A distinct captured payment was reported for a payment that was already captured.",
                            gatewayPayment.Id),
                        cancellationToken);

                    await paymentRepository.SaveChangesAsync(
                        cancellationToken);
                }

                await transactionManager.CommitAsync(
                    cancellationToken);

                return new(
                    VerifyRazorpayPaymentStatus.ReconciliationRequired);
            }

            // Razorpay's payment-fetch response provides created_at,
            // not a separate captured_at field. This is the time Arovia
            // verified capture, not a claimed provider capture timestamp.
            var verifiedAtUtc = timeProvider.GetUtcNow();

            var previousPaymentStatus = payment.Status;

            attempt.MarkCaptured(
                gatewayPayment.Id,
                method,
                verifiedAtUtc,
                verifiedAtUtc);

            payment.MarkCaptured(
                verifiedAtUtc,
                verifiedAtUtc);

            await paymentRepository.AddAsync(
                CreateEvent(
                    payment,
                    attempt,
                    PaymentEventType.PaymentCaptured,
                    PaymentEventSource.RazorpayApi,
                    verifiedAtUtc,
                    "Razorpay reported the payment as captured and Arovia verified the gateway payment.",
                    gatewayPayment.Id),
                cancellationToken);

            var canConfirmOrder =
                order.Status == OrderStatus.Placed &&
                previousPaymentStatus == PaymentStatus.Pending &&
                verifiedAtUtc < payment.ExpiresAtUtc;

            VerifyRazorpayPaymentStatus resultStatus;

            if (canConfirmOrder)
            {
                order.Confirm(verifiedAtUtc);

                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        attempt,
                        PaymentEventType.OrderConfirmed,
                        PaymentEventSource.Application,
                        verifiedAtUtc,
                        "Order confirmed after verified payment capture.",
                        gatewayPayment.Id),
                    cancellationToken);

                resultStatus = VerifyRazorpayPaymentStatus.Captured;
            }
            else
            {
                if (order.Status == OrderStatus.Placed)
                {
                    // The payment is real, but its window has expired or
                    // the payment session was no longer pending. Do not
                    // confirm the order; release its remaining reservation.
                    order.Cancel(verifiedAtUtc);

                    await inventoryReservationService.ReleaseForOrderAsync(
                        order,
                        verifiedAtUtc,
                        cancellationToken);
                }

                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        attempt,
                        PaymentEventType.ReconciliationRequired,
                        PaymentEventSource.Reconciliation,
                        verifiedAtUtc,
                        "Payment was captured, but the order could not be confirmed automatically.",
                        gatewayPayment.Id),
                    cancellationToken);

                resultStatus =
                    VerifyRazorpayPaymentStatus.ReconciliationRequired;
            }

            await paymentRepository.SaveChangesAsync(
                cancellationToken);

            var orderDetails = await orderRepository.GetByIdAsync(
                order.Id,
                cancellationToken);

            await transactionManager.CommitAsync(
                cancellationToken);

            return new(resultStatus, orderDetails);
        }
        catch
        {
            await transactionManager.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task<VerifyRazorpayPaymentResult>
        PersistNonCapturedStatusAsync(
            Guid userId,
            Guid orderId,
            Guid paymentId,
            Guid paymentAttemptId,
            string providerOrderId,
            GatewayPaymentResult gatewayPayment,
            PaymentMethod method,
            bool failed,
            CancellationToken cancellationToken)
    {
        await transactionManager.BeginAsync(cancellationToken);

        try
        {
            var order =
                await orderRepository.GetCustomerOrderForUpdateAsync(
                    orderId,
                    userId,
                    cancellationToken);

            if (order is null)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.OrderNotFound),
                    cancellationToken);
            }

            var payment =
                await paymentRepository.GetByOrderIdForUpdateAsync(
                    orderId,
                    cancellationToken);

            if (payment is null ||
                payment.Id != paymentId ||
                payment.ProviderOrderId != providerOrderId)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.InvalidPayment),
                    cancellationToken);
            }

            var attempt =
                await paymentRepository.GetAttemptByIdForUpdateAsync(
                    payment.Id,
                    paymentAttemptId,
                    cancellationToken);

            if (attempt is null)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.PaymentNotFound),
                    cancellationToken);
            }

            if (payment.Status == PaymentStatus.Captured)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.AlreadyCaptured),
                    cancellationToken);
            }

            if (payment.Status != PaymentStatus.Pending ||
                order.Status != OrderStatus.Placed)
            {
                return await RollbackAndReturnAsync(
                    new(VerifyRazorpayPaymentStatus.OrderNotPayable),
                    cancellationToken);
            }

            var now = timeProvider.GetUtcNow();

            // Avoid creating the same state event repeatedly when the
            // same Checkout result is submitted again.
            if (attempt.ProviderPaymentId == gatewayPayment.Id &&
                ((failed &&
                  attempt.Status == PaymentAttemptStatus.Failed) ||
                 (!failed &&
                  attempt.Status == PaymentAttemptStatus.Authorized)))
            {
                return await RollbackAndReturnAsync(
                    new(
                        failed
                            ? VerifyRazorpayPaymentStatus.PaymentFailed
                            : VerifyRazorpayPaymentStatus.PaymentPending),
                    cancellationToken);
            }

            PaymentEventType eventType;
            string description;

            if (failed)
            {
                attempt.MarkFailed(
                    gatewayPayment.Id,
                    method,
                    failureCode: null,
                    failureDescription: "Razorpay reported a failed payment.",
                    failedAtUtc: now,
                    recordedAtUtc: now);

                eventType = PaymentEventType.PaymentFailed;
                description = "Razorpay reported that the payment attempt failed.";
            }
            else
            {
                attempt.MarkAuthorized(
                    gatewayPayment.Id,
                    method,
                    authorizedAtUtc: now,
                    recordedAtUtc: now);

                eventType = PaymentEventType.PaymentAuthorized;
                description = "Razorpay reported that the payment was authorized but not captured.";
            }

            await paymentRepository.AddAsync(
                CreateEvent(
                    payment,
                    attempt,
                    eventType,
                    PaymentEventSource.RazorpayApi,
                    now,
                    description,
                    gatewayPayment.Id),
                cancellationToken);

            await paymentRepository.SaveChangesAsync(
                cancellationToken);

            await transactionManager.CommitAsync(
                cancellationToken);

            return new(
                failed
                    ? VerifyRazorpayPaymentStatus.PaymentFailed
                    : VerifyRazorpayPaymentStatus.PaymentPending);
        }
        catch
        {
            await transactionManager.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private static bool MatchesExpectedPayment(
        Payment payment,
        VerifyRazorpayPaymentRequest request,
        GatewayPaymentResult gatewayPayment,
        out PaymentMethod method)
    {
        method = default;

        if (gatewayPayment.Id != request.ProviderPaymentId ||
            gatewayPayment.OrderId != payment.ProviderOrderId ||
            !string.Equals(
                gatewayPayment.Currency,
                payment.Currency,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                payment.Currency,
                "INR",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var amountSubunits = payment.Amount * 100m;

        if (amountSubunits != decimal.Truncate(amountSubunits) ||
            amountSubunits != gatewayPayment.AmountSubunits)
        {
            return false;
        }

        return TryMapPaymentMethod(
            gatewayPayment.Method,
            out method);
    }

    private static bool TryMapPaymentMethod(
        string gatewayMethod,
        out PaymentMethod method)
    {
        switch (gatewayMethod.Trim().ToLowerInvariant())
        {
            case "card":
                method = PaymentMethod.Card;
                return true;

            case "netbanking":
                method = PaymentMethod.NetBanking;
                return true;

            case "upi":
                method = PaymentMethod.Upi;
                return true;

            default:
                method = default;
                return false;
        }
    }

    private static PaymentEvent CreateEvent(
        Payment payment,
        PaymentAttempt? attempt,
        PaymentEventType eventType,
        PaymentEventSource source,
        DateTimeOffset nowUtc,
        string description,
        string? providerPaymentId = null)
    {
        return PaymentEvent.Create(
            payment.Id,
            attempt?.Id,
            eventType,
            source,
            occurredAtUtc: nowUtc,
            recordedAtUtc: nowUtc,
            providerOrderId: payment.ProviderOrderId,
            providerPaymentId: providerPaymentId,
            amount: payment.Amount,
            currency: payment.Currency,
            description: description);
    }

    private async Task<VerifyRazorpayPaymentResult> RollbackAndReturnAsync(
        VerifyRazorpayPaymentResult result,
        CancellationToken cancellationToken)
    {
        await transactionManager.RollbackAsync(cancellationToken);
        return result;
    }
}