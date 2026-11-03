using HealthcareStore.Infrastructure.Persistence.MongoDb.Documents;
using MongoDB.Driver;

namespace HealthcareStore.Infrastructure.Persistence.MongoDb;

public static class MongoDbIndexInitializer
{
    public static async Task InitializeAsync(
        IMongoDatabase database,
        CancellationToken cancellationToken = default)
    {
        var productInformationCollection =
            database.GetCollection<ProductInformationDocument>(
                MongoCollectionNames.ProductInformation);

        var productIdIndex =
            new CreateIndexModel<ProductInformationDocument>(
                Builders<ProductInformationDocument>
                    .IndexKeys
                    .Ascending(x => x.ProductId),
                new CreateIndexOptions
                {
                    Name = "ux_product_information_product_id",
                    Unique = true
                });

        await productInformationCollection.Indexes.CreateOneAsync(
            productIdIndex,
            cancellationToken: cancellationToken);

        var productVariantMediaCollection =
            database.GetCollection<ProductVariantMediaDocument>(
                MongoCollectionNames.ProductVariantMedia);

        var productVariantIdIndex =
            new CreateIndexModel<ProductVariantMediaDocument>(
                Builders<ProductVariantMediaDocument>
                    .IndexKeys
                    .Ascending(x => x.ProductVariantId),
                new CreateIndexOptions
                {
                    Name = "ux_product_variant_media_product_variant_id",
                    Unique = true
                });

        await productVariantMediaCollection.Indexes.CreateOneAsync(
            productVariantIdIndex,
            cancellationToken: cancellationToken);
    }
}