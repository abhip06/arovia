namespace HealthcareStore.Application.Abstractions.Security;

public sealed record AccessTokenResult(
    string Token,
    int ExpiresIn);