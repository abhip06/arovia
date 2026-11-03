using System.Security.Claims;
using HealthcareStore.Api.Common.Authentication;
using HealthcareStore.Application.Auth.ExternalLogin;
using Microsoft.AspNetCore.Authentication;

namespace HealthcareStore.Api.Auth.Google;

public static class GoogleLinkEndpoint
{
    public static IEndpointRouteBuilder MapGoogleLinkEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/auth/google/link",
            (
                HttpContext httpContext) =>
            {
                var userIdValue =
                    httpContext.User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(
                        userIdValue,
                        out var userId))
                {
                    return Results.Unauthorized();
                }

                var properties =
                    new AuthenticationProperties
                    {
                        RedirectUri =
                            "/api/v1/auth/google/link/complete"
                    };

                properties.Items[
                    ExternalAuthenticationDefaults.LinkUserIdProperty] =
                    userId.ToString("D");

                return Results.Challenge(
                    properties,
                    [
                        ExternalAuthenticationDefaults.GoogleScheme
                    ]);
            })
            .RequireAuthorization();

        endpoints.MapGet(
            "/api/v1/auth/google/link/complete",
            async (
                HttpContext httpContext,
                LinkGoogleAccountService service,
                CancellationToken cancellationToken) =>
            {
                var result =
                    await httpContext.AuthenticateAsync(
                        ExternalAuthenticationDefaults.Scheme);

                if (!result.Succeeded ||
                    result.Principal is null ||
                    result.Properties is null)
                {
                    return Results.Unauthorized();
                }

                if (!result.Properties.Items.TryGetValue(
                        ExternalAuthenticationDefaults.LinkUserIdProperty,
                        out var userIdValue) ||
                    !Guid.TryParse(
                        userIdValue,
                        out var userId))
                {
                    await httpContext.SignOutAsync(
                        ExternalAuthenticationDefaults.Scheme);

                    return Results.BadRequest();
                }

                var providerSubject =
                    result.Principal.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                var emailVerified =
                    result.Principal.FindFirstValue(
                        "email_verified");

                if (string.IsNullOrWhiteSpace(providerSubject) ||
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

                await service.LinkAsync(
                    new LinkGoogleAccountRequest(
                        userId,
                        providerSubject),
                    cancellationToken);

                await httpContext.SignOutAsync(
                    ExternalAuthenticationDefaults.Scheme);

                return Results.Ok(
                    new
                    {
                        message =
                            "Google account linked successfully."
                    });
            });

        return endpoints;
    }
}