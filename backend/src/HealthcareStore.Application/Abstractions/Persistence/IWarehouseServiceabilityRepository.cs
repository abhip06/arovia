using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IWarehouseServiceabilityRepository
{
    Task<IReadOnlyList<Guid>> GetWarehouseIdsByPincodeAsync(
        string pincode,
        CancellationToken cancellationToken);

    Task<bool> WarehouseExistsAsync(
        Guid warehouseId,
        CancellationToken cancellationToken);

    Task<int> AddMappingsAsync(
        IReadOnlyCollection<WarehouseServiceablePincode> mappings,
        CancellationToken cancellationToken);

    Task<int> RemoveMappingsAsync(
        Guid warehouseId,
        IReadOnlyCollection<string> pincodes,
        CancellationToken cancellationToken);
}