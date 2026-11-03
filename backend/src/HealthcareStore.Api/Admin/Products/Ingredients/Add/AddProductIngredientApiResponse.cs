using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Api.Admin.Products.Ingredients.Add;

public sealed record AddProductIngredientApiResponse(
    Guid Id,
    Guid ProductId,
    Guid ActiveIngredientId,
    decimal StrengthValue,
    string StrengthUnit,
    IngredientRole Role);