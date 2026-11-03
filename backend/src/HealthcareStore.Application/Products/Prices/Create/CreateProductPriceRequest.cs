namespace HealthcareStore.Application.Products.Prices.Create;

public sealed record CreateProductPriceRequest(
    Guid ProductId,
    Guid ProductVariantId,
    decimal SellingPrice,
    DateTimeOffset? EffectiveFrom);