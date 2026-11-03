using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Carts.Models;
using HealthcareStore.Application.Carts.Requests;
using HealthcareStore.Application.Common.Caching;

namespace HealthcareStore.Application.Carts;

public sealed class CartService(
    ICartRepository cartRepository,
    ICartProductReader productReader,
    ICacheService cache)
{
    private static readonly TimeSpan CacheExpiration =
        TimeSpan.FromSeconds(15);

    private static readonly TimeSpan LocalCacheExpiration =
        TimeSpan.FromSeconds(1);

    public Task<CartReadModel> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return cache.GetOrCreateAsync(
            CacheKeys.Cart(userId),
            token =>
                new ValueTask<CartReadModel>(
                    BuildAsync(userId, token)),
            expiration: CacheExpiration,
            localCacheExpiration: LocalCacheExpiration,
            cancellationToken: cancellationToken)
            .AsTask();
    }

    public async Task<CartMutationResponse> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestHash =
            CartRequestHash.ForAdd(
                request.ProductVariantId,
                request.Quantity);

        var result =
            await cartRepository.AddItemAsync(
                userId,
                request.ProductVariantId,
                request.Quantity,
                idempotencyKey,
                requestHash,
                cancellationToken);

        return await CompleteMutationAsync(
            userId,
            result.Status,
            cancellationToken);
    }

    public async Task<CartMutationResponse> SetItemQuantityAsync(
        Guid userId,
        Guid cartItemId,
        SetCartItemQuantityRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestHash =
            CartRequestHash.ForSetQuantity(
                cartItemId,
                request.Quantity);

        var result =
            await cartRepository.SetItemQuantityAsync(
                userId,
                cartItemId,
                request.Quantity,
                idempotencyKey,
                requestHash,
                cancellationToken);

        return await CompleteMutationAsync(
            userId,
            result.Status,
            cancellationToken);
    }

    public async Task<CartMutationResponse> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var requestHash =
            CartRequestHash.ForRemove(
                cartItemId);

        var result =
            await cartRepository.RemoveItemAsync(
                userId,
                cartItemId,
                idempotencyKey,
                requestHash,
                cancellationToken);

        return await CompleteMutationAsync(
            userId,
            result.Status,
            cancellationToken);
    }

    private async Task<CartMutationResponse> CompleteMutationAsync(
        Guid userId,
        CartMutationStatus status,
        CancellationToken cancellationToken)
    {
        switch (status)
        {
            case CartMutationStatus.Applied:
                await cache.RemoveAsync(
                    CacheKeys.Cart(userId),
                    cancellationToken);

                var updatedCart =
                    await BuildAsync(
                        userId,
                        cancellationToken);

                return new CartMutationResponse(
                    status,
                    updatedCart);

            case CartMutationStatus.Replayed:
                var currentCart =
                    await GetAsync(
                        userId,
                        cancellationToken);

                return new CartMutationResponse(
                    status,
                    currentCart);

            case CartMutationStatus.NotFound:
            case CartMutationStatus.IdempotencyConflict:
            case CartMutationStatus.LimitExceeded:
                return new CartMutationResponse(
                    status,
                    null);

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Unknown cart mutation status.");
        }
    }

    private async Task<CartReadModel> BuildAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var snapshot =
            await cartRepository.GetSnapshotAsync(
                userId,
                cancellationToken);

        if (snapshot is null ||
            snapshot.Items.Count == 0)
        {
            return new CartReadModel(
                snapshot?.Id,
                [],
                0,
                0m,
                false);
        }

        var variantIds =
            snapshot.Items
                .Select(
                    static item =>
                        item.ProductVariantId)
                .Distinct()
                .ToArray();

        var products =
            await productReader.GetForCartAsync(
                variantIds,
                DateTimeOffset.UtcNow,
                cancellationToken);

        var productsByVariantId =
            products.ToDictionary(
                static product =>
                    product.ProductVariantId);

        var items =
            new List<CartItemReadModel>(
                snapshot.Items.Count);

        var itemCount = 0;
        var subtotal = 0m;
        var hasUnavailableItems = false;

        foreach (var item in snapshot.Items)
        {
            if (!productsByVariantId.TryGetValue(
                    item.ProductVariantId,
                    out var product))
            {
                throw new InvalidOperationException(
                    $"Cart item references missing product variant '{item.ProductVariantId}'.");
            }

            var isAvailable =
                product.CurrentSellingPrice.HasValue &&
                product.HasSellableStock;

            var unitPrice =
                isAvailable
                    ? product.CurrentSellingPrice
                    : null;

            decimal? lineTotal =
                unitPrice.HasValue
                    ? unitPrice.Value * item.Quantity
                    : null;

            if (!isAvailable)
            {
                hasUnavailableItems = true;
            }

            itemCount += item.Quantity;

            if (lineTotal.HasValue)
            {
                subtotal += lineTotal.Value;
            }

            items.Add(
                new CartItemReadModel(
                    Id: item.Id,
                    ProductVariantId:
                        item.ProductVariantId,
                    ProductId:
                        product.ProductId,
                    ProductName:
                        product.ProductName,
                    Sku:
                        product.Sku,
                    PackageQuantity:
                        product.PackageQuantity,
                    PackageUnit:
                        product.PackageUnit,
                    RequiresPrescription:
                        product.RequiresPrescription,
                    Quantity:
                        item.Quantity,
                    Mrp:
                        product.Mrp,
                    UnitPrice:
                        unitPrice,
                    LineTotal:
                        lineTotal,
                    ImageUrl:
                        product.ImageUrl,
                    IsAvailable:
                        isAvailable));
        }

        return new CartReadModel(
            snapshot.Id,
            items,
            itemCount,
            subtotal,
            hasUnavailableItems);
    }
}

public sealed record CartMutationResponse(
    CartMutationStatus Status,
    CartReadModel? Cart);