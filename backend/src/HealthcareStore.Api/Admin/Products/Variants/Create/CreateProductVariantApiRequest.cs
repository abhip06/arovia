namespace HealthcareStore.Api.Admin.Products.Variants.Create;

public sealed record CreateProductVariantApiRequest(
    string? Barcode,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit);