using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Variants.Create;

public sealed record CreateProductVariantResponse(
    Guid Id,
    Guid ProductId,
    string Sku,
    string? Barcode,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit,
    ProductVariantStatus Status);