using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Inventory.Warehouses.Serviceable;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Warehouses.Serviceable;

public sealed class ManageWarehouseServiceabilityService(
    IWarehouseServiceabilityRepository repository,
    ICacheService cacheService)
{
    private const int MaxPincodesPerRequest = 50;

    public async Task<AddWarehouseServiceablePincodesResult> AddAsync(
        Guid warehouseId,
        AddWarehouseServiceablePincodesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));
        }

        ArgumentNullException.ThrowIfNull(request.Pincodes);

        if (request.Pincodes.Count == 0)
        {
            throw new ArgumentException(
                "At least one pincode is required.",
                nameof(request.Pincodes));
        }

        if (request.Pincodes.Count > MaxPincodesPerRequest)
        {
            throw new ArgumentException(
                $"A maximum of {MaxPincodesPerRequest} pincodes can be added in one request.",
                nameof(request.Pincodes));
        }

        if (!await repository.WarehouseExistsAsync(
                warehouseId,
                cancellationToken))
        {
            throw new NotFoundException(
                "Warehouse was not found.");
        }

        var mappings = new Dictionary<string, WarehouseServiceablePincode>(
            StringComparer.Ordinal);

        foreach (var pincode in request.Pincodes)
        {
            var mapping =
                WarehouseServiceablePincode.Create(
                    warehouseId,
                    pincode);

            mappings.TryAdd(
                mapping.Pincode,
                mapping);
        }

        var uniqueMappings =
            mappings.Values.ToArray();

        var addedCount =
            await repository.AddMappingsAsync(
                uniqueMappings,
                cancellationToken);

        if (addedCount > 0)
        {
            await InvalidateServiceabilityCacheAsync(
                uniqueMappings
                    .Select(static mapping => mapping.Pincode),
                cancellationToken);
        }

        return new AddWarehouseServiceablePincodesResult(
            uniqueMappings.Length,
            addedCount);
    }

    public async Task<RemoveWarehouseServiceablePincodeResult> RemoveAsync(
        Guid warehouseId,
        string pincode,
        CancellationToken cancellationToken = default)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));
        }

        var mapping =
            WarehouseServiceablePincode.Create(
                warehouseId,
                pincode);

        if (!await repository.WarehouseExistsAsync(
                warehouseId,
                cancellationToken))
        {
            throw new NotFoundException(
                "Warehouse was not found.");
        }

        var removedCount =
            await repository.RemoveMappingsAsync(
                warehouseId,
                [mapping.Pincode],
                cancellationToken);

        if (removedCount > 0)
        {
            await InvalidateServiceabilityCacheAsync(
                [mapping.Pincode],
                cancellationToken);
        }

        return new RemoveWarehouseServiceablePincodeResult(
            mapping.Pincode,
            removedCount > 0);
    }

    private async Task InvalidateServiceabilityCacheAsync(
        IEnumerable<string> pincodes,
        CancellationToken cancellationToken)
    {
        foreach (var pincode in pincodes)
        {
            await cacheService.RemoveAsync(
                CacheKeys.WarehouseServiceability(pincode),
                cancellationToken);
        }
    }
}