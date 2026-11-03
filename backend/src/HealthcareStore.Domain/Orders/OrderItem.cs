using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem()
    {
    }

    private OrderItem(
        Guid productVariantId,
        string productName,
        string sku,
        string? imageUrl,
        decimal packageQuantity,
        string packageUnit,
        int quantity,
        decimal unitMrp,
        decimal unitSellingPrice,
        decimal lineTaxAmount,
        bool requiresPrescription)
    {
        ProductVariantId = productVariantId;
        ProductName = productName;
        Sku = sku;
        ImageUrl = imageUrl;
        PackageQuantity = packageQuantity;
        PackageUnit = packageUnit;
        Quantity = quantity;
        UnitMrp = unitMrp;
        UnitSellingPrice = unitSellingPrice;
        LineTaxAmount = lineTaxAmount;
        RequiresPrescription = requiresPrescription;

        LineDiscountAmount = CalculateLineDiscount(
            unitMrp,
            unitSellingPrice,
            quantity);
    }

    public Guid OrderId { get; private set; }

    public Guid ProductVariantId { get; private set; } = Guid.Empty;

    public string ProductName { get; private set; } = null!;

    public string Sku { get; private set; } = null!;

    public string? ImageUrl { get; private set; }

    public decimal PackageQuantity { get; private set; }

    public string PackageUnit { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitMrp { get; private set; }

    public decimal UnitSellingPrice { get; private set; }

    public decimal LineTaxAmount { get; private set; }

    public decimal LineDiscountAmount { get; private set; }

    public bool RequiresPrescription { get; private set; }

    // Merchandise amount before discount and tax.
    public decimal LineSubtotal =>
        RoundMoney(UnitMrp * Quantity);

    // Merchandise amount after discount, before tax.
    public decimal LineTotal =>
        RoundMoney(UnitSellingPrice * Quantity);

    public static OrderItem Create(
        Guid productVariantId,
        string productName,
        string sku,
        string? imageUrl,
        decimal packageQuantity,
        string packageUnit,
        int quantity,
        decimal unitMrp,
        decimal unitSellingPrice,
        decimal lineTaxAmount,
        bool requiresPrescription)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }

        var normalizedProductName = Required(
            productName,
            nameof(productName));

        var normalizedSku = Required(
            sku,
            nameof(sku));

        var normalizedPackageUnit = Required(
            packageUnit,
            nameof(packageUnit));

        var normalizedImageUrl = Optional(imageUrl);

        if (packageQuantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(packageQuantity),
                "Package quantity must be greater than zero.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        if (unitMrp < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitMrp),
                "MRP cannot be negative.");
        }

        if (unitSellingPrice < 0m ||
            unitSellingPrice > unitMrp)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitSellingPrice),
                "Selling price must be non-negative and no greater than MRP.");
        }

        if (lineTaxAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineTaxAmount),
                "Line tax amount cannot be negative.");
        }

        return new OrderItem(
            productVariantId,
            normalizedProductName,
            normalizedSku,
            normalizedImageUrl,
            packageQuantity,
            normalizedPackageUnit,
            quantity,
            unitMrp,
            unitSellingPrice,
            lineTaxAmount,
            requiresPrescription);
    }

    internal void AttachToOrder(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        if (OrderId != Guid.Empty && OrderId != orderId)
        {
            throw new InvalidOperationException(
                "An order item cannot belong to multiple orders.");
        }

        OrderId = orderId;
    }

    private static decimal CalculateLineDiscount(
        decimal unitMrp,
        decimal unitSellingPrice,
        int quantity)
    {
        var lineSubtotal = RoundMoney(unitMrp * quantity);

        var lineTotal = RoundMoney(
            unitSellingPrice * quantity);

        return lineSubtotal - lineTotal;
    }

    private static decimal RoundMoney(decimal amount) =>
        Math.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);

    private static string Required(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}