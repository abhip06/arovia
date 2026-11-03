using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Helpers;

public sealed record CategoryReadModel(
    Guid Id,
    ProductType ProductType,
    string Name,
    string Slug,
    Guid? ParentCategoryId,
    CatalogEntityStatus Status,
    int SortOrder);