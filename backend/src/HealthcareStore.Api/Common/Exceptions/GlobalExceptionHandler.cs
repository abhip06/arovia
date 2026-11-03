using HealthcareStore.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareStore.Api.Common.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            BadHttpRequestException =>
                StatusCodes.Status400BadRequest,

            ConflictException =>
                StatusCodes.Status409Conflict,

            NotFoundException =>
                StatusCodes.Status404NotFound,

            UnauthorizedException =>
                StatusCodes.Status401Unauthorized,

            ForbiddenException =>
                StatusCodes.Status403Forbidden,

            InvalidVerificationTokenException =>
                StatusCodes.Status400BadRequest,

            ArgumentException =>
                StatusCodes.Status400BadRequest,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        if (statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {HttpMethod} {RequestPath}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Request failed with status code {StatusCode} for {HttpMethod} {RequestPath}.",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest =>
                "Bad Request",

            StatusCodes.Status401Unauthorized =>
                "Unauthorized",

            StatusCodes.Status403Forbidden =>
                "Forbidden",

            StatusCodes.Status404NotFound =>
                "Not Found",

            StatusCodes.Status409Conflict =>
                "Conflict",

            _ =>
                "Internal Server Error"
        };

        var detail = statusCode switch
        {
            StatusCodes.Status500InternalServerError =>
                "An unexpected error occurred.",

            StatusCodes.Status400BadRequest
                when exception is BadHttpRequestException =>
                "The request could not be understood.",

            _ =>
                exception.Message
        };

        httpContext.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = httpContext.Request.Path
                }
            });

        return true;
    }
}