using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Catalog.Manufacturers.Delete;

public sealed class DeleteManufacturerService(
    IManufacturerRepository manufacturerRepository,
    ICacheService cache)
{
    public async Task DeleteAsync(
        Guid manufacturerId,
        CancellationToken cancellationToken)
    {
        var manufacturer =
            await manufacturerRepository.GetForDeleteAsync(
                manufacturerId,
                cancellationToken) ?? throw new NotFoundException(
                "The specified manufacturer was not found.");

        var result =
            await manufacturerRepository.TryDeleteAsync(
                manufacturer,
                cancellationToken);

        if (result == ManufacturerDeleteResult.Referenced)
        {
            throw new ConflictException(
                "The manufacturer cannot be deleted because it is being used by one or more products.");
        }

        await cache.RemoveByTagAsync(
            CacheTags.Manufacturers,
            cancellationToken);
    }
}