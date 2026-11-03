namespace HealthcareStore.Application.Auth.Login;

public sealed record LoginUserResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role);