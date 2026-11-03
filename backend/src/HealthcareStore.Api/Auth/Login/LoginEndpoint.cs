using HealthcareStore.Api.Admin.Products.Create;
using HealthcareStore.Api.Common.Authentication;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Application.Auth.Login;
using ApplicationLoginRequest =
    HealthcareStore.Application.Auth.Login.LoginUserRequest;

namespace HealthcareStore.Api.Auth.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/login",
            async (
                LoginRequest request,
                LoginUserService service,
                RefreshTokenCookie refreshTokenCookie,
                HttpResponse response,
                CancellationToken cancellationToken) =>
            {
                var applicationRequest =
                    new ApplicationLoginRequest(
                        request.Email,
                        request.Password);

                var result = await service.LoginAsync(
                    applicationRequest,
                    cancellationToken);

                refreshTokenCookie.Append(
                    response,
                    result.RefreshToken,
                    result.RefreshTokenExpiresAt);

                var apiResponse = new LoginApiResponse(
                    result.AccessToken,
                    result.ExpiresIn,
                    result.UserId,
                    result.FirstName,
                    result.LastName,
                    result.Email,
                    result.Role);

                return Results.Ok(apiResponse);
            })
            .RequireRateLimiting(RateLimitPolicies.Authentication)
            .WithName("Login")
            .Produces<LoginApiResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status429TooManyRequests);

        return endpoints;
    }
}