namespace HealthcareStore.Api.Common.Authentication;

public sealed class RefreshTokenCookieOptions
{
    public const string SectionName = "Authentication:RefreshTokenCookie";

    public string Name { get; init; } = "healthcare_store_refresh";

    public string Path { get; init; } = "/api/v1/auth/refresh";

    public bool HttpOnly { get; init; } = true;

    public bool Secure { get; init; } = true;

    public SameSiteMode SameSite { get; init; } = SameSiteMode.None;
}