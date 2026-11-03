namespace HealthcareStore.Application.Products.Update;

public sealed record UpdateProductRequest(
    Guid ProductId,
    string Name,
    Guid ManufacturerId,
    Guid? BrandId,
    bool RequiresPrescription,
    string HsnCode,
    decimal TaxRate,
    bool Returnable,
    int ReturnWindowDays);