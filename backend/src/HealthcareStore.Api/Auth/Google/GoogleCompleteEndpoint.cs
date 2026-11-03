using HealthcareStore.Api.Common.Authentication;
using HealthcareStore.Application.Auth.AuthSession;
using HealthcareStore.Application.Auth.ExternalLogin;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace HealthcareStore.Api.Auth.Google;

public static class GoogleCompleteEndpoint
{
    public static IEndpointRouteBuilder MapGoogleCompleteEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/auth/google/complete",
            async (
                HttpContext httpContext,
                ExternalLoginUserService service,
                RefreshTokenCookie refreshTokenCookie,
                CancellationToken cancellationToken) =>
            {
                var result =
                    await httpContext.AuthenticateAsync(
                        ExternalAuthenticationDefaults.Scheme);

                if (!result.Succeeded ||
                    result.Principal is null)
                {
                    return Results.Unauthorized();
                }

                var principal = result.Principal;

                var providerSubject =
                    principal.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                var email =
                    principal.FindFirstValue(
                        ClaimTypes.Email);

                var firstName =
                    principal.FindFirstValue(
                        ClaimTypes.GivenName);

                var lastName =
                    principal.FindFirstValue(
                        ClaimTypes.Surname);

                var emailVerified =
                    principal.FindFirstValue(
                        "email_verified");

                if (string.IsNullOrWhiteSpace(providerSubject) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(lastName) ||
                    !string.Equals(
                        emailVerified,
                        "true",
                        StringComparison.OrdinalIgnoreCase))
                {
                    await httpContext.SignOutAsync(
                        ExternalAuthenticationDefaults.Scheme);

                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Google account information is incomplete."
                        });
                }

                var serviceResult =
                    await service.LoginAsync(
                        new ExternalLoginUserRequest(
                            providerSubject,
                            email,
                            firstName,
                            lastName),
                        cancellationToken);

                refreshTokenCookie.Append(
                    httpContext.Response,
                    serviceResult.RefreshToken,
                    serviceResult.RefreshTokenExpiresAt);

                await httpContext.SignOutAsync(
                    ExternalAuthenticationDefaults.Scheme);

                var apiResponse =
                    new GoogleLoginApiResponse(
                        serviceResult.AccessToken,
                        serviceResult.ExpiresIn,
                        serviceResult.UserId,
                        serviceResult.FirstName,
                        serviceResult.LastName,
                        serviceResult.Email,
                        serviceResult.Role);

                return Results.Ok(apiResponse);
            });

        return endpoints;
    }
}