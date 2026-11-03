using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IBrandRepository
{
    Task<bool> ExistsActiveAsync(
        Guid brandId,
        CancellationToken cancellationToken);

    Task<BrandInsertResult> TryCreateAsync(
        Brand brand,
        CancellationToken cancellationToken);

    Task<Brand?> GetForDeleteAsync(
        Guid brandId,
        CancellationToken cancellationToken);

    Task<BrandDeleteResult> TryDeleteAsync(
        Brand brand,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BrandReadModel>> GetAllForReadAsync(
        CancellationToken cancellationToken);
}

public enum BrandInsertResult
{
    Created = 1,
    DuplicateSlug = 2
}

public enum BrandDeleteResult
{
    Deleted = 1,
    Referenced = 2
}