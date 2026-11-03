using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Api.Admin.Catalog.Brands.Create;

public sealed record CreateBrandApiResponse(
    Guid Id,
    string Name,
    string Slug,
    CatalogEntityStatus Status);