namespace HealthcareStore.Application.Auth.VerifyEmail;

public sealed record VerifyEmailResponse(
    Guid UserId,
    string Email,
    bool IsEmailVerified);