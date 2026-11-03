using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Create;

public sealed class CreateProductService
{
    private const int MaxSlugAttempts = 100;

    private readonly IProductRepository _productRepository;
    private readonly IManufacturerRepository _manufacturerRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ICacheService _cacheService;

    public CreateProductService(
        IProductRepository productRepository,
        IManufacturerRepository manufacturerRepository,
        IBrandRepository brandRepository,
        ISlugGenerator slugGenerator,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        _brandRepository = brandRepository;
        _slugGenerator = slugGenerator;
        _cacheService = cacheService;
    }

    public async Task<CreateProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var manufacturerExists =
            await _manufacturerRepository.ExistsActiveAsync(
                request.ManufacturerId,
                cancellationToken);

        if (!manufacturerExists)
        {
            throw new NotFoundException(
                "The specified manufacturer does not exist or is inactive.");
        }

        if (request.BrandId.HasValue)
        {
            var brandExists =
                await _brandRepository.ExistsActiveAsync(
                    request.BrandId.Value,
                    cancellationToken);

            if (!brandExists)
            {
                throw new NotFoundException(
                    "The specified brand does not exist or is inactive.");
            }
        }

        var normalizedName =
            Product.NormalizeName(request.Name);

        var identityExists =
            await _productRepository.IdentityExistsAsync(
                request.ProductType,
                request.ManufacturerId,
                request.BrandId,
                normalizedName,
                cancellationToken);

        if (identityExists)
        {
            throw new ConflictException(
                "This product already exists in the catalog.");
        }

        var baseSlug =
            _slugGenerator.Generate(request.Name);

        for (var attempt = 1; attempt <= MaxSlugAttempts; attempt++)
        {
            var slug = attempt == 1
                ? baseSlug
                : _slugGenerator.AppendSuffix(
                    baseSlug,
                    attempt);

            var product = Product.Create(
                request.Name,
                slug,
                request.ProductType,
                request.ManufacturerId,
                request.BrandId,
                request.RequiresPrescription,
                request.HsnCode,
                request.TaxRate,
                request.Returnable,
                request.ReturnWindowDays);

            var result =
                await _productRepository.TryCreateAsync(
                    product,
                    cancellationToken);

            switch (result)
            {
                case ProductInsertResult.Created:

                    await _cacheService.RemoveByTagAsync(
                        CacheTags.ProductCatalog,
                        cancellationToken);

                    return new CreateProductResponse(
                        product.Id,
                        product.Name,
                        product.Slug,
                        product.ProductType,
                        product.Status);

                case ProductInsertResult.DuplicateCatalogIdentity:
                    throw new ConflictException(
                        "This product already exists in the catalog.");

                case ProductInsertResult.DuplicateSlug:
                    continue;

                default:
                    throw new InvalidOperationException(
                        "The product could not be created.");
            }
        }

        throw new ConflictException(
            "Unable to generate a unique product slug.");
    }
}
