using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;

namespace HealthcareStore.Application.Catalog.Brands.Get;

public sealed class GetBrandsService(
    IBrandRepository brandRepository,
    ICacheService cache)
{
    public async Task<IReadOnlyList<BrandItem>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            CacheKeys.Brands,
            async token =>
            {
                var brands =
                    await brandRepository.GetAllForReadAsync(token);

                return brands
                    .Select(static brand => new BrandItem(
                        brand.Slug,
                        brand.Name))
                    .ToList();
            },
            expiration: TimeSpan.FromMinutes(15),
            localCacheExpiration: TimeSpan.FromSeconds(30),
            tags:
            [
                CacheTags.Brands
            ],
            cancellationToken: cancellationToken);
    }
}