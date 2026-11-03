namespace HealthcareStore.Application.Abstractions.Security;

public interface IAccessTokenSessionValidator
{
    Task<bool> IsActiveAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}