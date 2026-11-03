using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<bool> IdentityExistsAsync(
        ProductType productType,
        Guid manufacturerId,
        Guid? brandId,
        string normalizedName,
        CancellationToken cancellationToken);

    Task<bool> ExistsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<ProductInsertResult> TryCreateAsync(
        Product product,
        CancellationToken cancellationToken);

    Task<ProductVariantParentInfo?> GetForVariantCreationAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<Product?> GetWithIngredientsAsync(
        Guid productId,
        CancellationToken cancellationToken);

    void AddIngredient(ProductIngredient ingredient);
    void RemoveIngredient(ProductIngredient ingredient);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<Product?> GetForUpdateAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<bool> IdentityExistsForAnotherProductAsync(
        ProductType productType,
        Guid manufacturerId,
        Guid? brandId,
        string normalizedName,
        Guid productId,
        CancellationToken cancellationToken);

    Task<ProductUpdateResult> TryUpdateAsync(
        Product product,
        CancellationToken cancellationToken);

}

public enum ProductInsertResult
{
    Created = 1,
    DuplicateCatalogIdentity = 2,
    DuplicateSlug = 3
}

public enum ProductUpdateResult
{
    Updated = 1,
    DuplicateCatalogIdentity = 2
}
