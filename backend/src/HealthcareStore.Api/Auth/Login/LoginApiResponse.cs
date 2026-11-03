namespace HealthcareStore.Api.Auth.Login;

public sealed record LoginApiResponse(
    string AccessToken,
    int ExpiresIn,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role);