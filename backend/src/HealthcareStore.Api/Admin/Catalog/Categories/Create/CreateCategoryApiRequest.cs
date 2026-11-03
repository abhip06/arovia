using HealthcareStore.Domain.Products;

namespace HealthcareStore.Api.Admin.Catalog.Categories.Create;

public sealed record CreateCategoryApiRequest(
    ProductType ProductType,
    string Name,
    Guid? ParentCategoryId,
    int SortOrder);