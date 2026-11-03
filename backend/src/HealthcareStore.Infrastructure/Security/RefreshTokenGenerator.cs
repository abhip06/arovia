using System.Security.Cryptography;
using System.Text;
using HealthcareStore.Application.Abstractions.Security;

namespace HealthcareStore.Infrastructure.Security;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 32;

    public string Generate()
    {
        Span<byte> tokenBytes = stackalloc byte[TokenSizeInBytes];

        RandomNumberGenerator.Fill(tokenBytes);

        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hash);
    }
}