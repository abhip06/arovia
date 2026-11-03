using HealthcareStore.Domain.Products;

namespace HealthcareStore.Api.Admin.Products.Create;

public sealed record CreateProductApiRequest(
    string Name,
    ProductType ProductType,
    Guid ManufacturerId,
    Guid? BrandId,
    bool RequiresPrescription,
    string HsnCode,
    decimal TaxRate,
    bool Returnable,
    int ReturnWindowDays);