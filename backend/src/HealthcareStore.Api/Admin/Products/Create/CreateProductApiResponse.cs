using HealthcareStore.Domain.Products;

namespace HealthcareStore.Api.Admin.Products.Create;

public sealed record CreateProductApiResponse(
    Guid Id,
    string Name,
    string Slug,
    ProductType ProductType,
    ProductStatus Status);