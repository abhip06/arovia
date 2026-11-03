using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Api.Inventory.Movements.Get;

public sealed record GetInventoryMovementHistoryApiResponse(
    IReadOnlyList<InventoryMovementApiItem> Items,
    int Page,
    int PageSize,
    bool HasNextPage);

public sealed record InventoryMovementApiItem(
    Guid Id,
    Guid WarehouseStockId,
    InventoryMovementType MovementType,
    int QuantityDelta,
    Guid? CreatedByUserId,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Note,
    DateTimeOffset CreatedAt);