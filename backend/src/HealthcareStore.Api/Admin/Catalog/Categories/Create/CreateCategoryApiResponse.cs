using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Api.Admin.Catalog.Categories.Create;

public sealed record CreateCategoryApiResponse(
    Guid Id,
    ProductType ProductType,
    string Name,
    string Slug,
    Guid? ParentCategoryId,
    CatalogEntityStatus Status,
    int SortOrder);