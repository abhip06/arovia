using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Location;

namespace HealthcareStore.Application.Products.Get.Detail;

public sealed class GetProductDetailService(
    IProductDetailRepository productDetailRepository,
    IProductInformationRepository productInformationRepository,
    IProductVariantMediaRepository productVariantMediaRepository,
    IWarehouseServiceabilityRepository warehouseServiceabilityRepository,
    IWarehouseStockRepository warehouseStockRepository)
{
    public async Task<ProductDetailReadModel> GetAsync(
        string productSlug,
        string sku,
        PincodeContext? pincodeContext,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productSlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        var normalizedProductSlug =
            productSlug.Trim().ToLowerInvariant();

        var normalizedSku =
            sku.Trim().ToUpperInvariant();

        var pincode =
            pincodeContext?.Pincode;

        Console.WriteLine(pincode);

        var now = DateTimeOffset.UtcNow;

        var product =
            await productDetailRepository.GetAsync(
                normalizedProductSlug,
                normalizedSku,
                now,
                cancellationToken)
            ?? throw new NotFoundException(
                "The requested product variant was not found.");

        var informationTask =
            productInformationRepository.GetAsync(
                product.ProductId,
                cancellationToken);

        var mediaTask =
            productVariantMediaRepository.GetAsync(
                product.ProductVariantId,
                cancellationToken);

        var serviceableWarehouseIdsTask =
            pincode is null
                ? null
                : warehouseServiceabilityRepository
                    .GetWarehouseIdsByPincodeAsync(
                        pincode,
                        cancellationToken);

        if (serviceableWarehouseIdsTask is null)
        {
            await Task.WhenAll(
                informationTask,
                mediaTask);
        }
        else
        {
            await Task.WhenAll(
                informationTask,
                mediaTask,
                serviceableWarehouseIdsTask);
        }

        var information =
            await informationTask;

        var media =
            await mediaTask;

        ProductDetailAvailabilityReadModel? availability = null;

        if (pincode is not null)
        {
            var warehouseIds =
                await serviceableWarehouseIdsTask!;

            var isServiceable =
                warehouseIds.Count > 0;

            var stockAvailability =
                isServiceable
                    ? await warehouseStockRepository
                        .GetAvailableProductVariantDetailAsync(
                            product.ProductVariantId,
                            warehouseIds,
                            DateOnly.FromDateTime(
                                now.UtcDateTime),
                            cancellationToken)
                    : null;

            availability =
                new ProductDetailAvailabilityReadModel(
                    pincode,
                    isServiceable,
                    stockAvailability is not null,
                    stockAvailability?.AvailableQuantity ?? 0,
                    stockAvailability?.Mrp);
        }

        var informationReadModel =
            information is null
                ? null
                : new ProductInformationReadModel(
                    information.Description,
                    information.Highlights.ToList(),
                    information.Directions,
                    information.Warnings.ToList(),
                    information.StorageInstructions,
                    information.SideEffects.ToList(),
                    information.HowToUse,
                    information.Specifications
                        .OrderBy(
                            static item => item.Key,
                            StringComparer.Ordinal)
                        .Select(static item =>
                            new ProductInformationSpecificationReadModel(
                                item.Key,
                                item.Value))
                        .ToList(),
                    information.Faqs
                        .Select(static faq =>
                            new ProductInformationFaqReadModel(
                                faq.Question,
                                faq.Answer))
                        .ToList());

        var images =
            media is null
                ? []
                : media.Media
                    .OrderBy(static image => image.SortOrder)
                    .Select(static image =>
                        new ProductDetailImageReadModel(
                            image.AltText,
                            image.SortOrder,
                            image.IsPrimary,
                            image.Url))
                    .ToList();

        return new ProductDetailReadModel(
            product,
            informationReadModel,
            availability,
            images);
    }
}