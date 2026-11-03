using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Warehouses.Create;

public sealed class CreateWarehouseService(
    IWarehouseRepository warehouseRepository,
    ICacheService cache)
{
    public async Task<CreateWarehouseResponse> CreateAsync(
        CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = Warehouse.Create(
            request.Code,
            request.Name,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var result = await warehouseRepository.TryCreateAsync(
            warehouse,
            cancellationToken);

        if (result == WarehouseInsertResult.DuplicateCode)
        {
            throw new ConflictException(
                "A warehouse with this code already exists.");
        }

        await cache.RemoveByTagAsync(
            CacheTags.Warehouses,
            cancellationToken);

        return new CreateWarehouseResponse(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            warehouse.AddressLine1,
            warehouse.AddressLine2,
            warehouse.City,
            warehouse.State,
            warehouse.PostalCode,
            warehouse.Country,
            warehouse.Status);
    }
}