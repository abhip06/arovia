using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Security;

public interface IAccessTokenGenerator
{
    AccessTokenResult Generate(User user, Guid sessionId);
}