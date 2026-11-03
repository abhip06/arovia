using HealthcareStore.Domain.Products;

namespace HealthcareStore.Api.Admin.Products.Update;

public sealed record UpdateProductApiResponse(
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