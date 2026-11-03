using CloudinaryClient = CloudinaryDotNet.Cloudinary;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HealthcareStore.Application.Abstractions.Media;
using Microsoft.Extensions.Options;

namespace HealthcareStore.Infrastructure.Media.Cloudinary;

public sealed class CloudinaryProductMediaStorage(
    CloudinaryClient cloudinary,
    IOptions<CloudinaryOptions> options)
    : IProductMediaStorage
{
    private readonly CloudinaryOptions _options =
        options.Value;

    public string Provider =>
        "cloudinary";

    public async Task<ProductMediaStorage> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        Guid productVariantId,
        Guid storageId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        cancellationToken.ThrowIfCancellationRequested();

        var publicId =
            $"{_options.ProductVariantMediaFolder.TrimEnd('/')}/" +
            $"{productVariantId:N}/" +
            $"{storageId:N}";

        var uploadParameters =
            new ImageUploadParams
            {
                File =
                    new FileDescription(
                        Path.GetFileName(fileName),
                        content),
                PublicId = publicId,
                Overwrite = false,
                UniqueFilename = false
            };

        var result =
            await cloudinary.UploadAsync(
                uploadParameters);

        cancellationToken.ThrowIfCancellationRequested();

        if (result.Error is not null)
        {
            throw new InvalidOperationException(
                $"Cloudinary upload failed: {result.Error.Message}");
        }

        if (string.IsNullOrWhiteSpace(result.AssetId) ||
            string.IsNullOrWhiteSpace(result.PublicId) ||
            result.SecureUrl is null)
        {
            throw new InvalidOperationException(
                "Cloudinary returned an incomplete upload response.");
        }

        return new ProductMediaStorage(
            Provider,
            result.AssetId,
            result.PublicId,
            result.SecureUrl.ToString());
    }

    public async Task DeleteAsync(
        ProductMediaStorage media,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(media);

        if (!string.Equals(
                media.Provider,
                Provider,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Media provider '{media.Provider}' is not supported by this storage implementation.");
        }

        if (string.IsNullOrWhiteSpace(media.PublicId))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var deletionParameters =
            new DeletionParams(
                media.PublicId.ToString());

        var result =
            await cloudinary.DestroyAsync(
                deletionParameters);

        cancellationToken.ThrowIfCancellationRequested();

        if (result.Error is not null)
        {
            throw new InvalidOperationException(
                $"Cloudinary deletion failed: {result.Error.Message}");
        }
    }
}