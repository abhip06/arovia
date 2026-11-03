using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Api.Admin.Catalog.ActiveIngredients.Create;

public sealed record CreateActiveIngredientApiResponse(
    Guid Id,
    string Name,
    string Slug,
    CatalogEntityStatus Status);