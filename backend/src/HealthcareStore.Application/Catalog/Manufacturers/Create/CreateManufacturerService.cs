using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Manufacturers.Create;

public sealed class CreateManufacturerService
{
    private const int MaxSlugAttempts = 100;

    private readonly IManufacturerRepository _manufacturerRepository;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ICacheService _cache;

    public CreateManufacturerService(
        IManufacturerRepository manufacturerRepository,
        ISlugGenerator slugGenerator,
        ICacheService cache)
    {
        _manufacturerRepository = manufacturerRepository;
        _slugGenerator = slugGenerator;
        _cache = cache;
    }

    public async Task<CreateManufacturerResponse> CreateAsync(
        CreateManufacturerRequest request,
        CancellationToken cancellationToken)
    {
        var baseSlug = _slugGenerator.Generate(request.Name);

        for (var attempt = 1; attempt <= MaxSlugAttempts; attempt++)
        {
            var slug = attempt == 1
                ? baseSlug
                : _slugGenerator.AppendSuffix(
                    baseSlug,
                    attempt);

            var manufacturer = Manufacturer.Create(
                request.Name,
                slug);

            var result =
                await _manufacturerRepository.TryCreateAsync(
                    manufacturer,
                    cancellationToken);

            if (result == ManufacturerInsertResult.Created)
            {
                await _cache.RemoveByTagAsync(
                    CacheTags.Manufacturers,
                    cancellationToken);

                return new CreateManufacturerResponse(
                    manufacturer.Id,
                    manufacturer.Name,
                    manufacturer.Slug,
                    manufacturer.Status);
            }
        }

        throw new ConflictException(
            "Unable to generate a unique manufacturer slug.");
    }
}