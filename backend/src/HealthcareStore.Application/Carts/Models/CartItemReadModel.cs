namespace HealthcareStore.Application.Carts.Models;

public sealed record CartItemReadModel(
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
    string? ImageUrl,
    bool IsAvailable);