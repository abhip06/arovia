using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthcareStore.Infrastructure.Persistence.MongoDb.Documents;

public sealed class ProductInformationDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("productId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ProductId { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("highlights")]
    public List<string> Highlights { get; set; } = [];

    [BsonElement("directions")]
    public string? Directions { get; set; }

    [BsonElement("warnings")]
    public List<string> Warnings { get; set; } = [];

    [BsonElement("storageInstructions")]
    public string? StorageInstructions { get; set; }

    [BsonElement("sideEffects")]
    public List<string> SideEffects { get; set; } = [];

    [BsonElement("howToUse")]
    public string? HowToUse { get; set; }

    [BsonElement("specifications")]
    public Dictionary<string, string> Specifications { get; set; } = [];

    [BsonElement("faqs")]
    public List<ProductFaqDocument> Faqs { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public sealed class ProductFaqDocument
{
    [BsonElement("question")]
    public string Question { get; set; } = null!;

    [BsonElement("answer")]
    public string Answer { get; set; } = null!;
}