namespace HealthcareStore.Application.Checkout;

public sealed record CheckoutReadModel(
    Guid? Id,
    IReadOnlyList<CheckoutItemReadModel> Items,
    int ItemCount,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TotalTax,
    decimal ShippingFee,
    decimal GrandTotal,
    bool HasUnavailableItems);

public sealed record CheckoutItemReadModel(
    Guid Id,
    Guid ProductVariantId,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal PackageQuantity,
    string PackageUnit,
    bool RequiresPrescription,
    int Quantity,
    decimal? Mrp,
    decimal? UnitPrice,
    decimal? LineTotal,
    decimal? LineDiscountAmount,
    decimal? LineTaxAmount,
    string? ImageUrl,
    bool IsAvailable);