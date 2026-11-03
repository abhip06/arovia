using HealthcareStore.Domain.Products;

namespace HealthcareStore.Api.Admin.Products.Variants.Create;

public sealed record CreateProductVariantApiResponse(
    Guid Id,
    Guid ProductId,
    string Sku,
    string? Barcode,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit,
    ProductVariantStatus Status);