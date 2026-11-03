namespace HealthcareStore.Application.Auth.Register;

public sealed record RegisterUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsEmailVerified);