namespace HealthcareStore.Api.Location;

public sealed record CurrentLocationResponse(
    string Pincode,
    string? City,
    string? State,
    string? Country,
    string Source,
    bool IsConfirmed);