using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Products.Prices.Get;

public sealed class GetProductPriceHistoryService(
    IProductPriceRepository priceRepository)
{
    public async Task<IReadOnlyList<GetProductPriceHistoryResponse>> GetAsync(
        Guid productVariantId,
        CancellationToken cancellationToken)
    {
        var prices = await priceRepository.GetHistoryAsync(
            productVariantId,
            cancellationToken);

        if (prices.Count == 0)
        {
            throw new NotFoundException(
                "No prices were found for the specified product variant.");
        }

        var now = DateTimeOffset.UtcNow;

        var currentPrice = prices
            .Where(x => x.EffectiveFrom <= now)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();

        return prices
            .Select(price => new GetProductPriceHistoryResponse(
                price.Id,
                price.ProductVariantId,
                price.SellingPrice,
                price.EffectiveFrom,
                currentPrice?.Id == price.Id,
                price.EffectiveFrom > now))
            .ToList();
    }
}