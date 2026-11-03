using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Prices.Create;

public sealed class CreateProductPriceService
{
    private readonly IProductVariantRepository _variantRepository;
    private readonly IProductPriceRepository _priceRepository;

    public CreateProductPriceService(
        IProductVariantRepository variantRepository,
        IProductPriceRepository priceRepository)
    {
        _variantRepository = variantRepository;
        _priceRepository = priceRepository;
    }

    public async Task<CreateProductPriceResponse> CreateAsync(
        CreateProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        var variant =
            await _variantRepository.GetForPriceCreationAsync(
                request.ProductVariantId,
                cancellationToken);

        if (variant is null)
        {
            throw new NotFoundException(
                "The specified product variant was not found.");
        }

        if (variant.ProductId != request.ProductId)
        {
            throw new NotFoundException(
                "The specified product variant was not found.");
        }

        if (variant.Status == ProductVariantStatus.Discontinued)
        {
            throw new ConflictException(
                "A price cannot be added to a discontinued product variant.");
        }

        var effectiveFrom =
            request.EffectiveFrom?.ToUniversalTime()
            ?? DateTimeOffset.UtcNow;

        var price = ProductPrice.Create(
            request.ProductVariantId,
            request.SellingPrice,
            effectiveFrom);

        var result =
            await _priceRepository.TryCreateAsync(
                price,
                cancellationToken);

        if (result ==
            ProductPriceInsertResult.DuplicateEffectiveFrom)
        {
            throw new ConflictException(
                "A price with this effective time already exists for the product variant.");
        }

        return new CreateProductPriceResponse(
            price.Id,
            price.ProductVariantId,
            price.SellingPrice,
            price.EffectiveFrom);
    }
}