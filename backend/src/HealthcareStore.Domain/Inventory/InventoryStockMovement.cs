using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Inventory;

public sealed class InventoryStockMovement : Entity
{
    private InventoryStockMovement() { }

    private InventoryStockMovement(
        Guid warehouseStockId,
        InventoryMovementType movementType,
        int quantityDelta,
        Guid? createdByUserId,
        string? referenceType,
        Guid? referenceId,
        string? note)
    {
        if (warehouseStockId == Guid.Empty)
            throw new ArgumentException(
                "Warehouse stock ID cannot be empty.",
                nameof(warehouseStockId));

        if (quantityDelta == 0)
            throw new ArgumentException(
                "Quantity delta cannot be zero.",
                nameof(quantityDelta));

        if (createdByUserId.HasValue &&
            createdByUserId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Created by user ID cannot be empty.",
                nameof(createdByUserId));
        }

        if (referenceId.HasValue &&
            string.IsNullOrWhiteSpace(referenceType))
        {
            throw new ArgumentException(
                "Reference type is required when reference ID is provided.",
                nameof(referenceType));
        }

        WarehouseStockId = warehouseStockId;
        MovementType = movementType;
        QuantityDelta = quantityDelta;
        CreatedByUserId = createdByUserId;

        ReferenceType = string.IsNullOrWhiteSpace(referenceType)
            ? null
            : referenceType.Trim();

        ReferenceId = referenceId;

        Note = string.IsNullOrWhiteSpace(note)
            ? null
            : note.Trim();

        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid WarehouseStockId { get; private set; }

    public InventoryMovementType MovementType { get; private set; }

    public int QuantityDelta { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static InventoryStockMovement Create(
        Guid warehouseStockId,
        InventoryMovementType movementType,
        int quantityDelta,
        Guid? createdByUserId = null,
        string? referenceType = null,
        Guid? referenceId = null,
        string? note = null)
    {
        return new InventoryStockMovement(
            warehouseStockId,
            movementType,
            quantityDelta,
            createdByUserId,
            referenceType,
            referenceId,
            note);
    }
}