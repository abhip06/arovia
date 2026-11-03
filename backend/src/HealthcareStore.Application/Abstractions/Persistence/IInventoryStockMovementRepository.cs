using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IInventoryStockMovementRepository
{
    Task<IReadOnlyList<InventoryStockMovement>> GetPageAsync(
        Guid warehouseStockId,
        int skip,
        int take,
        CancellationToken cancellationToken);
}