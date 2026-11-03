namespace HealthcareStore.Application.Auth.AuthSession;

public sealed record ExternalLoginUserResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role);