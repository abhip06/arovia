using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductPriceRepository
{
    Task<ProductPrice?> GetForDeleteAsync(
        Guid priceId,
        CancellationToken cancellationToken);

    void Remove(ProductPrice price);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<ProductPriceInsertResult> TryCreateAsync(
        ProductPrice price,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductPrice>> GetHistoryAsync(
        Guid productVariantId,
        CancellationToken cancellationToken);
}

public enum ProductPriceInsertResult
{
    Created = 1,
    DuplicateEffectiveFrom = 2
}