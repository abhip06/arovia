namespace HealthcareStore.Api.Auth.RefreshToken;

public sealed record RefreshTokenApiResponse(
    string AccessToken,
    int ExpiresIn);