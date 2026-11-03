using HealthcareStore.Application.Abstractions.Media;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Products;
using Microsoft.Extensions.Logging;

namespace HealthcareStore.Application.Products.Variants.Media;

public sealed class ProductVariantMediaService(
    IProductVariantRepository productVariantRepository,
    IProductVariantMediaRepository productVariantMediaRepository,
    IProductMediaStorage mediaStorage,
    ILogger<ProductVariantMediaService> logger)
{
    public async Task<ProductVariantMediaResult> UploadAsync(
        Guid productVariantId,
        ProductVariantMediaRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateProductVariantId(productVariantId);

        if (!await productVariantRepository.ExistsByIdAsync(
                productVariantId,
                cancellationToken))
        {
            throw new NotFoundException(
                "The specified product variant was not found.");
        }

        var mediaId = Guid.NewGuid();

        var storedMedia =
            await mediaStorage.UploadAsync(
                request.Content,
                request.FileName,
                request.ContentType,
                productVariantId,
                mediaId,
                cancellationToken);

        try
        {
            var media =
                ProductMedia.Create(
                    mediaId,
                    request.AltText,
                    request.SortOrder,
                    request.IsPrimary,
                    storedMedia.Provider,
                    storedMedia.AssetId,
                    storedMedia.PublicId,
                    storedMedia.Url);

            var variantMedia =
                await productVariantMediaRepository.GetAsync(
                    productVariantId,
                    cancellationToken);

            if (variantMedia is null)
            {
                variantMedia =
                    ProductVariantMedia.Create(
                        productVariantId,
                        []);

                variantMedia.AddMedia(media);

                await productVariantMediaRepository.CreateAsync(
                    variantMedia,
                    cancellationToken);
            }
            else
            {
                variantMedia.AddMedia(media);

                var updated =
                    await productVariantMediaRepository.UpdateAsync(
                        variantMedia,
                        cancellationToken);

                if (!updated)
                {
                    throw new NotFoundException(
                        "Product variant media was not found.");
                }
            }

            return Map(media);
        }
        catch
        {
            await TryDeleteStoredMediaAsync(storedMedia);

            throw;
        }
    }

    public async Task<ProductVariantMediaResult> ReplaceAsync(
        Guid productVariantId,
        Guid mediaId,
        ProductVariantMediaRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateProductVariantId(productVariantId);
        ValidateMediaId(mediaId);

        if (!await productVariantRepository.ExistsByIdAsync(
                productVariantId,
                cancellationToken))
        {
            throw new NotFoundException(
                "The specified product variant was not found.");
        }

        var variantMedia =
            await productVariantMediaRepository.GetAsync(
                productVariantId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Product variant media was not found.");

        var existingMedia =
            variantMedia.Media.FirstOrDefault(
                x => x.Id == mediaId)
            ?? throw new NotFoundException(
                "The specified media was not found.");

        var storageId = Guid.NewGuid();

        var storedMedia =
            await mediaStorage.UploadAsync(
                request.Content,
                request.FileName,
                request.ContentType,
                productVariantId,
                storageId,
                cancellationToken);

        try
        {
            var replacement =
                ProductMedia.Create(
                    mediaId,
                    request.AltText,
                    request.SortOrder,
                    request.IsPrimary,
                    storedMedia.Provider,
                    storedMedia.AssetId,
                    storedMedia.PublicId,
                    storedMedia.Url);

            variantMedia.ReplaceMedia(
                mediaId,
                replacement);

            var updated =
                await productVariantMediaRepository.UpdateAsync(
                    variantMedia,
                    cancellationToken);

            if (!updated)
            {
                throw new NotFoundException(
                    "Product variant media was not found.");
            }

            if (!string.IsNullOrWhiteSpace(existingMedia.Provider) &&
                !string.IsNullOrWhiteSpace(existingMedia.PublicId))
            {
                await TryDeleteStoredMediaAsync(
                    new ProductMediaStorage(
                        existingMedia.Provider,
                        existingMedia.AssetId,
                        existingMedia.PublicId,
                        existingMedia.Url));
            }

            return Map(replacement);
        }
        catch
        {
            await TryDeleteStoredMediaAsync(storedMedia);

            throw;
        }
    }

    public async Task DeleteAsync(
        Guid productVariantId,
        Guid mediaId,
        CancellationToken cancellationToken)
    {
        ValidateProductVariantId(productVariantId);
        ValidateMediaId(mediaId);

        var variantMedia =
            await productVariantMediaRepository.GetAsync(
                productVariantId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Product variant media was not found.");

        var existingMedia =
            variantMedia.Media.FirstOrDefault(
                x => x.Id == mediaId)
            ?? throw new NotFoundException(
                "The specified media was not found.");

        variantMedia.RemoveMedia(mediaId);

        if (variantMedia.Media.Count == 0)
        {
            var deleted =
                await productVariantMediaRepository.DeleteAsync(
                    productVariantId,
                    cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException(
                    "Product variant media was not found.");
            }
        }
        else
        {
            var updated =
                await productVariantMediaRepository.UpdateAsync(
                    variantMedia,
                    cancellationToken);

            if (!updated)
            {
                throw new NotFoundException(
                    "Product variant media was not found.");
            }
        }

        if (!string.IsNullOrWhiteSpace(existingMedia.Provider) &&
            !string.IsNullOrWhiteSpace(existingMedia.PublicId))
        {
            await TryDeleteStoredMediaAsync(
                new ProductMediaStorage(
                    existingMedia.Provider,
                    existingMedia.AssetId,
                    existingMedia.PublicId,
                    existingMedia.Url));
        }
    }

    private async Task TryDeleteStoredMediaAsync(
        ProductMediaStorage media)
    {
        if (string.IsNullOrWhiteSpace(media.Provider) ||
            string.IsNullOrWhiteSpace(media.PublicId))
        {
            return;
        }

        try
        {
            await mediaStorage.DeleteAsync(
                media,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to delete media asset {PublicId}.",
                media.PublicId);
        }
    }

    private static ProductVariantMediaResult Map(
        ProductMedia media)
    {
        return new ProductVariantMediaResult(
            media.Id,
            media.AltText,
            media.SortOrder,
            media.IsPrimary,
            media.Url);
    }

    private static void ValidateProductVariantId(
        Guid productVariantId)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }
    }

    private static void ValidateMediaId(
        Guid mediaId)
    {
        if (mediaId == Guid.Empty)
        {
            throw new ArgumentException(
                "Media ID cannot be empty.",
                nameof(mediaId));
        }
    }
}