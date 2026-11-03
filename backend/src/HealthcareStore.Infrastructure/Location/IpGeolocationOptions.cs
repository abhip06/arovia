namespace HealthcareStore.Infrastructure.Location;

public sealed class IpGeolocationOptions
{
    public const string SectionName = "IpGeolocation";

    public string BaseUrl { get; init; } = "https://ipapi.co/";

    public string CountryCode { get; init; } = "IN";

    public int TimeoutSeconds { get; init; } = 2;

    public int CacheMinutes { get; init; } = 15;
}