namespace HealthcareStore.Application.Location;

public sealed record PincodeContext(
    string Pincode,
    PincodeSource Source,
    string? City = null,
    string? State = null,
    string? Country = null)
{
    public bool IsConfirmed =>
        Source == PincodeSource.User;
}