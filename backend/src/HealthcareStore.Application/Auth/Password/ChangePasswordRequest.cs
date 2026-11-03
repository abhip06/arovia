namespace HealthcareStore.Application.Auth.Password;

public sealed record ChangePasswordRequest(
    Guid UserId,
    string CurrentPassword,
    string NewPassword);