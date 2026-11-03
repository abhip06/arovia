namespace HealthcareStore.Application.Catalog.Categories.Get;

public sealed record CategoryTreeItem(
    string ProductType,
    string Slug,
    string Name,
    IReadOnlyList<CategoryTreeItem> Children);