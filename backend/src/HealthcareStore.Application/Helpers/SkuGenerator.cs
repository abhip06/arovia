using System.Globalization;
using System.Text.RegularExpressions;

namespace HealthcareStore.Application.Helpers;

public sealed class SkuGenerator : ISkuGenerator
{
    private const int MaxSkuLength = 100;

    private static readonly Regex TokenRegex = new(
        @"[\p{L}\p{Nd}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex NumberRegex = new(
        @"\d+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Generate(
        string productName,
        decimal packageQuantity,
        string packageUnit,
        decimal? netQuantity,
        string? netQuantityUnit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageUnit);

        var tokens = TokenRegex
            .Matches(productName)
            .Select(x => x.Value)
            .ToArray();

        var firstWord = tokens
            .FirstOrDefault(x => x.Any(char.IsLetter));

        var prefix = firstWord is null
            ? "PROD"
            : new string(
                firstWord
                    .Where(char.IsLetter)
                    .Take(4)
                    .Select(char.ToUpperInvariant)
                    .ToArray());

        var parts = new List<string>
        {
            prefix
        };

        var numbers = NumberRegex
            .Matches(productName)
            .Select(x => x.Value)
            .Distinct()
            .Take(2);

        parts.AddRange(numbers);

        parts.Add(GetUnitCode(packageUnit));
        parts.Add(FormatQuantity(packageQuantity));

        if (netQuantity.HasValue &&
            !string.IsNullOrWhiteSpace(netQuantityUnit))
        {
            parts.Add(
                $"{FormatQuantity(netQuantity.Value)}" +
                GetUnitCode(netQuantityUnit));
        }

        var sku = string.Join("-", parts);

        return Truncate(sku);
    }

    public string AppendSuffix(
        string baseSku,
        int suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseSku);

        if (suffix < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(suffix),
                "SKU suffix must be 2 or greater.");
        }

        var suffixText = $"-{suffix}";
        var maxBaseLength = MaxSkuLength - suffixText.Length;

        var prefix = baseSku.Length > maxBaseLength
            ? baseSku[..maxBaseLength].TrimEnd('-')
            : baseSku;

        return $"{prefix}{suffixText}";
    }

    private static string GetUnitCode(string unit)
    {
        return unit.Trim().ToLowerInvariant() switch
        {
            "tablet" or "tablets" => "TAB",
            "capsule" or "capsules" => "CAP",
            "bottle" or "bottles" => "BTL",
            "strip" or "strips" => "STR",
            "box" or "boxes" => "BOX",
            "pack" or "packs" => "PK",
            "piece" or "pieces" => "PC",
            "milligram" or "milligrams" or "mg" => "MG",
            "gram" or "grams" or "g" => "G",
            "kilogram" or "kilograms" or "kg" => "KG",
            "milliliter" or "milliliters" or "ml" => "ML",
            "liter" or "liters" or "l" => "L",
            _ => CreateFallbackCode(unit)
        };
    }

    private static string CreateFallbackCode(string unit)
    {
        var code = new string(
            unit
                .Trim()
                .Where(char.IsLetterOrDigit)
                .Take(4)
                .Select(char.ToUpperInvariant)
                .ToArray());

        return string.IsNullOrEmpty(code)
            ? "UNIT"
            : code;
    }

    private static string FormatQuantity(decimal quantity)
    {
        var value = quantity.ToString(
            "0.###",
            CultureInfo.InvariantCulture);

        return value.Replace('.', 'P');
    }

    private static string Truncate(string value)
    {
        return value.Length <= MaxSkuLength
            ? value
            : value[..MaxSkuLength].TrimEnd('-');
    }
}