using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Api.Admin.Catalog.Manufacturers.Create;

public sealed record CreateManufacturerApiResponse(
    Guid Id,
    string Name,
    string Slug,
    CatalogEntityStatus Status);