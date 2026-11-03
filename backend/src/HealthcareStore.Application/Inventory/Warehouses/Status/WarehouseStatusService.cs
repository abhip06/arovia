using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Warehouses.Status;

public sealed class WarehouseStatusService(
    IWarehouseRepository warehouseRepository,
    ICacheService cache)
{
    public Task ActivateAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            warehouseId,
            warehouse => warehouse.Activate(),
            cancellationToken);
    }

    public Task DeactivateAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            warehouseId,
            warehouse => warehouse.Deactivate(),
            cancellationToken);
    }

    private async Task ChangeStatusAsync(
        Guid warehouseId,
        Action<Warehouse> changeStatus,
        CancellationToken cancellationToken)
    {
        var warehouse =
            await warehouseRepository.GetForUpdateAsync(
                warehouseId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified warehouse was not found.");

        try
        {
            changeStatus(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await warehouseRepository.SaveChangesAsync(
            cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Warehouses,
            cancellationToken);
    }
}