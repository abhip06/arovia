using HealthcareStore.Application.Carts.Models;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductVariantRepository
{
    Task<bool> IdentityExistsAsync(
        Guid productId,
        decimal packageQuantity,
        string packageUnit,
        decimal? netQuantity,
        string? netQuantityUnit,
        CancellationToken cancellationToken);

    Task<bool> ExistsByIdAsync(
        Guid productVariantId,
        CancellationToken cancellationToken);

    Task<ProductVariantInsertResult> TryCreateAsync(
        ProductVariant variant,
        CancellationToken cancellationToken);

    Task<ProductVariantPriceInfo?> GetForPriceCreationAsync(
        Guid variantId,
        CancellationToken cancellationToken);

    Task<ProductVariant?> GetForUpdateAsync(
        Guid variantId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<ProductVariantStatus?> GetStatusAsync(
        Guid variantId,
        CancellationToken cancellationToken);
}

public enum ProductVariantInsertResult
{
    Created = 1,
    DuplicateIdentity = 2,
    DuplicateSku = 3,
    DuplicateBarcode = 4
}