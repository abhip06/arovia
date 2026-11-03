namespace HealthcareStore.Application.Checkout;

public sealed record CheckoutData(
    Guid? CartId,
    IReadOnlyList<CheckoutItemData> Items);

public sealed record CheckoutItemData(
    Guid CartItemId,
    Guid ProductVariantId,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal PackageQuantity,
    string PackageUnit,
    bool RequiresPrescription,
    int Quantity,
    decimal Mrp,
    decimal? SellingPrice,
    decimal TaxRate,
    int AvailableQuantity,
    string? ImageUrl,
    bool IsProductActive,
    bool IsVariantActive);