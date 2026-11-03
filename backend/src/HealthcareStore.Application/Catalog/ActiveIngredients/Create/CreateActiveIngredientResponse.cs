using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.ActiveIngredients.Create;

public sealed record CreateActiveIngredientResponse(
    Guid Id,
    string Name,
    string Slug,
    CatalogEntityStatus Status);