using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Inventory;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class InventoryStockMovementRepository(
    ApplicationDbContext dbContext)
    : IInventoryStockMovementRepository
{
    public Task<IReadOnlyList<InventoryStockMovement>> GetPageAsync(
    Guid warehouseStockId,
    int skip,
    int take,
    CancellationToken cancellationToken)
    {
        return dbContext.InventoryStockMovements
            .AsNoTracking()
            .Where(x => x.WarehouseStockId == warehouseStockId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ContinueWith(
                task => (IReadOnlyList<InventoryStockMovement>)task.Result,
                cancellationToken);
    }
}