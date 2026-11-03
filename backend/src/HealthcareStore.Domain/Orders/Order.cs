using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Orders;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public string OrderNumber { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }

    public OrderAddress ShippingAddress { get; private set; } = null!;

    public string Currency { get; private set; } = "INR";

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal Subtotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal GrandTotal { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private Order(
        string orderNumber,
        Guid userId,
        OrderStatus status,
        OrderAddress shippingAddress,
        string currency,
        decimal shippingFee)
    {
        OrderNumber = orderNumber;
        UserId = userId;
        Status = status;
        ShippingAddress = shippingAddress;
        Currency = currency;
        ShippingFee = shippingFee;

        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static Order Create(
        string orderNumber,
        Guid userId,
        OrderAddress shippingAddress,
        string currency,
        IEnumerable<OrderItem> items,
        decimal shippingFee)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(shippingAddress);
        ArgumentNullException.ThrowIfNull(items);

        var normalizedOrderNumber = Required(
            orderNumber,
            nameof(orderNumber));

        var normalizedCurrency = NormalizeCurrency(currency);

        if (shippingFee < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shippingFee),
                "Shipping fee cannot be negative.");
        }

        var itemList = items.ToList();

        if (itemList.Count == 0)
        {
            throw new ArgumentException(
                "An order must contain at least one item.",
                nameof(items));
        }

        var order = new Order(
            normalizedOrderNumber,
            userId,
            OrderStatus.Placed,
            shippingAddress,
            normalizedCurrency,
            shippingFee);

        foreach (var item in itemList)
        {
            order.AddItem(item);
        }

        order.CalculateTotals();

        return order;
    }

    public void Confirm(DateTimeOffset utcNow) =>
        TransitionTo(OrderStatus.Confirmed, utcNow, OrderStatus.Placed);

    public void StartProcessing(DateTimeOffset utcNow) =>
        TransitionTo(OrderStatus.Processing, utcNow, OrderStatus.Confirmed);

    public void MarkShipped(DateTimeOffset utcNow) =>
        TransitionTo(OrderStatus.Shipped, utcNow, OrderStatus.Processing);

    public void MarkOutForDelivery(DateTimeOffset utcNow) =>
        TransitionTo(
            OrderStatus.OutForDelivery,
            utcNow,
            OrderStatus.Shipped);

    public void MarkDelivered(DateTimeOffset utcNow) =>
        TransitionTo(
            OrderStatus.Delivered,
            utcNow,
            OrderStatus.OutForDelivery);

    public void Cancel(DateTimeOffset utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        EnsureTimestampIsNotEarlier(utcNow);

        if (Status is not (
            OrderStatus.Placed or
            OrderStatus.Confirmed or
            OrderStatus.Processing))
        {
            throw new InvalidOperationException(
                $"An order with status '{Status}' cannot be cancelled " +
                "through this operation.");
        }

        Status = OrderStatus.Cancelled;
        UpdatedAtUtc = utcNow;
    }

    private void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(existing => existing.Id == item.Id))
        {
            throw new InvalidOperationException(
                "Duplicate order item IDs are not allowed.");
        }

        item.AttachToOrder(Id);
        _items.Add(item);
    }

    private void CalculateTotals()
    {
        Subtotal = _items.Sum(item => item.LineSubtotal);
        DiscountTotal = _items.Sum(item => item.LineDiscountAmount);
        TaxTotal = _items.Sum(item => item.LineTaxAmount);

        GrandTotal = (Subtotal - DiscountTotal) + TaxTotal + ShippingFee;
    }

    private void TransitionTo(
        OrderStatus nextStatus,
        DateTimeOffset utcNow,
        OrderStatus expectedCurrentStatus)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        EnsureTimestampIsNotEarlier(utcNow);

        if (Status != expectedCurrentStatus)
        {
            throw new InvalidOperationException(
                $"Cannot transition an order from '{Status}' to " +
                $"'{nextStatus}'. Expected current status: " +
                $"'{expectedCurrentStatus}'.");
        }

        Status = nextStatus;
        UpdatedAtUtc = utcNow;
    }

    private void EnsureTimestampIsNotEarlier(DateTimeOffset utcNow)
    {
        if (utcNow < UpdatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(utcNow),
                "The timestamp cannot be earlier than the last update.");
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

    private static string NormalizeCurrency(string currency)
    {
        var value = Required(currency, nameof(currency)).ToUpperInvariant();

        if (value.Length != 3 || !value.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                "Currency must be a three-letter currency code.",
                nameof(currency));
        }

        return value;
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }
}