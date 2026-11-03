using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Create;

public sealed record CreateProductResponse(
    Guid Id,
    string Name,
    string Slug,
    ProductType ProductType,
    ProductStatus Status);