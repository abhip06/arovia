namespace HealthcareStore.Application.Abstractions.Security;

public interface IRefreshTokenGenerator
{
    string Generate();

    string Hash(string token);
}