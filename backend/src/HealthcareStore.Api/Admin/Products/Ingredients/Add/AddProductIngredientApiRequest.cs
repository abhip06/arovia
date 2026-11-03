using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Api.Admin.Products.Ingredients.Add;

public sealed record AddProductIngredientApiRequest(
    Guid ActiveIngredientId,
    decimal StrengthValue,
    string StrengthUnit,
    IngredientRole Role);