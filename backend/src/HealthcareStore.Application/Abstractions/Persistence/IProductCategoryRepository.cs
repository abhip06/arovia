using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductCategoryRepository
{
    Task<ProductType?> GetProductTypeAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<CategoryInfo?> GetCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken);

    Task<bool> AssignmentExistsAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken);

    Task<ProductCategoryInsertResult> TryCreateAsync(
        ProductCategory productCategory,
        CancellationToken cancellationToken);

    Task<ProductCategory?> GetAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken);

    void Remove(ProductCategory productCategory);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}

public sealed record CategoryInfo(
    ProductType ProductType,
    CatalogEntityStatus Status);

public enum ProductCategoryInsertResult
{
    Created = 1,
    Duplicate = 2
}