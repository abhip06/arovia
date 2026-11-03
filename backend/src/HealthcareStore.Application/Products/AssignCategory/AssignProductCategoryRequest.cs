namespace HealthcareStore.Application.Products.AssignCategory;

public sealed record AssignProductCategoryRequest(
    Guid ProductId,
    Guid CategoryId);