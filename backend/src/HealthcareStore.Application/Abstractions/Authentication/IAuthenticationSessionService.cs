using HealthcareStore.Application.Auth.AuthSession;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Authentication;

public interface IAuthenticationSessionService
{
    Task<AuthenticationSessionResponse> CreateAsync(
        User user,
        CancellationToken cancellationToken = default);
}