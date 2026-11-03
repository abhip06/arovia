namespace HealthcareStore.Api.Auth.Register;

public sealed record RegisterApiResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsEmailVerified);
