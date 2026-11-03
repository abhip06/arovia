using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Brands.Create;

public sealed record CreateBrandResponse(
    Guid Id,
    string Name,
    string Slug,
    CatalogEntityStatus Status);