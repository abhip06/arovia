namespace HealthcareStore.Application.Auth.RefreshToken;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    DateTimeOffset RefreshTokenExpiresAt);