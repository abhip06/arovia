using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IManufacturerRepository
{
    Task<bool> ExistsActiveAsync(
        Guid manufacturerId,
        CancellationToken cancellationToken);

    Task<ManufacturerInsertResult> TryCreateAsync(
        Manufacturer manufacturer,
        CancellationToken cancellationToken);

    Task<Manufacturer?> GetForDeleteAsync(
        Guid manufacturerId,
        CancellationToken cancellationToken);

    Task<ManufacturerDeleteResult> TryDeleteAsync(
        Manufacturer manufacturer,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ManufacturerReadModel>> GetAllForReadAsync(
        CancellationToken cancellationToken);
}

public enum ManufacturerInsertResult
{
    Created = 1,
    DuplicateSlug = 2
}

public enum ManufacturerDeleteResult
{
    Deleted = 1,
    Referenced = 2
}