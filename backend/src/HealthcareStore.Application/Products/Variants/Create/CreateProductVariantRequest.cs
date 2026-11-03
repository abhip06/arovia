namespace HealthcareStore.Application.Products.Variants.Create;

public sealed record CreateProductVariantRequest(
    Guid ProductId,
    string? Barcode,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit);