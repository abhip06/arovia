using HealthcareStore.Api.Auth.Login;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Application.Auth.Register;

namespace HealthcareStore.Api.Auth.Register;

public static class RegisterEndpoint
{
    public static IEndpointRouteBuilder MapRegisterEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/register",
            async (
                RegisterRequest request,
                RegisterUserService service,
                CancellationToken cancellationToken) =>
            {
                var registerUserRequest = new RegisterUserRequest(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password);

                var result = await service.RegisterAsync(registerUserRequest, cancellationToken);

                var response = new RegisterApiResponse(
                    result.Id,
                    result.FirstName,
                    result.LastName,
                    result.Email,
                    result.Role,
                    result.IsEmailVerified);

                return Results.Created(
                    $"/api/v1/users/{result.Id}",
                    response);
            })
            .RequireRateLimiting(RateLimitPolicies.Authentication)
            .WithName("Register")
            .Produces<RegisterApiResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status429TooManyRequests);

        return endpoints;
    }
}