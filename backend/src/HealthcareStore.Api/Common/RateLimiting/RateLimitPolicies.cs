using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace HealthcareStore.Api.Common.RateLimiting;

public static class RateLimitPolicies
{
    public const string GeneralApi = "GeneralApi";
    public const string Authentication = "Authentication";
    public const string PasswordReset = "PasswordReset";
    public const string CartRead = "CartRead";
    public const string CartWrite = "CartWrite";
    public const string Orders = "Orders";

    public static void AddPolicies(
        RateLimiterOptions options)
    {
        options.AddPolicy(
            GeneralApi,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        GetIpAddress(httpContext),
                    factory => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

        options.AddPolicy(
            Authentication,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        GetIpAddress(httpContext),
                    factory => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

        options.AddPolicy(
            PasswordReset,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        GetIpAddress(httpContext),
                    factory => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

        options.AddPolicy(
            CartRead,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        GetUserPartitionKey(httpContext),
                    factory => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

        options.AddPolicy(
            CartWrite,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        GetUserPartitionKey(httpContext),
                    factory => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

        options.AddPolicy(
            Orders,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserPartitionKey(httpContext),
                    factory => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
    }

    private static string GetIpAddress(
        HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }

    private static string GetUserPartitionKey(
        HttpContext httpContext)
    {
        var userId =
            httpContext.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");

        return userId is not null
            ? $"user:{userId}"
            : $"ip:{GetIpAddress(httpContext)}";
    }
}