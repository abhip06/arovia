using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Checkout;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace HealthcareStore.Application.Orders;

public sealed class CreateOrderService(
    ICartRepository cartRepository,
    IAddressRepository addressRepository,
    IUserRepository userRepository,
    CheckoutService checkoutService,
    IOrderRepository orderRepository,
    IOrderIdempotencyRepository idempotencyRepository,
    IOrderInventoryReservationService inventoryReservationService,
    IOrderNumberGenerator orderNumberGenerator,
    ITransactionManager transactionManager,
    ICacheService cache,
    TimeProvider timeProvider,
    ILogger<CreateOrderService> logger)
{
    private static readonly TimeSpan IdempotencyLifetime = TimeSpan.FromHours(24);

    public async Task<CreateOrderResult> CreateAsync(
        Guid userId,
        CreateOrderRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        if (request.ShippingAddressId == Guid.Empty)
        {
            throw new ArgumentException(
                "Shipping address ID is required.",
                nameof(request));
        }

        var key = NormalizeIdempotencyKey(idempotencyKey);

        var requestHash = CacheKeyFingerprint.Create(
            "create-order-v1",
            request.ShippingAddressId);

        // Fast path for a previously committed order.
        var existing = await idempotencyRepository.FindAsync(
            userId,
            key,
            cancellationToken);

        if (existing is not null)
        {
            return await ResolveExistingAsync(
                existing,
                requestHash,
                cancellationToken);
        }

        await transactionManager.BeginAsync(cancellationToken);

        try
        {
            // Must use the same PostgreSQL advisory transaction lock as
            // AddItemAsync, SetItemQuantityAsync and RemoveItemAsync.
            await cartRepository.AcquireCheckoutLockAsync(
                userId,
                cancellationToken);

            // A concurrent checkout may have committed while we waited.
            existing = await idempotencyRepository.FindAsync(
                userId,
                key,
                cancellationToken);

            if (existing is not null)
            {
                var replay = await ResolveExistingAsync(
                    existing,
                    requestHash,
                    cancellationToken);

                await transactionManager.CommitAsync(cancellationToken);

                return replay;
            }

            var address = await addressRepository.GetByIdAsync(
                userId,
                request.ShippingAddressId,
                cancellationToken);

            if (address is null)
            {
                return await AbortAsync(
                    CreateOrderStatus.ShippingAddressNotFound);
            }

            if (!IsValidPincode(address.PostalCode) ||
                string.IsNullOrWhiteSpace(address.RecipientName) ||
                string.IsNullOrWhiteSpace(address.AddressLine1) ||
                string.IsNullOrWhiteSpace(address.City) ||
                string.IsNullOrWhiteSpace(address.State) ||
                string.IsNullOrWhiteSpace(address.Country))
            {
                return await AbortAsync(
                    CreateOrderStatus.InvalidShippingAddress);
            }

            var recipientPhone = address.RecipientPhone;

            if (string.IsNullOrWhiteSpace(recipientPhone))
            {
                recipientPhone = await userRepository.GetPhoneAsync(
                    userId,
                    cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(recipientPhone))
            {
                return await AbortAsync(
                    CreateOrderStatus.RecipientPhoneRequired);
            }

            var shippingAddress = OrderAddress.Create(
                recipientName: address.RecipientName,
                recipientPhone: recipientPhone,
                addressLine1: address.AddressLine1,
                addressLine2: address.AddressLine2,
                city: address.City,
                state: address.State,
                postalCode: address.PostalCode,
                country: address.Country);

            // This reads the cart and runs the existing checkout pricing,
            // tax and serviceability logic exactly once in this workflow.
            var checkout = await checkoutService.GetAsync(
                userId,
                address.PostalCode,
                cancellationToken);

            if (checkout.Items.Count == 0)
            {
                return await AbortAsync(CreateOrderStatus.CartEmpty);
            }

            if (!checkout.Id.HasValue || checkout.Id.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Checkout returned items without a valid cart ID.");
            }

            if (checkout.HasUnavailableItems ||
                checkout.Items.Any(item =>
                    !item.IsAvailable ||
                    !item.Mrp.HasValue ||
                    !item.UnitPrice.HasValue ||
                    !item.LineTotal.HasValue ||
                    !item.LineDiscountAmount.HasValue ||
                    !item.LineTaxAmount.HasValue ||
                    item.UnitPrice.Value < 0m ||
                    item.UnitPrice.Value > item.Mrp.Value ||
                    item.LineDiscountAmount.Value < 0m))
            {
                return await AbortAsync(
                    CreateOrderStatus.ItemsUnavailable);
            }

            var orderItems = checkout.Items
                .Select(item => OrderItem.Create(
                    productVariantId: item.ProductVariantId,
                    productName: item.ProductName,
                    sku: item.Sku,
                    imageUrl: item.ImageUrl,
                    packageQuantity: item.PackageQuantity,
                    packageUnit: item.PackageUnit,
                    quantity: item.Quantity,
                    unitMrp: item.Mrp!.Value,
                    unitSellingPrice: item.UnitPrice!.Value,
                    lineTaxAmount: item.LineTaxAmount!.Value,
                    requiresPrescription: item.RequiresPrescription))
                .ToList();

            var now = timeProvider.GetUtcNow();

            var order = Order.Create(
                orderNumber: orderNumberGenerator.Generate(now),
                userId: userId,
                shippingAddress: shippingAddress,
                currency: "INR",
                items: orderItems,
                shippingFee: checkout.ShippingFee);

            // Detect any future divergence between checkout totals and
            // the aggregate's persisted financial snapshot.
            if (order.Subtotal != checkout.Subtotal ||
                order.TaxTotal != checkout.TotalTax ||
                order.ShippingFee != checkout.ShippingFee ||
                order.DiscountTotal != checkout.DiscountTotal ||
                order.GrandTotal != checkout.GrandTotal)
            {
                throw new InvalidOperationException(
                    "Checkout totals and order totals do not match.");
            }

            // Persist within the open transaction so allocations can
            // reference the generated Order and OrderItem IDs.
            await orderRepository.AddAsync(
                order,
                cancellationToken);

            var reservationResult =
                await inventoryReservationService.ReserveForOrderAsync(
                    order,
                    address.PostalCode,
                    DateOnly.FromDateTime(now.UtcDateTime),
                    cancellationToken);

            if (reservationResult is
                OrderInventoryReservationResult.InsufficientStock)
            {
                return await AbortAsync(
                    CreateOrderStatus.InsufficientStock);
            }

            if (reservationResult is not
                OrderInventoryReservationResult.Reserved)
            {
                throw new InvalidOperationException(
                    "Unknown inventory reservation result.");
            }

            await idempotencyRepository.AddAsync(
                new OrderIdempotencyEntry(
                    UserId: userId,
                    IdempotencyKey: key,
                    RequestHash: requestHash,
                    OrderId: order.Id,
                    CreatedAtUtc: now,
                    ExpiresAtUtc: now.Add(IdempotencyLifetime)),
                cancellationToken);

            // No nested transaction in this operation.
            await cartRepository.ClearForCheckoutAsync(
                userId,
                checkout.Id.Value,
                cancellationToken);

            var response = ToDetails(order);

            await transactionManager.CommitAsync(cancellationToken);

            // Post-commit failure must not turn a successful checkout
            // into an error response.
            await InvalidateCachesAsync(userId);

            return new CreateOrderResult(
                CreateOrderStatus.Created,
                response);
        }
        catch
        {
            try
            {
                await transactionManager.RollbackAsync(
                    CancellationToken.None);
            }
            catch (Exception rollbackException)
            {
                logger.LogError(
                    rollbackException,
                    "Rollback failed while creating an order for user {UserId}.",
                    userId);
            }

            throw;
        }
    }

    private async Task<CreateOrderResult> ResolveExistingAsync(
        OrderIdempotencyEntry existing,
        string requestHash,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                existing.RequestHash,
                requestHash,
                StringComparison.Ordinal))
        {
            return new CreateOrderResult(
                CreateOrderStatus.IdempotencyConflict);
        }

        var order = await orderRepository.GetByIdAsync(
            existing.OrderId,
            cancellationToken) ?? throw new InvalidOperationException(
                "An idempotency record references a missing order.");

        return new CreateOrderResult(
            CreateOrderStatus.Replayed,
            order);
    }

    private async Task<CreateOrderResult> AbortAsync(
        CreateOrderStatus status)
    {
        await transactionManager.RollbackAsync(
            CancellationToken.None);

        return new CreateOrderResult(status);
    }

    private async Task InvalidateCachesAsync(Guid userId)
    {
        await TryInvalidateAsync(
            () => cache.RemoveAsync(
                CacheKeys.Cart(userId),
                CancellationToken.None),
            "cart",
            userId);

        await TryInvalidateAsync(
            () => cache.RemoveByTagAsync(
                CacheTags.AdminOrders,
                CancellationToken.None),
            "admin order lists",
            userId);

        await TryInvalidateAsync(
            () => cache.RemoveByTagAsync(
                CacheTags.UserOrders(userId),
                CancellationToken.None),
            "customer order lists",
            userId);
    }

    private async Task TryInvalidateAsync(
        Func<ValueTask> invalidate,
        string cacheDescription,
        Guid userId)
    {
        try
        {
            await invalidate();
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not invalidate {CacheDescription} after checkout for user {UserId}.",
                cacheDescription,
                userId);
        }
    }

    private static OrderDetailsModel ToDetails(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.Status,
            order.Currency,
            order.Subtotal,
            order.DiscountTotal,
            order.TaxTotal,
            order.ShippingFee,
            order.GrandTotal,
            new OrderAddressModel(
                order.ShippingAddress.RecipientName,
                order.ShippingAddress.RecipientPhone,
                order.ShippingAddress.AddressLine1,
                order.ShippingAddress.AddressLine2,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country),
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            order.Items.Select(item => new OrderItemDetailsModel(
                item.Id,
                item.ProductVariantId,
                item.ProductName,
                item.Sku,
                item.ImageUrl,
                item.PackageQuantity,
                item.PackageUnit,
                item.Quantity,
                item.UnitMrp,
                item.UnitSellingPrice,
                item.LineTaxAmount,
                item.LineDiscountAmount,
                item.LineSubtotal,
                item.LineTotal,
                item.RequiresPrescription)).ToList());

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

    private static bool IsValidPincode(string? pincode) =>
        pincode is { Length: 6 } &&
        pincode.All(static character =>
            character is >= '0' and <= '9');
}