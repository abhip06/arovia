namespace HealthcareStore.Application.Abstractions.Media;

public interface IProductMediaStorage
{
    string Provider { get; }

    Task<ProductMediaStorage> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        Guid productVariantId,
        Guid storageId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        ProductMediaStorage media,
        CancellationToken cancellationToken);
}

public sealed record ProductMediaStorage(
    string Provider,
    string? AssetId,
    string? PublicId,
    string Url);