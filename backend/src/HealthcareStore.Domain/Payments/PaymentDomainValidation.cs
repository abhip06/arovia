namespace HealthcareStore.Domain.Payments;

internal static class PaymentDomainValidation
{
    public static void EnsureUtc(
        DateTimeOffset value,
        string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The timestamp must use UTC.",
                parameterName);
        }
    }

    public static void EnsureDefined<TEnum>(
        TEnum value,
        string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The supplied enum value is invalid.");
        }
    }

    public static string RequiredText(
        string? value,
        string parameterName,
        int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            value,
            parameterName);

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    public static string? OptionalText(
        string? value,
        string parameterName,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    public static string NormalizeCurrency(
        string? currency,
        string parameterName)
    {
        var normalized = RequiredText(
            currency,
            parameterName,
            maxLength: 3).ToUpperInvariant();

        if (normalized.Length != 3 ||
            normalized.Any(static character =>
                character is < 'A' or > 'Z'))
        {
            throw new ArgumentException(
                "Currency must be a three-letter ISO currency code.",
                parameterName);
        }

        return normalized;
    }

    public static string NormalizeSha256Hash(
        string? hash,
        string parameterName)
    {
        var normalized = RequiredText(
            hash,
            parameterName,
            maxLength: 64);

        if (normalized.Length != 64 ||
            normalized.Any(static character =>
                !((character >= '0' && character <= '9') ||
                  (character >= 'a' && character <= 'f'))))
        {
            throw new ArgumentException(
                "The request hash must be a lowercase SHA-256 hexadecimal hash.",
                parameterName);
        }

        return normalized;
    }

    public static void EnsureNotBefore(
        DateTimeOffset occurredAtUtc,
        DateTimeOffset createdAtUtc,
        string parameterName)
    {
        EnsureUtc(occurredAtUtc, parameterName);

        if (occurredAtUtc < createdAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "The timestamp cannot be earlier than the creation timestamp.");
        }
    }
}