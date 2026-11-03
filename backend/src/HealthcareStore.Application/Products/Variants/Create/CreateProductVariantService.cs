using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Variants.Create;

public sealed class CreateProductVariantService
{
    private const int MaxSkuAttempts = 1000;

    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly ISkuGenerator _skuGenerator;

    public CreateProductVariantService(
        IProductRepository productRepository,
        IProductVariantRepository variantRepository,
        ISkuGenerator skuGenerator)
    {
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _skuGenerator = skuGenerator;
    }

    public async Task<CreateProductVariantResponse> CreateAsync(
        CreateProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        var product =
            await _productRepository.GetForVariantCreationAsync(
                request.ProductId,
                cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "The specified product was not found.");
        }

        if (product.Status == ProductStatus.Discontinued)
        {
            throw new ConflictException(
                "A variant cannot be added to a discontinued product.");
        }

        var packageUnit = NormalizeUnit(request.PackageUnit);

        var netQuantityUnit =
            string.IsNullOrWhiteSpace(request.NetQuantityUnit)
                ? null
                : NormalizeUnit(request.NetQuantityUnit);

        var identityExists =
            await _variantRepository.IdentityExistsAsync(
                request.ProductId,
                request.PackageQuantity,
                packageUnit,
                request.NetQuantity,
                netQuantityUnit,
                cancellationToken);

        if (identityExists)
        {
            throw new ConflictException(
                "This product variant already exists.");
        }

        var barcode = string.IsNullOrWhiteSpace(request.Barcode)
            ? null
            : request.Barcode.Trim();

        var baseSku = _skuGenerator.Generate(
            product.Name,
            request.PackageQuantity,
            packageUnit,
            request.NetQuantity,
            netQuantityUnit);

        for (var attempt = 1;
             attempt <= MaxSkuAttempts;
             attempt++)
        {
            var sku = attempt == 1
                ? baseSku
                : _skuGenerator.AppendSuffix(
                    baseSku,
                    attempt);

            var variant = ProductVariant.Create(
                request.ProductId,
                sku,
                barcode,
                request.PackageQuantity,
                packageUnit,
                request.NetQuantity,
                netQuantityUnit);

            var result =
                await _variantRepository.TryCreateAsync(
                    variant,
                    cancellationToken);

            switch (result)
            {
                case ProductVariantInsertResult.Created:
                    return new CreateProductVariantResponse(
                        variant.Id,
                        variant.ProductId,
                        variant.Sku,
                        variant.Barcode,
                        variant.PackageQuantity,
                        variant.PackageUnit,
                        variant.NetQuantity,
                        variant.NetQuantityUnit,
                        variant.Status);

                case ProductVariantInsertResult.DuplicateIdentity:
                    throw new ConflictException(
                        "This product variant already exists.");

                case ProductVariantInsertResult.DuplicateSku:
                    continue;

                case ProductVariantInsertResult.DuplicateBarcode:
                    throw new ConflictException(
                        "A product variant with this barcode already exists.");

                default:
                    throw new InvalidOperationException(
                        "The product variant could not be created.");
            }
        }

        throw new ConflictException(
            "Unable to generate a unique SKU.");
    }

    private static string NormalizeUnit(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}