using Microsoft.Extensions.Options;

namespace HealthcareStore.Api.Common.Authentication;

public sealed class RefreshTokenCookie(
    IOptions<RefreshTokenCookieOptions> options)
{
    private readonly RefreshTokenCookieOptions _options = options.Value;

    public void Append(
        HttpResponse response,
        string refreshToken,
        DateTimeOffset expiresAt)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        response.Cookies.Append(
            _options.Name,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = _options.HttpOnly,
                Secure = _options.Secure,
                SameSite = _options.SameSite,
                Path = _options.Path,
                Expires = expiresAt,
                IsEssential = true
            });
    }

    public void Delete(HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.Cookies.Delete(
            _options.Name,
            new CookieOptions
            {
                HttpOnly = _options.HttpOnly,
                Secure = _options.Secure,
                SameSite = _options.SameSite,
                Path = _options.Path
            });
    }

    public string? Get(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Cookies.TryGetValue(
            _options.Name,
            out var refreshToken)
            ? refreshToken
            : null;
    }
}