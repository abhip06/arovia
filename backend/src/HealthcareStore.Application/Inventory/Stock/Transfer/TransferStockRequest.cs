namespace HealthcareStore.Application.Inventory.Stock.Transfer;

public sealed record TransferStockRequest(
    Guid InventoryBatchId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    int Quantity,
    string? Note);