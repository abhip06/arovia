using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Inventory;

public sealed class WarehouseStock : Entity
{
    private WarehouseStock() { }

    private WarehouseStock(
        Guid inventoryBatchId,
        Guid warehouseId,
        decimal costPrice,
        int quantityOnHand,
        DateTimeOffset receivedAt)
    {
        if (inventoryBatchId == Guid.Empty)
            throw new ArgumentException(
                "Inventory batch ID cannot be empty.",
                nameof(inventoryBatchId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));

        if (costPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(costPrice),
                "Cost price cannot be negative.");

        if (quantityOnHand < 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantityOnHand));

        InventoryBatchId = inventoryBatchId;
        WarehouseId = warehouseId;
        CostPrice = costPrice;
        QuantityOnHand = quantityOnHand;
        QuantityReserved = 0;
        ReceivedAt = receivedAt;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid InventoryBatchId { get; private set; }
    public Guid WarehouseId { get; private set; }

    public decimal CostPrice { get; private set; }

    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public int AvailableQuantity =>
        QuantityOnHand - QuantityReserved;

    public static WarehouseStock Create(
        Guid inventoryBatchId,
        Guid warehouseId,
        decimal costPrice,
        int quantityOnHand,
        DateTimeOffset receivedAt)
    {
        return new WarehouseStock(
            inventoryBatchId,
            warehouseId,
            costPrice,
            quantityOnHand,
            receivedAt);
    }

    public void Receive(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Received quantity must be greater than zero.");
        }

        QuantityOnHand += quantity;
        Touch();
    }

    public void Adjust(int quantityDelta)
    {
        if (quantityDelta == 0)
        {
            throw new ArgumentException(
                "Adjustment quantity cannot be zero.",
                nameof(quantityDelta));
        }

        var newQuantityOnHand =
            QuantityOnHand + quantityDelta;

        if (newQuantityOnHand < QuantityReserved)
        {
            throw new InvalidOperationException(
                "Stock adjustment cannot reduce available stock below the reserved quantity.");
        }

        QuantityOnHand = newQuantityOnHand;
        Touch();
    }

    public void TransferOut(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Transfer quantity must be greater than zero.");
        }

        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException(
                "Insufficient available stock for transfer.");
        }

        QuantityOnHand -= quantity;
        Touch();
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Reservation quantity must be greater than zero.");
        }

        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException(
                "Insufficient available stock to reserve the requested quantity.");
        }

        QuantityReserved += quantity;
        Touch();
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Release quantity must be greater than zero.");
        }

        if (QuantityReserved < quantity)
        {
            throw new InvalidOperationException(
                "Release quantity cannot exceed the reserved quantity.");
        }

        QuantityReserved -= quantity;
        Touch();
    }

    public void FulfillReserved(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Fulfilled quantity must be greater than zero.");
        }

        if (QuantityReserved < quantity)
        {
            throw new InvalidOperationException(
                "Fulfilled quantity cannot exceed the reserved quantity.");
        }

        if (QuantityOnHand < quantity)
        {
            throw new InvalidOperationException(
                "Fulfilled quantity cannot exceed the quantity on hand.");
        }

        QuantityOnHand -= quantity;
        QuantityReserved -= quantity;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}