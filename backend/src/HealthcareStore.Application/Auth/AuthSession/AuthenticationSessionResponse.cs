namespace HealthcareStore.Application.Auth.AuthSession;

public sealed record AuthenticationSessionResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);