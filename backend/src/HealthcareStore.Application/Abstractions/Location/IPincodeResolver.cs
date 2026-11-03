using System.Net;

namespace HealthcareStore.Application.Abstractions.Location;

public interface IPincodeResolver
{
    Task<IpLocation?> ResolveFromIpAsync(
        IPAddress clientIp,
        CancellationToken cancellationToken = default);
}

public sealed record IpLocation(
    string Pincode,
    string? City,
    string? State,
    string? Country);