namespace HealthcareStore.Application.Location;

public sealed record SavedAddressLocation(
    string? Pincode,
    string? City,
    string? State,
    string? Country);