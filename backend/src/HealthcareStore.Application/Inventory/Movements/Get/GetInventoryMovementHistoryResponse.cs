using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Movements.Get;

public sealed record GetInventoryMovementHistoryResponse(
    IReadOnlyList<InventoryMovementItem> Items,
    int Page,
    int PageSize,
    bool HasNextPage);

public sealed record InventoryMovementItem(
    Guid Id,
    Guid WarehouseStockId,
    InventoryMovementType MovementType,
    int QuantityDelta,
    Guid? CreatedByUserId,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Note,
    DateTimeOffset CreatedAt);