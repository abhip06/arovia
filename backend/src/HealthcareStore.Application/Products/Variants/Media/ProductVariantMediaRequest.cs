namespace HealthcareStore.Application.Products.Variants.Media;

public sealed record ProductVariantMediaRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Length,
    string? AltText,
    int SortOrder,
    bool IsPrimary);