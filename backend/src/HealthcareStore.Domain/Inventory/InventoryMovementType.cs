namespace HealthcareStore.Domain.Inventory;

public enum InventoryMovementType
{
    Received = 1,
    TransferredIn = 2,
    TransferredOut = 3,
    Sold = 4,
    Returned = 5,
    Damaged = 6,
    Expired = 7,
    Adjusted = 8
}