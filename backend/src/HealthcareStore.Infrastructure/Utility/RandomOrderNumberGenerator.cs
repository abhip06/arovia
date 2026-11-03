using HealthcareStore.Application.Helpers;
using System.Security.Cryptography;

namespace HealthcareStore.Infrastructure.Utility;

public sealed class RandomOrderNumberGenerator
    : IOrderNumberGenerator
{
    public string Generate(DateTimeOffset createdAtUtc)
    {
        if (createdAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Timestamp must use UTC.",
                nameof(createdAtUtc));
        }

        var suffix = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(12));

        return $"ORD-{createdAtUtc:yyyyMMdd}-{suffix}";
    }
}