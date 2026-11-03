using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductVariantMediaRepository
{
    Task<ProductVariantMedia?> GetAsync(
        Guid productVariantId,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetPrimaryUrlsAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        CancellationToken cancellationToken);

    Task CreateAsync(
        ProductVariantMedia variantMedia,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ProductVariantMedia variantMedia,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid productVariantId,
        CancellationToken cancellationToken);
}