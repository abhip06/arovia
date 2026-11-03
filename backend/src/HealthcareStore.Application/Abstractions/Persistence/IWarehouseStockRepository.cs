using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Products.Get;
using HealthcareStore.Application.Products.Get.Detail;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IWarehouseStockRepository
{
    Task<WarehouseStock?> GetForUpdateAsync(
        Guid inventoryBatchId,
        Guid warehouseId,
        CancellationToken cancellationToken);

    void Add(
        WarehouseStock warehouseStock);

    void AddMovement(
        InventoryStockMovement movement);

    Task<WarehouseStockSaveResult> TrySaveChangesAsync(
        CancellationToken cancellationToken);

    Task<WarehouseStock?> GetForReadAsync(
        Guid inventoryBatchId,
        Guid warehouseId,
        CancellationToken cancellationToken);

    Task<PagedResult<WarehouseStockListReadModel>>GetPageForWarehouseReadAsync(
        Guid warehouseId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetAvailableVariantIdsAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        IReadOnlyCollection<Guid> warehouseIds,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductCatalogAvailabilityRow>>GetAvailableCatalogVariantsAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        IReadOnlyCollection<Guid> warehouseIds,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<ProductDetailStockAvailabilityReadModel?> GetAvailableProductVariantDetailAsync(
        Guid productVariantId,
        IReadOnlyCollection<Guid> warehouseIds,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WarehouseStockReservationCandidate>>GetReservationCandidatesAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        string pincode,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<WarehouseStock?> GetForUpdateByIdAsync(
        Guid warehouseStockId,
        CancellationToken cancellationToken);
}

public enum WarehouseStockSaveResult
{
    Saved = 1,
    DuplicateStock = 2
}
