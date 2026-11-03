namespace HealthcareStore.Api.Auth.Google;

public sealed record GoogleLoginApiResponse(
    string AccessToken,
    int ExpiresIn,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role);