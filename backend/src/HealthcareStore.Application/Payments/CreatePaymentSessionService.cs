using System.Security.Cryptography;
using System.Text;
using HealthcareStore.Application.Abstractions.Payments;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Payments.Models;
using HealthcareStore.Domain.Orders;
using HealthcareStore.Domain.Payments;

namespace HealthcareStore.Application.Payments;

public sealed class CreatePaymentSessionService(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IOrderInventoryReservationService inventoryReservationService,
    ITransactionManager transactionManager,
    IPaymentGateway paymentGateway,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan PaymentWindow =
        TimeSpan.FromMinutes(15);

    private static readonly TimeSpan InitializationLease =
        TimeSpan.FromSeconds(30);

    public async Task<CreatePaymentSessionResult> ExecuteAsync(
        Guid userId,
        Guid orderId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID cannot be empty.",
                nameof(orderId));
        }

        idempotencyKey = NormalizeIdempotencyKey(idempotencyKey);

        var requestHash = CreateRequestHash(orderId);
        var now = timeProvider.GetUtcNow();

        Payment payment;
        PaymentAttempt attempt;
        var initializeGatewayOrder = false;

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
                    new(CreatePaymentSessionStatus.OrderNotFound),
                    cancellationToken);
            }

            var existingPayment =
                await paymentRepository.GetByOrderIdForUpdateAsync(
                    orderId,
                    cancellationToken);

            if (existingPayment?.Status == PaymentStatus.Captured)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.PaymentAlreadyCaptured),
                    cancellationToken);
            }

            if (existingPayment is null &&
                order.Status != OrderStatus.Placed)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.OrderNotPayable),
                    cancellationToken);
            }

            var isNewPayment = existingPayment is null;

            payment = existingPayment ?? Payment.Create(
                order.Id,
                PaymentProvider.Razorpay,
                order.GrandTotal,
                order.Currency,
                now,
                now.Add(PaymentWindow));

            if (isNewPayment)
            {
                await paymentRepository.AddAsync(
                    payment,
                    cancellationToken);

                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        null,
                        PaymentEventType.PaymentCreated,
                        PaymentEventSource.Application,
                        now,
                        "Payment record created."),
                    cancellationToken);
            }

            if (payment.Status == PaymentStatus.Pending &&
                now >= payment.ExpiresAtUtc)
            {
                payment.Expire(now);

                if (order.Status == OrderStatus.Placed)
                {
                    order.Cancel(now);

                    await inventoryReservationService.ReleaseForOrderAsync(
                        order,
                        now,
                        cancellationToken);
                }

                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        null,
                        PaymentEventType.PaymentSessionExpired,
                        PaymentEventSource.Application,
                        now,
                        "The payment session expired."),
                    cancellationToken);

                await paymentRepository.SaveChangesAsync(
                    cancellationToken);

                await transactionManager.CommitAsync(
                    cancellationToken);

                return new(CreatePaymentSessionStatus.PaymentExpired);
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.PaymentExpired),
                    cancellationToken);
            }

            if (order.Status != OrderStatus.Placed)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.OrderNotPayable),
                    cancellationToken);
            }

            var existingAttempt =
                await paymentRepository
                    .GetAttemptByIdempotencyKeyForUpdateAsync(
                        payment.Id,
                        idempotencyKey,
                        cancellationToken);

            if (existingAttempt is not null &&
                existingAttempt.RequestHash != requestHash)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.IdempotencyConflict),
                    cancellationToken);
            }

            if (payment.ProviderOrderId is not null)
            {
                if (existingAttempt is null)
                {
                    attempt = PaymentAttempt.Create(
                        payment.Id,
                        idempotencyKey,
                        requestHash,
                        now);

                    attempt.MarkPending(now);

                    await paymentRepository.AddAsync(
                        attempt,
                        cancellationToken);

                    await paymentRepository.AddAsync(
                        CreateEvent(
                            payment,
                            attempt,
                            PaymentEventType.PaymentAttemptCreated,
                            PaymentEventSource.Application,
                            now,
                            "Payment attempt created."),
                        cancellationToken);

                    await paymentRepository.SaveChangesAsync(
                        cancellationToken);

                    await transactionManager.CommitAsync(
                        cancellationToken);

                    return new(
                        CreatePaymentSessionStatus.Created,
                        CreateSession(payment, attempt));
                }

                attempt = existingAttempt;

                if (attempt.Status == PaymentAttemptStatus.Initiated)
                {
                    attempt.MarkPending(now);

                    await paymentRepository.SaveChangesAsync(
                        cancellationToken);
                }

                await transactionManager.CommitAsync(
                    cancellationToken);

                return new(
                    CreatePaymentSessionStatus.Replayed,
                    CreateSession(payment, attempt));
            }

            if (existingAttempt is not null)
            {
                attempt = existingAttempt;

                if (!attempt.TryAcquireGatewayOrderInitialization(
                        now,
                        InitializationLease))
                {
                    return await RollbackAndReturnAsync(
                        new(
                            CreatePaymentSessionStatus.PaymentAttemptInProgress),
                        cancellationToken);
                }
            }
            else
            {
                var activeAttempts =
                    await paymentRepository.GetActiveAttemptsForUpdateAsync(
                        payment.Id,
                        cancellationToken);

                if (activeAttempts.Count > 0)
                {
                    return await RollbackAndReturnAsync(
                        new(
                            CreatePaymentSessionStatus.PaymentAttemptInProgress),
                        cancellationToken);
                }

                attempt = PaymentAttempt.Create(
                    payment.Id,
                    idempotencyKey,
                    requestHash,
                    now);

                await paymentRepository.AddAsync(
                    attempt,
                    cancellationToken);

                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        attempt,
                        PaymentEventType.PaymentAttemptCreated,
                        PaymentEventSource.Application,
                        now,
                        "Payment attempt created."),
                    cancellationToken);
            }

            await paymentRepository.SaveChangesAsync(
                cancellationToken);

            await transactionManager.CommitAsync(
                cancellationToken);

            initializeGatewayOrder = true;
        }
        catch
        {
            await transactionManager.RollbackAsync(
                cancellationToken);

            throw;
        }

        if (!initializeGatewayOrder)
        {
            return new(
                CreatePaymentSessionStatus.PaymentAttemptInProgress);
        }

        return await InitializeGatewayOrderAsync(
            userId,
            orderId,
            idempotencyKey,
            payment.Id,
            attempt.Id,
            payment.Amount,
            payment.Currency,
            payment.ExpiresAtUtc,
            cancellationToken);
    }

    private async Task<CreatePaymentSessionResult>
        InitializeGatewayOrderAsync(
            Guid userId,
            Guid orderId,
            string idempotencyKey,
            Guid paymentId,
            Guid paymentAttemptId,
            decimal amount,
            string currency,
            DateTimeOffset expiresAtUtc,
            CancellationToken cancellationToken)
    {
        var receipt = paymentId.ToString("N");
        var amountSubunits = ToSubunits(amount, currency);

        GatewayOrderResult? gatewayOrder;

        try
        {
            gatewayOrder =
                await paymentGateway.FindOrderByReceiptAsync(
                    receipt,
                    cancellationToken);

            if (gatewayOrder is null)
            {
                try
                {
                    gatewayOrder =
                        await paymentGateway.CreateOrderAsync(
                            new CreateGatewayOrderRequest(
                                amountSubunits,
                                currency,
                                receipt),
                            cancellationToken);
                }
                catch (Exception exception) when (
                    !cancellationToken.IsCancellationRequested &&
                    exception is HttpRequestException or TaskCanceledException)
                {
                    // The request may have reached Razorpay even if
                    // the client timed out. Recover before retrying creation.
                    gatewayOrder =
                        await paymentGateway.FindOrderByReceiptAsync(
                            receipt,
                            cancellationToken);

                    if (gatewayOrder is null)
                    {
                        return new(
                            CreatePaymentSessionStatus.GatewayUnavailable);
                    }
                }
            }
        }
        catch (Exception exception) when (
            !cancellationToken.IsCancellationRequested &&
            exception is HttpRequestException or TaskCanceledException)
        {
            return new(
                CreatePaymentSessionStatus.GatewayUnavailable);
        }

        if (gatewayOrder is null)
        {
            return new(
                CreatePaymentSessionStatus.GatewayUnavailable);
        }

        if (!string.Equals(
                gatewayOrder.Receipt,
                receipt,
                StringComparison.Ordinal) ||
            gatewayOrder.AmountSubunits != amountSubunits ||
            !string.Equals(
                gatewayOrder.Currency,
                currency,
                StringComparison.OrdinalIgnoreCase))
        {
            await RecordReconciliationRequiredAsync(
                userId,
                orderId,
                paymentId,
                paymentAttemptId,
                gatewayOrder,
                "Gateway order receipt, amount, or currency did not match.",
                cancellationToken);

            return new(
                CreatePaymentSessionStatus.ReconciliationRequired);
        }

        if (string.Equals(
                gatewayOrder.Status,
                "paid",
                StringComparison.OrdinalIgnoreCase))
        {
            await RecordReconciliationRequiredAsync(
                userId,
                orderId,
                paymentId,
                paymentAttemptId,
                gatewayOrder,
                "The Razorpay order is already paid but has not been reconciled locally.",
                cancellationToken);

            return new(
                CreatePaymentSessionStatus.ReconciliationRequired);
        }

        return await PersistGatewayOrderAsync(
            userId,
            orderId,
            idempotencyKey,
            paymentId,
            paymentAttemptId,
            gatewayOrder,
            expiresAtUtc,
            cancellationToken);
    }

    private async Task<CreatePaymentSessionResult>
        PersistGatewayOrderAsync(
            Guid userId,
            Guid orderId,
            string idempotencyKey,
            Guid paymentId,
            Guid paymentAttemptId,
            GatewayOrderResult gatewayOrder,
            DateTimeOffset expiresAtUtc,
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

            var payment =
                await paymentRepository.GetByOrderIdForUpdateAsync(
                    orderId,
                    cancellationToken);

            if (order is null || payment is null ||
                payment.Id != paymentId)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.OrderNotFound),
                    cancellationToken);
            }

            var attempt =
                await paymentRepository
                    .GetAttemptByIdempotencyKeyForUpdateAsync(
                        payment.Id,
                        idempotencyKey,
                        cancellationToken);

            if (attempt is null || attempt.Id != paymentAttemptId)
            {
                return await RollbackAndReturnAsync(
                    new(CreatePaymentSessionStatus.ReconciliationRequired),
                    cancellationToken);
            }

            if (payment.ProviderOrderId is not null &&
                payment.ProviderOrderId != gatewayOrder.Id)
            {
                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        attempt,
                        PaymentEventType.ReconciliationRequired,
                        PaymentEventSource.Reconciliation,
                        timeProvider.GetUtcNow(),
                        "A different Razorpay order ID was already stored."),
                    cancellationToken);

                await paymentRepository.SaveChangesAsync(
                    cancellationToken);

                await transactionManager.CommitAsync(
                    cancellationToken);

                return new(
                    CreatePaymentSessionStatus.ReconciliationRequired);
            }

            if (order.Status != OrderStatus.Placed ||
                payment.Status != PaymentStatus.Pending ||
                timeProvider.GetUtcNow() >= payment.ExpiresAtUtc)
            {
                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        attempt,
                        PaymentEventType.ReconciliationRequired,
                        PaymentEventSource.Reconciliation,
                        timeProvider.GetUtcNow(),
                        "Gateway order created after the local order or payment became ineligible."),
                    cancellationToken);

                await paymentRepository.SaveChangesAsync(
                    cancellationToken);

                await transactionManager.CommitAsync(
                    cancellationToken);

                return new(
                    CreatePaymentSessionStatus.OrderNotPayable);
            }

            var providerOrderAlreadyStored =
                payment.ProviderOrderId is not null;

            payment.SetProviderOrderId(
                gatewayOrder.Id,
                timeProvider.GetUtcNow());

            if (attempt.Status == PaymentAttemptStatus.Initiated)
            {
                attempt.MarkPending(timeProvider.GetUtcNow());
            }

            if (!providerOrderAlreadyStored)
            {
                await paymentRepository.AddAsync(
                    CreateEvent(
                        payment,
                        attempt,
                        PaymentEventType.RazorpayOrderCreated,
                        PaymentEventSource.RazorpayApi,
                        timeProvider.GetUtcNow(),
                        "Razorpay order created and linked to the payment."),
                    cancellationToken);
            }

            await paymentRepository.SaveChangesAsync(
                cancellationToken);

            await transactionManager.CommitAsync(
                cancellationToken);

            return new(
                CreatePaymentSessionStatus.Created,
                new PaymentSessionModel(
                    payment.Id,
                    attempt.Id,
                    paymentGateway.PublicKeyId,
                    gatewayOrder.Id,
                    gatewayOrder.AmountSubunits,
                    gatewayOrder.Currency,
                    expiresAtUtc));
        }
        catch
        {
            await transactionManager.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task RecordReconciliationRequiredAsync(
        Guid userId,
        Guid orderId,
        Guid paymentId,
        Guid paymentAttemptId,
        GatewayOrderResult gatewayOrder,
        string description,
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

            var payment =
                await paymentRepository.GetByOrderIdForUpdateAsync(
                    orderId,
                    cancellationToken);

            if (order is not null &&
                payment is not null &&
                payment.Id == paymentId)
            {
                var amount = string.Equals(
                    gatewayOrder.Currency,
                    "INR",
                    StringComparison.OrdinalIgnoreCase)
                    ? gatewayOrder.AmountSubunits / 100m
                    : (decimal?)null;

                await paymentRepository.AddAsync(
                    PaymentEvent.Create(
                        paymentId,
                        paymentAttemptId,
                        PaymentEventType.ReconciliationRequired,
                        PaymentEventSource.Reconciliation,
                        timeProvider.GetUtcNow(),
                        timeProvider.GetUtcNow(),
                        providerOrderId: gatewayOrder.Id,
                        amount: amount,
                        currency: amount.HasValue
                            ? gatewayOrder.Currency
                            : null,
                        description: description),
                    cancellationToken);

                await paymentRepository.SaveChangesAsync(
                    cancellationToken);
            }

            await transactionManager.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transactionManager.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task<CreatePaymentSessionResult> RollbackAndReturnAsync(
        CreatePaymentSessionResult result,
        CancellationToken cancellationToken)
    {
        await transactionManager.RollbackAsync(cancellationToken);

        return result;
    }

    private static PaymentEvent CreateEvent(
        Payment payment,
        PaymentAttempt? attempt,
        PaymentEventType eventType,
        PaymentEventSource source,
        DateTimeOffset nowUtc,
        string description)
    {
        return PaymentEvent.Create(
            payment.Id,
            attempt?.Id,
            eventType,
            source,
            occurredAtUtc: nowUtc,
            recordedAtUtc: nowUtc,
            amount: payment.Amount,
            currency: payment.Currency,
            description: description);
    }

    private static string NormalizeIdempotencyKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var normalized = key.Trim();

        if (normalized.Length > 128)
        {
            throw new ArgumentException(
                "Idempotency key cannot exceed 128 characters.",
                nameof(key));
        }

        return normalized;
    }

    private static string CreateRequestHash(Guid orderId)
    {
        var input = Encoding.UTF8.GetBytes(
            $"create-payment-session-v1|{orderId:N}");

        return Convert.ToHexString(
                SHA256.HashData(input))
            .ToLowerInvariant();
    }

    private static long ToSubunits(
        decimal amount,
        string currency)
    {
        if (!string.Equals(
                currency,
                "INR",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Currency '{currency}' is not supported by this payment integration.");
        }

        var subunits = amount * 100m;

        if (subunits != decimal.Truncate(subunits))
        {
            throw new InvalidOperationException(
                "The payment amount contains fractions smaller than one paise.");
        }

        return decimal.ToInt64(subunits);
    }

    private PaymentSessionModel CreateSession(
        Payment payment,
        PaymentAttempt attempt)
    {
        return new PaymentSessionModel(
            payment.Id,
            attempt.Id,
            paymentGateway.PublicKeyId,
            payment.ProviderOrderId!,
            ToSubunits(payment.Amount, payment.Currency),
            payment.Currency,
            payment.ExpiresAtUtc);
    }
}