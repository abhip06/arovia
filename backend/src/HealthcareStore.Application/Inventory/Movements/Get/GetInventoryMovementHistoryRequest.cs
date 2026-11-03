namespace HealthcareStore.Application.Inventory.Movements.Get;

public sealed record GetInventoryMovementHistoryRequest(
    Guid WarehouseStockId,
    int Page);