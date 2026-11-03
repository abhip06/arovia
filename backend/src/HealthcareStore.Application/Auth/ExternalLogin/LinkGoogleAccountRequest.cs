namespace HealthcareStore.Application.Auth.ExternalLogin;

public sealed record LinkGoogleAccountRequest(Guid UserId, string ProviderSubject);