using HealthcareStore.Application.Location;
using System.Net;

namespace HealthcareStore.Application.Abstractions.Location;

public interface IPincodeContextResolver
{
    // Existing behavior: use a supplied pincode, otherwise resolve by IP.
    Task<PincodeContext?> ResolveAsync(
        string? pincode,
        IPAddress? clientIp,
        CancellationToken cancellationToken = default);

    // New behavior: use the user's primary address, otherwise resolve by IP.
    Task<PincodeContext?> ResolveCurrentAsync(
        Guid? userId,
        IPAddress? clientIp,
        CancellationToken cancellationToken = default);
}