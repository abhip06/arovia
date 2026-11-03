using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Products.Prices.Delete;

public sealed class DeleteProductPriceService(
    IProductPriceRepository priceRepository)
{
    public async Task DeleteAsync(
        Guid priceId,
        CancellationToken cancellationToken)
    {
        var price =
            await priceRepository.GetForDeleteAsync(
                priceId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified product price was not found.");

        if (price.EffectiveFrom <= DateTimeOffset.UtcNow)
        {
            throw new ConflictException(
                "An effective or historical product price cannot be deleted.");
        }

        priceRepository.Remove(price);

        await priceRepository.SaveChangesAsync(
            cancellationToken);
    }
}