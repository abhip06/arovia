namespace HealthcareStore.Application.Products.Prices.Get;

public sealed record GetProductPriceHistoryResponse(
    Guid Id,
    Guid ProductVariantId,
    decimal SellingPrice,
    DateTimeOffset EffectiveFrom,
    bool IsCurrent,
    bool IsFuture);