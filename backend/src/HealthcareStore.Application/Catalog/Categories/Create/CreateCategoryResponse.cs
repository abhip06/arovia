using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Catalog.Categories.Create;

public sealed record CreateCategoryResponse(
    Guid Id,
    ProductType ProductType,
    string Name,
    string Slug,
    Guid? ParentCategoryId,
    CatalogEntityStatus Status,
    int SortOrder);