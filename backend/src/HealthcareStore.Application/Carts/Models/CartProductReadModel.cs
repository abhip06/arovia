namespace HealthcareStore.Application.Carts.Models;

public sealed record CartProductReadModel(
    Guid ProductVariantId,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal PackageQuantity,
    string PackageUnit,
    bool RequiresPrescription,
    decimal? Mrp,
    decimal? CurrentSellingPrice,
    bool HasSellableStock,
    string? ImageUrl);