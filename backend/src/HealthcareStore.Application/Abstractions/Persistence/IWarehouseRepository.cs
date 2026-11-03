using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IWarehouseRepository
{
    Task<WarehouseInsertResult> TryCreateAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken);

    Task<Warehouse?> GetForUpdateAsync(
        Guid warehouseId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<WarehouseStatus?> GetStatusAsync(
        Guid warehouseId,
        CancellationToken cancellationToken);

    Task<PagedResult<WarehouseReadModel>> GetPageForReadAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

public enum WarehouseInsertResult
{
    Created = 1,
    DuplicateCode = 2
}