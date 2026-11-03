using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Products;
using HealthcareStore.Infrastructure.Persistence.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthcareStore.Infrastructure.Persistence.MongoDb.Repositories;

public sealed class ProductInformationRepository(
    IMongoDatabase database)
    : IProductInformationRepository
{
    private readonly IMongoCollection<ProductInformationDocument> _collection =
        database.GetCollection<ProductInformationDocument>(
            MongoCollectionNames.ProductInformation);

    public async Task<ProductInformation?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var document =
            await _collection
                .Find(x => x.ProductId == productId)
                .FirstOrDefaultAsync(cancellationToken);

        return document is null
            ? null
            : ToDomain(document);
    }

    public async Task CreateAsync(
        ProductInformation productInformation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productInformation);

        var document =
            ToDocument(
                productInformation,
                ObjectId.GenerateNewId());

        await _collection.InsertOneAsync(
            document,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        ProductInformation productInformation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productInformation);

        var existing =
            await _collection
                .Find(x => x.ProductId == productInformation.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            return false;
        }

        var replacement =
            ToDocument(
                productInformation,
                existing.Id);

        var result =
            await _collection.ReplaceOneAsync(
                x => x.Id == existing.Id,
                replacement,
                cancellationToken: cancellationToken);

        return result.MatchedCount == 1;
    }

    private static ProductInformationDocument ToDocument(
        ProductInformation information,
        ObjectId id)
    {
        return new ProductInformationDocument
        {
            Id = id,
            ProductId = information.ProductId,
            Description = information.Description,
            Highlights = information.Highlights.ToList(),
            Directions = information.Directions,
            Warnings = information.Warnings.ToList(),
            StorageInstructions = information.StorageInstructions,
            SideEffects = information.SideEffects.ToList(),
            HowToUse = information.HowToUse,
            Specifications =
                information.Specifications.ToDictionary(
                    static x => x.Key,
                    static x => x.Value),
            Faqs =
                information.Faqs
                    .Select(
                        static x => new ProductFaqDocument
                        {
                            Question = x.Question,
                            Answer = x.Answer
                        })
                    .ToList(),
            CreatedAt = information.CreatedAt.UtcDateTime,
            UpdatedAt = information.UpdatedAt.UtcDateTime
        };
    }

    private static ProductInformation ToDomain(
        ProductInformationDocument document)
    {
        var faqs =
            document.Faqs
                .Select(
                    static x => new ProductFaq(
                        x.Question,
                        x.Answer))
                .ToList();

        var specifications =
            document.Specifications
                .Select(
                    static x =>
                        new KeyValuePair<string, string>(
                            x.Key,
                            x.Value))
                .ToList();

        return ProductInformation.Reconstitute(
            document.ProductId,
            document.Description,
            document.Highlights,
            document.Directions,
            document.Warnings,
            document.StorageInstructions,
            document.SideEffects,
            document.HowToUse,
            specifications,
            faqs,
            new DateTimeOffset(
                document.CreatedAt.ToUniversalTime()),
            new DateTimeOffset(
                document.UpdatedAt.ToUniversalTime()));
    }

    public async Task<bool> DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result =
            await _collection.DeleteOneAsync(
                x => x.ProductId == productId,
                cancellationToken);

        return result.DeletedCount == 1;
    }
}