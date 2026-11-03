namespace HealthcareStore.Application.Auth.Password;

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);