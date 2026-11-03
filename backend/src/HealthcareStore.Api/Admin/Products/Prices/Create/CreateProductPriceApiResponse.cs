namespace HealthcareStore.Api.Admin.Products.Prices.Create;

public sealed record CreateProductPriceApiResponse(
    Guid Id,
    Guid ProductVariantId,
    decimal SellingPrice,
    DateTimeOffset EffectiveFrom);