using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Catalog.Categories.Create;

public sealed record CreateCategoryRequest(
    ProductType ProductType,
    string Name,
    Guid? ParentCategoryId,
    int SortOrder);