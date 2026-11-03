using HealthcareStore.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace HealthcareStore.Infrastructure.Caching;

public sealed class HybridCacheService(
    HybridCache cache) : ICacheService
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan expiration,
        TimeSpan? localCacheExpiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration =
                localCacheExpiration ?? expiration
        };

        return cache.GetOrCreateAsync(
            key,
            factory,
            options,
            tags,
            cancellationToken);
    }

    public ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return cache.RemoveAsync(
            key,
            cancellationToken);
    }

    public ValueTask RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        return cache.RemoveByTagAsync(
            tag,
            cancellationToken);
    }
}