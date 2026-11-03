using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Admin.Products.Variants.Media;

public sealed class ProductVariantMediaRequest
{
    public const long MaxFileSize = 5 * 1024 * 1024;

    [Required]
    public IFormFile? File { get; init; }

    [StringLength(
        500,
        MinimumLength = 1)]
    public string? AltText { get; init; }

    [Required]
    [Range(0, 100)]
    public int? SortOrder { get; init; }

    public bool IsPrimary { get; init; }
}

public sealed record ProductVariantMediaResponse(
    Guid Id,
    string? AltText,
    int SortOrder,
    bool IsPrimary,
    string Url);