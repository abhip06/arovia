using System.Security.Cryptography;
using System.Text;

namespace HealthcareStore.Application.Common.Caching;

public static class CacheKeyFingerprint
{
    public static string Create(params object?[] values)
    {
        var canonicalValue = string.Join(
            "|",
            values.Select(Normalize));

        var bytes = Encoding.UTF8.GetBytes(canonicalValue);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    private static string Normalize(object? value)
    {
        return value switch
        {
            null => "<null>",
            string text => text.Trim().ToLowerInvariant(),
            Guid id => id.ToString("N"),
            DateTimeOffset dateTime =>
                dateTime.ToUniversalTime().ToString("O"),
            DateOnly date => date.ToString("yyyy-MM-dd"),
            bool boolean => boolean ? "true" : "false",
            _ => Convert.ToString(
                value,
                System.Globalization.CultureInfo.InvariantCulture)
                ?? string.Empty
        };
    }
}