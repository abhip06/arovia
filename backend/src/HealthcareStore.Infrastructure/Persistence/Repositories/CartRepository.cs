using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Carts.Models;
using HealthcareStore.Domain.Carts;
using HealthcareStore.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(
    ApplicationDbContext dbContext)
    : ICartRepository
{
    private static readonly TimeSpan IdempotencyLifetime = TimeSpan.FromHours(24);

    public async Task<CartSnapshot?> GetSnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cart =
            await dbContext.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .SingleOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

        if (cart is null)
        {
            return null;
        }

        var items =
            cart.Items
                .Select(
                    static item =>
                        new CartItemSnapshot(
                            item.Id,
                            item.ProductVariantId,
                            item.Quantity))
                .ToList();

        return new CartSnapshot(
            cart.Id,
            items);
    }

    public async Task<CartMutationResult> AddItemAsync(
        Guid userId,
        Guid productVariantId,
        int quantity,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await AcquireUserLockAsync(
            userId,
            cancellationToken);

        var idempotencyResult =
            await CheckIdempotencyAsync(
                userId,
                idempotencyKey,
                requestHash,
                cancellationToken);

        if (idempotencyResult is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return idempotencyResult;
        }

        var variantExists =
            await dbContext.ProductVariants
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == productVariantId,
                    cancellationToken);

        if (!variantExists)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return new CartMutationResult(
                CartMutationStatus.NotFound);
        }

        var cart =
            await GetOrCreateCartAsync(
                userId,
                cancellationToken);

        var existingItem =
            cart.Items.SingleOrDefault(
                x =>
                    x.ProductVariantId ==
                    productVariantId);

        if (existingItem is null &&
            cart.Items.Count >= Cart.MaxDistinctItems)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return new CartMutationResult(
                CartMutationStatus.LimitExceeded);
        }

        cart.AddItem(
            productVariantId,
            quantity);

        try
        {
            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                Console.WriteLine(
                    $"Tracked entity: {entry.Metadata.ClrType.FullName}, " +
                    $"State: {entry.State}");
            }

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                Console.WriteLine(
                    $"Concurrency entity: {entry.Metadata.ClrType.FullName}");

                Console.WriteLine(
                    $"State: {entry.State}");

                foreach (var property in entry.Properties)
                {
                    Console.WriteLine(
                        $"  {property.Metadata.Name}: " +
                        $"Current={property.CurrentValue}, " +
                        $"Original={property.OriginalValue}");
                }
            }

            throw;
        }

        await transaction.CommitAsync(
            cancellationToken);

        return new CartMutationResult(
            CartMutationStatus.Applied);
    }

    public async Task<CartMutationResult> SetItemQuantityAsync(
        Guid userId,
        Guid cartItemId,
        int quantity,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await AcquireUserLockAsync(
            userId,
            cancellationToken);

        var idempotencyResult =
            await CheckIdempotencyAsync(
                userId,
                idempotencyKey,
                requestHash,
                cancellationToken);

        if (idempotencyResult is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return idempotencyResult;
        }

        var cart =
            await dbContext.Carts
                .Include(x => x.Items)
                .SingleOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

        if (cart is null)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return new CartMutationResult(
                CartMutationStatus.NotFound);
        }

        var updated =
            cart.SetItemQuantity(
                cartItemId,
                quantity);

        if (!updated)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return new CartMutationResult(
                CartMutationStatus.NotFound);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new CartMutationResult(
            CartMutationStatus.Applied);
    }

    public async Task<CartMutationResult> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await AcquireUserLockAsync(
            userId,
            cancellationToken);

        var idempotencyResult =
            await CheckIdempotencyAsync(
                userId,
                idempotencyKey,
                requestHash,
                cancellationToken);

        if (idempotencyResult is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return idempotencyResult;
        }

        var cart =
            await dbContext.Carts
                .Include(x => x.Items)
                .SingleOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

        if (cart is null)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return new CartMutationResult(
                CartMutationStatus.NotFound);
        }

        var removed =
            cart.RemoveItem(cartItemId);

        if (!removed)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return new CartMutationResult(
                CartMutationStatus.NotFound);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new CartMutationResult(
            CartMutationStatus.Applied);
    }

    private async Task<Cart> GetOrCreateCartAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var cart =
            await dbContext.Carts
                .Include(x => x.Items)
                .SingleOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = Cart.Create(userId);

        dbContext.Carts.Add(cart);

        return cart;
    }

    private async Task AcquireUserLockAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended(CAST({userId} AS text), 0));",
            cancellationToken);
    }

    private async Task<CartMutationResult?> CheckIdempotencyAsync(
        Guid userId,
        string? idempotencyKey,
        string? requestHash,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(requestHash))
        {
            throw new ArgumentException(
                "Request hash is required when an idempotency key is provided.",
                nameof(requestHash));
        }

        var now = DateTimeOffset.UtcNow;

        var record =
            await dbContext.CartIdempotencyRecords
                .SingleOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.IdempotencyKey == idempotencyKey,
                    cancellationToken);

        if (record is null)
        {
            dbContext.CartIdempotencyRecords.Add(
                new CartIdempotencyRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IdempotencyKey = idempotencyKey,
                    RequestHash = requestHash,
                    CreatedAt = now,
                    ExpiresAt =
                        now.Add(IdempotencyLifetime)
                });

            return null;
        }

        if (record.ExpiresAt <= now)
        {
            record.RequestHash = requestHash;
            record.CreatedAt = now;
            record.ExpiresAt =
                now.Add(IdempotencyLifetime);

            return null;
        }

        if (!string.Equals(
                record.RequestHash,
                requestHash,
                StringComparison.Ordinal))
        {
            return new CartMutationResult(
                CartMutationStatus.IdempotencyConflict);
        }

        return new CartMutationResult(
            CartMutationStatus.Replayed);
    }

    public Task AcquireCheckoutLockAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return AcquireUserLockAsync(
            userId,
            cancellationToken);
    }

    public async Task ClearForCheckoutAsync(
        Guid userId,
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        if (cartId == Guid.Empty)
        {
            throw new ArgumentException(
                "Cart ID is required.",
                nameof(cartId));
        }

        var now = DateTimeOffset.UtcNow;

        var updated = await dbContext.Carts
            .Where(cart =>
                cart.Id == cartId &&
                cart.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    cart => cart.UpdatedAt,
                    now),
                cancellationToken);

        if (updated != 1)
        {
            throw new InvalidOperationException(
                "The customer's cart could not be found during checkout.");
        }

        await dbContext.CartItems
            .Where(item => item.CartId == cartId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}