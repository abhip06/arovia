using System.Net;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Location;

namespace HealthcareStore.Application.Abstractions.Location;

public sealed class PincodeContextResolver(
    IPincodeResolver pincodeResolver,
    IAddressRepository addressRepository)
    : IPincodeContextResolver
{
    public async Task<PincodeContext?> ResolveAsync(
        string? pincode,
        IPAddress? clientIp,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(pincode))
        {
            var normalizedPincode = pincode.Trim();

            if (!IsValidPincode(normalizedPincode))
            {
                throw new ArgumentException(
                    "A valid 6-digit pincode is required.",
                    nameof(pincode));
            }

            return new PincodeContext(
                normalizedPincode,
                PincodeSource.User);
        }

        return await ResolveFromIpAsync(
            clientIp,
            cancellationToken);
    }

    public async Task<PincodeContext?> ResolveCurrentAsync(
        Guid? userId,
        IPAddress? clientIp,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (userId.HasValue)
        {
            var savedLocation =
                await addressRepository.GetPrimaryLocationAsync(
                    userId.Value,
                    cancellationToken);

            if (savedLocation is not null &&
                IsValidPincode(savedLocation.Pincode))
            {
                return new PincodeContext(
                    savedLocation.Pincode!.Trim(),
                    PincodeSource.User,
                    Normalize(savedLocation.City),
                    Normalize(savedLocation.State),
                    Normalize(savedLocation.Country));
            }
        }

        return await ResolveFromIpAsync(
            clientIp,
            cancellationToken);
    }

    private async Task<PincodeContext?> ResolveFromIpAsync(
        IPAddress? clientIp,
        CancellationToken cancellationToken)
    {
        if (clientIp is null)
        {
            return null;
        }

        var location =
            await pincodeResolver.ResolveFromIpAsync(
                clientIp,
                cancellationToken);

        if (location is null ||
            !IsValidPincode(location.Pincode))
        {
            return null;
        }

        return new PincodeContext(
            location.Pincode.Trim(),
            PincodeSource.Ip,
            Normalize(location.City),
            Normalize(location.State),
            Normalize(location.Country));
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
}