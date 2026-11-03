namespace HealthcareStore.Api.Catalog.Categories;

public sealed record CategoryTreeResponse(
    string ProductType,
    string Slug,
    string Name,
    IReadOnlyList<CategoryTreeResponse> Children);