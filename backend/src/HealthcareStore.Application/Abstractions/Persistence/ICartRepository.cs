using HealthcareStore.Application.Carts.Models;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface ICartRepository
{
    Task<CartSnapshot?> GetSnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CartMutationResult> AddItemAsync(
        Guid userId,
        Guid productVariantId,
        int quantity,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken = default);

    Task<CartMutationResult> SetItemQuantityAsync(
        Guid userId,
        Guid cartItemId,
        int quantity,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken = default);

    Task<CartMutationResult> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken = default);

    Task AcquireCheckoutLockAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ClearForCheckoutAsync(
        Guid userId,
        Guid cartId,
        CancellationToken cancellationToken = default);
}

public sealed record CartSnapshot(
    Guid Id,
    IReadOnlyList<CartItemSnapshot> Items);

public sealed record CartItemSnapshot(
    Guid Id,
    Guid ProductVariantId,
    int Quantity);