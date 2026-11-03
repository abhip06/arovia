namespace HealthcareStore.Application.Products.Prices.Create;

public sealed record CreateProductPriceResponse(
    Guid Id,
    Guid ProductVariantId,
    decimal SellingPrice,
    DateTimeOffset EffectiveFrom);