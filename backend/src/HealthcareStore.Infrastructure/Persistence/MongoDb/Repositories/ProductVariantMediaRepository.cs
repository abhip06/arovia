using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Products;
using HealthcareStore.Infrastructure.Persistence.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthcareStore.Infrastructure.Persistence.MongoDb.Repositories;

public sealed class ProductVariantMediaRepository(
    IMongoDatabase database)
    : IProductVariantMediaRepository
{
    private readonly IMongoCollection<ProductVariantMediaDocument> _collection =
        database.GetCollection<ProductVariantMediaDocument>(
            MongoCollectionNames.ProductVariantMedia);

    public async Task<ProductVariantMedia?> GetAsync(
        Guid productVariantId,
        CancellationToken cancellationToken)
    {
        var document =
            await _collection
                .Find(x => x.ProductVariantId == productVariantId)
                .FirstOrDefaultAsync(cancellationToken);

        return document is null
            ? null
            : ToDomain(document);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetPrimaryUrlsAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productVariantIds);

        if (productVariantIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var ids = productVariantIds
            .Where(static id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var documents =
            await _collection
                .Find(
                    Builders<ProductVariantMediaDocument>.Filter
                        .In(x => x.ProductVariantId, ids))
                .Project(
                    x => new
                    {
                        x.ProductVariantId,
                        x.Media
                    })
                .ToListAsync(cancellationToken);

        return documents
            .Select(
                static document =>
                {
                    var imageUrl =
                        document.Media
                            .Where(
                                static media =>
                                    !string.IsNullOrWhiteSpace(media.Url))
                            .OrderByDescending(
                                static media => media.IsPrimary)
                            .ThenBy(
                                static media => media.SortOrder)
                            .Select(
                                static media => media.Url)
                            .FirstOrDefault();

                    return new
                    {
                        document.ProductVariantId,
                        ImageUrl = imageUrl
                    };
                })
            .Where(
                static x =>
                    !string.IsNullOrWhiteSpace(x.ImageUrl))
            .ToDictionary(
                static x => x.ProductVariantId,
                static x => x.ImageUrl!);
    }

    public async Task CreateAsync(
        ProductVariantMedia variantMedia,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(variantMedia);

        var document =
            ToDocument(
                variantMedia,
                ObjectId.GenerateNewId());

        await _collection.InsertOneAsync(
            document,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        ProductVariantMedia variantMedia,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(variantMedia);

        var existing =
            await _collection
                .Find(
                    x => x.ProductVariantId ==
                         variantMedia.ProductVariantId)
                .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            return false;
        }

        var replacement =
            ToDocument(
                variantMedia,
                existing.Id);

        var result =
            await _collection.ReplaceOneAsync(
                x => x.Id == existing.Id,
                replacement,
                cancellationToken: cancellationToken);

        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteAsync(
        Guid productVariantId,
        CancellationToken cancellationToken)
    {
        var result =
            await _collection.DeleteOneAsync(
                x => x.ProductVariantId == productVariantId,
                cancellationToken);

        return result.DeletedCount == 1;
    }

    private static ProductVariantMediaDocument ToDocument(
        ProductVariantMedia variantMedia,
        ObjectId id)
    {
        return new ProductVariantMediaDocument
        {
            Id = id,
            ProductVariantId = variantMedia.ProductVariantId,
            Media =
                variantMedia.Media
                    .Select(
                        static x => new ProductMediaDocument
                        {
                            Id = x.Id,
                            AltText = x.AltText,
                            SortOrder = x.SortOrder,
                            IsPrimary = x.IsPrimary,
                            Provider = x.Provider,
                            AssetId = x.AssetId,
                            PublicId = x.PublicId,
                            Url = x.Url
                        })
                    .ToList(),
            CreatedAt = variantMedia.CreatedAt.UtcDateTime,
            UpdatedAt = variantMedia.UpdatedAt.UtcDateTime
        };
    }

    private static ProductVariantMedia ToDomain(
        ProductVariantMediaDocument document)
    {
        var media =
            document.Media
                .Select(
                    static x => ProductMedia.Reconstitute(
                        x.Id,
                        x.AltText,
                        x.SortOrder,
                        x.IsPrimary,
                        x.Provider,
                        x.AssetId,
                        x.PublicId,
                        x.Url))
                .ToList();

        return ProductVariantMedia.Reconstitute(
            document.ProductVariantId,
            media,
            new DateTimeOffset(
                document.CreatedAt.ToUniversalTime()),
            new DateTimeOffset(
                document.UpdatedAt.ToUniversalTime()));
    }
}