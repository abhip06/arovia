using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.AddIngredient;

public sealed record AddProductIngredientResponse(
    Guid Id,
    Guid ProductId,
    Guid ActiveIngredientId,
    decimal StrengthValue,
    string StrengthUnit,
    IngredientRole Role);