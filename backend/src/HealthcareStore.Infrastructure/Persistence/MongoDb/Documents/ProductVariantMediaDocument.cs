using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthcareStore.Infrastructure.Persistence.MongoDb.Documents;

public sealed class ProductVariantMediaDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("productVariantId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ProductVariantId { get; set; }

    [BsonElement("media")]
    public List<ProductMediaDocument> Media { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public sealed class ProductMediaDocument
{
    [BsonElement("id")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("altText")]
    public string? AltText { get; set; }

    [BsonElement("sortOrder")]
    public int SortOrder { get; set; }

    [BsonElement("isPrimary")]
    public bool IsPrimary { get; set; }

    [BsonElement("provider")]
    public string? Provider { get; set; }

    [BsonElement("assetId")]
    public string? AssetId { get; set; }

    [BsonElement("publicId")]
    public string? PublicId { get; set; }

    [BsonElement("url")]
    public string Url { get; set; } = null!;
}