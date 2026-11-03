using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Update;

public sealed record UpdateProductResponse(
    Guid Id,
    string Name,
    string Slug,
    ProductType ProductType,
    ProductStatus Status,
    Guid ManufacturerId,
    Guid? BrandId,
    bool RequiresPrescription,
    string HsnCode,
    decimal TaxRate,
    bool Returnable,
    int ReturnWindowDays);