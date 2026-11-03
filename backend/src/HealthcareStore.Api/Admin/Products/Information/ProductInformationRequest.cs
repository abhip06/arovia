using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Admin.Products.Information;

public sealed class ProductInformationRequest
{
    [Required]
    [StringLength(
        10_000,
        MinimumLength = 1)]
    public string Description { get; init; } = null!;

    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public IReadOnlyList<string> Highlights { get; init; } = [];

    [Required]
    [StringLength(
        10_000,
        MinimumLength = 1)]
    public string Directions { get; init; } = null!;

    [MaxLength(20)]
    public IReadOnlyList<string>? Warnings { get; init; }

    [StringLength(
        5_000,
        MinimumLength = 1)]
    public string? StorageInstructions { get; init; }

    [MaxLength(20)]
    public IReadOnlyList<string>? SideEffects { get; init; }

    [Required]
    [StringLength(
        10_000,
        MinimumLength = 1)]
    public string HowToUse { get; init; } = null!;

    [MaxLength(50)]
    public IReadOnlyDictionary<string, string>? Specifications { get; init; }

    [MaxLength(20)]
    public IReadOnlyList<ProductFaqRequest>? Faqs { get; init; }
}

public sealed class ProductFaqRequest
{
    [Required]
    [StringLength(
        500,
        MinimumLength = 1)]
    public string Question { get; init; } = null!;

    [Required]
    [StringLength(
        5_000,
        MinimumLength = 1)]
    public string Answer { get; init; } = null!;
}