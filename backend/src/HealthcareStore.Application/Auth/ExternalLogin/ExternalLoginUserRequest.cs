namespace HealthcareStore.Application.Auth.ExternalLogin;

public sealed record ExternalLoginUserRequest(
    string ProviderSubject,
    string Email,
    string FirstName,
    string LastName);