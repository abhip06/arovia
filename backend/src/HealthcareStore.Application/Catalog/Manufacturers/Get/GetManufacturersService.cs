using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Catalog.Manufacturers.Get;
using HealthcareStore.Application.Common.Caching;

namespace HealthcareStore.Application.Catalog.Manufacturers.Get;

public sealed class GetManufacturersService(
    IManufacturerRepository manufacturerRepository,
    ICacheService cache)
{
    public async Task<IReadOnlyList<ManufacturerItem>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            CacheKeys.Manufacturers,
            async token =>
            {
                var manufacturers =
                    await manufacturerRepository.GetAllForReadAsync(token);

                return manufacturers
                    .Select(static manufacturer =>
                        new ManufacturerItem(
                            manufacturer.Slug,
                            manufacturer.Name))
                    .ToList();
            },
            expiration: TimeSpan.FromMinutes(15),
            localCacheExpiration: TimeSpan.FromSeconds(30),
            tags:
            [
                CacheTags.Manufacturers
            ],
            cancellationToken: cancellationToken);
    }
}