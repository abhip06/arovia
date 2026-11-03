using System.Text.RegularExpressions;

namespace HealthcareStore.Application.Helpers;

public sealed class SlugGenerator : ISlugGenerator
{
    private const int MaxSlugLength = 220;

    private static readonly Regex SeparatorRegex = new(
        @"[^\p{L}\p{Nd}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Generate(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var slug = SeparatorRegex
            .Replace(value.Trim().ToLowerInvariant(), "-")
            .Trim('-');

        if (slug.Length == 0)
        {
            throw new ArgumentException(
                "A valid slug could not be generated.",
                nameof(value));
        }

        return Truncate(slug, MaxSlugLength);
    }

    public string AppendSuffix(
        string baseSlug,
        int suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseSlug);

        if (suffix < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(suffix),
                "Slug suffix must be 2 or greater.");
        }

        var suffixText = $"-{suffix}";
        var maxBaseLength = MaxSlugLength - suffixText.Length;

        var prefix = baseSlug.Length > maxBaseLength
            ? baseSlug[..maxBaseLength].TrimEnd('-')
            : baseSlug;

        return $"{prefix}{suffixText}";
    }

    private static string Truncate(
        string value,
        int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value[..maxLength].TrimEnd('-');
    }
}