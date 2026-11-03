using HealthcareStore.Application.Abstractions.Caching;

namespace HealthcareStore.Application.Caching;

public sealed class CacheVerificationService(
    ICacheService cache)
{
    public async Task<string> VerifyAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            "v1:cache-verification",
            _ =>
            {
                var value =
                    $"created-{DateTimeOffset.UtcNow:O}";

                return ValueTask.FromResult(value);
            },
            expiration: TimeSpan.FromMinutes(5),
            localCacheExpiration: TimeSpan.FromSeconds(30),
            cancellationToken: cancellationToken);
    }
}