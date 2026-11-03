using System.Net;
using System.Net.Sockets;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HealthcareStore.Application.Abstractions.Location;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthcareStore.Infrastructure.Location;

public sealed class IpApiPincodeResolver(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IOptions<IpGeolocationOptions> options,
    ILogger<IpApiPincodeResolver> logger)
    : IPincodeResolver
{
    private readonly IpGeolocationOptions _options = options.Value;

    public async Task<IpLocation?> ResolveFromIpAsync(
        IPAddress clientIp,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientIp);

        if (!IsPublicAddress(clientIp))
        {
            return null;
        }

        var normalizedIp = NormalizeIp(clientIp).ToString();
        var cacheKey = $"pincode:ip:v1:{normalizedIp}";

        if (memoryCache.TryGetValue(
                cacheKey,
                out IpLocation? cachedLocation))
        {
            return cachedLocation;
        }

        try
        {
            var response =
                await httpClient.GetFromJsonAsync<IpApiResponse>(
                    $"{normalizedIp}/json/",
                    cancellationToken);

            if (response is null ||
                !string.Equals(
                    response.CountryCode,
                    _options.CountryCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var pincode = response.Postal?.Trim();

            if (!IsValidPincode(pincode))
            {
                return null;
            }

            var location = new IpLocation(
                pincode!,
                Normalize(response.City),
                Normalize(response.State),
                Normalize(response.Country));

            memoryCache.Set(
                cacheKey,
                location,
                TimeSpan.FromMinutes(_options.CacheMinutes));

            return location;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "IP geolocation provider request failed.");

            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "IP geolocation provider returned invalid JSON.");

            return null;
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "IP geolocation provider request timed out.");

            return null;
        }
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        address = NormalizeIp(address);

        if (IPAddress.IsLoopback(address))
        {
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();

            var first = bytes[0];
            var second = bytes[1];
            var third = bytes[2];

            return first != 0 &&
                   first != 10 &&
                   first != 127 &&
                   first < 224 &&
                   !(first == 100 && second is >= 64 and <= 127) &&
                   !(first == 169 && second == 254) &&
                   !(first == 172 && second is >= 16 and <= 31) &&
                   !(first == 192 && second == 168) &&
                   !(first == 192 && second == 0 && third is 0 or 2) &&
                   !(first == 198 &&
                     (second is 18 or 19 ||
                      (second == 51 && third == 100))) &&
                   !(first == 203 && second == 0 && third == 113);
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return !address.Equals(IPAddress.IPv6Any) &&
                   !address.IsIPv6LinkLocal &&
                   !address.IsIPv6SiteLocal &&
                   !address.IsIPv6UniqueLocal &&
                   !address.IsIPv6Multicast;
        }

        return false;
    }

    private static IPAddress NormalizeIp(IPAddress address)
    {
        return address.IsIPv4MappedToIPv6
            ? address.MapToIPv4()
            : address;
    }

    private static bool IsValidPincode(string? pincode)
    {
        return pincode is { Length: 6 } &&
               pincode.All(static digit =>
                   digit is >= '0' and <= '9');
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private sealed record IpApiResponse(
        [property: JsonPropertyName("country_code")]
        string? CountryCode,

        [property: JsonPropertyName("country_name")]
        string? Country,

        [property: JsonPropertyName("postal")]
        string? Postal,

        [property: JsonPropertyName("region")]
        string? State,

        [property: JsonPropertyName("city")]
        string? City);
}