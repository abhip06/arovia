using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Create;

public sealed record CreateProductRequest(
    string Name,
    ProductType ProductType,
    Guid ManufacturerId,
    Guid? BrandId,
    bool RequiresPrescription,
    string HsnCode,
    decimal TaxRate,
    bool Returnable,
    int ReturnWindowDays);