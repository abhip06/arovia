namespace HealthcareStore.Api.Admin.Products.Prices.Get;

public sealed record GetProductPriceHistoryApiResponse(
    Guid Id,
    Guid ProductVariantId,
    decimal SellingPrice,
    DateTimeOffset EffectiveFrom,
    bool IsCurrent,
    bool IsFuture);