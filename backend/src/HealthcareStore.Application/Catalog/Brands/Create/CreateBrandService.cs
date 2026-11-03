using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Brands.Create;

public sealed class CreateBrandService
{
    private const int MaxSlugAttempts = 100;

    private readonly IBrandRepository _brandRepository;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ICacheService _cache;

    public CreateBrandService(
        IBrandRepository brandRepository,
        ISlugGenerator slugGenerator,
        ICacheService cache)
    {
        _brandRepository = brandRepository;
        _slugGenerator = slugGenerator;
        _cache = cache;
    }

    public async Task<CreateBrandResponse> CreateAsync(
        CreateBrandRequest request,
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

            var brand = Brand.Create(
                request.Name,
                slug);

            var result =
                await _brandRepository.TryCreateAsync(
                    brand,
                    cancellationToken);

            if (result == BrandInsertResult.Created)
            {
                await _cache.RemoveByTagAsync(
                    CacheTags.Brands,
                    cancellationToken);

                return new CreateBrandResponse(
                    brand.Id,
                    brand.Name,
                    brand.Slug,
                    brand.Status);
            }
        }

        throw new ConflictException(
            "Unable to generate a unique brand slug.");
    }
}