using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Manufacturers.Create;

public sealed record CreateManufacturerResponse(
    Guid Id,
    string Name,
    string Slug,
    CatalogEntityStatus Status);