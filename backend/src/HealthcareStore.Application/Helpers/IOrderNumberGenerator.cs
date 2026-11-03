namespace HealthcareStore.Application.Helpers;

public interface IOrderNumberGenerator
{
    public string Generate(DateTimeOffset createdAtUtc);
}
