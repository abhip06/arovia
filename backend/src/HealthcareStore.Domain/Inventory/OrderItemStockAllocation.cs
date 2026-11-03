using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Inventory;

public enum OrderItemStockAllocationStatus
{
    Reserved = 1,
    Released = 2,
    Fulfilled = 3
}

public sealed class OrderItemStockAllocation : Entity
{
    private OrderItemStockAllocation()
    {
    }

    private OrderItemStockAllocation(
        Guid orderItemId,
        Guid warehouseStockId,
        int quantity,
        DateTimeOffset createdAtUtc)
    {
        OrderItemId = orderItemId;
        WarehouseStockId = warehouseStockId;
        Quantity = quantity;
        Status = OrderItemStockAllocationStatus.Reserved;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrderItemId { get; private set; }

    public Guid WarehouseStockId { get; private set; }

    public int Quantity { get; private set; }

    public OrderItemStockAllocationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    public DateTimeOffset? FulfilledAtUtc { get; private set; }

    public static OrderItemStockAllocation Create(
        Guid orderItemId,
        Guid warehouseStockId,
        int quantity,
        DateTimeOffset createdAtUtc)
    {
        if (orderItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order item ID is required.",
                nameof(orderItemId));
        }

        if (warehouseStockId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse stock ID is required.",
                nameof(warehouseStockId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Allocation quantity must be greater than zero.");
        }

        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new OrderItemStockAllocation(
            orderItemId,
            warehouseStockId,
            quantity,
            createdAtUtc);
    }

    public void Release(DateTimeOffset utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        EnsureTimestampIsNotEarlier(utcNow);

        EnsureReserved();

        Status = OrderItemStockAllocationStatus.Released;
        ReleasedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Fulfill(DateTimeOffset utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        EnsureTimestampIsNotEarlier(utcNow);

        EnsureReserved();

        Status = OrderItemStockAllocationStatus.Fulfilled;
        FulfilledAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    private void EnsureReserved()
    {
        if (Status != OrderItemStockAllocationStatus.Reserved)
        {
            throw new InvalidOperationException(
                $"A {Status} allocation cannot be released or fulfilled again.");
        }
    }

    private void EnsureTimestampIsNotEarlier(DateTimeOffset utcNow)
    {
        if (utcNow < UpdatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(utcNow),
                "Timestamp cannot be earlier than the last update.");
        }
    }

    private static void EnsureUtc(
        DateTimeOffset value,
        string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Timestamp must use UTC.",
                parameterName);
        }
    }
}