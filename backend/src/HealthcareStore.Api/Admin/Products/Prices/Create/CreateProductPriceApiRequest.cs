namespace HealthcareStore.Api.Admin.Products.Prices.Create;

public sealed record CreateProductPriceApiRequest(
    decimal SellingPrice,
    DateTimeOffset? EffectiveFrom);