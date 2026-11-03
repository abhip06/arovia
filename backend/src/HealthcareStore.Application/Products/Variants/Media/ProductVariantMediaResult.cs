namespace HealthcareStore.Application.Products.Variants.Media;

public sealed record ProductVariantMediaResult(
    Guid Id,
    string? AltText,
    int SortOrder,
    bool IsPrimary,
    string Url);