using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Catalog.Brands.Delete;

public sealed class DeleteBrandService(
    IBrandRepository brandRepository,
    ICacheService cache)
{
    public async Task DeleteAsync(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        var brand =
            await brandRepository.GetForDeleteAsync(
                brandId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified brand was not found.");

        var result =
            await brandRepository.TryDeleteAsync(
                brand,
                cancellationToken);

        if (result == BrandDeleteResult.Referenced)
        {
            throw new ConflictException(
                "The brand cannot be deleted because it is being used by one or more products.");
        }

        await cache.RemoveByTagAsync(
            CacheTags.Brands,
            cancellationToken);
    }
}