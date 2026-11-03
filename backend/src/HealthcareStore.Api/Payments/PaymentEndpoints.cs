using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Api.Common.Pagination;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Common.Pagination;
using HealthcareStore.Application.Payments;
using HealthcareStore.Application.Payments.Models;
using System.Security.Claims;

namespace HealthcareStore.Api.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var orderPayments = endpoints
            .MapGroup("/api/v1/orders/{orderId:guid}")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Orders);

        orderPayments.MapPost(
            "/payment-session",
            async Task<IResult> (
                Guid orderId,
                HttpRequest httpRequest,
                ClaimsPrincipal user,
                CreatePaymentSessionService service,
                CancellationToken cancellationToken) =>
            {
                var values = httpRequest.Headers["Idempotency-Key"];

                if (values.Count != 1 ||
                    string.IsNullOrWhiteSpace(values[0]))
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["Idempotency-Key"] =
                            [
                                "A single Idempotency-Key header is required."
                            ]
                        });
                }

                var idempotencyKey = values[0]!.Trim();

                if (idempotencyKey.Length > 128)
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["Idempotency-Key"] =
                            [
                                "Idempotency-Key cannot exceed 128 characters."
                            ]
                        });
                }

                var result = await service.ExecuteAsync(
                    user.GetRequiredUserId(),
                    orderId,
                    idempotencyKey,
                    cancellationToken);

                return result.Status switch
                {
                    CreatePaymentSessionStatus.Created or
                    CreatePaymentSessionStatus.Replayed
                        when result.Session is not null
                            => Results.Ok(result.Session),

                    CreatePaymentSessionStatus.OrderNotFound
                        => Results.NotFound(),

                    CreatePaymentSessionStatus.OrderNotPayable
                        or CreatePaymentSessionStatus.PaymentExpired
                        or CreatePaymentSessionStatus.PaymentAlreadyCaptured
                        or CreatePaymentSessionStatus.IdempotencyConflict
                        or CreatePaymentSessionStatus.PaymentAttemptInProgress
                            => Results.Conflict(new
                            {
                                result.Status
                            }),

                    CreatePaymentSessionStatus.GatewayUnavailable
                        => Results.Problem(
                            statusCode: StatusCodes.Status502BadGateway,
                            title: "Payment gateway unavailable",
                            detail: "The payment gateway could not be reached. Retry using the same idempotency key."),

                    CreatePaymentSessionStatus.ReconciliationRequired
                        => Results.Conflict(new
                        {
                            result.Status,
                            Message = "The payment requires reconciliation before checkout can continue."
                        }),

                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unable to create payment session.")
                };
            });

        var payments = endpoints
            .MapGroup("/api/v1/payments")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Orders);

        payments.MapPost(
            "/verify",
            async Task<IResult> (
                ClaimsPrincipal user,
                VerifyRazorpayPaymentRequest request,
                VerifyRazorpayPaymentService service,
                SendOrderPaymentConfirmationEmailService emailService,
                CancellationToken cancellationToken) =>
            {
                var customerId = user.GetRequiredUserId();
                var result = await service.ExecuteAsync(
                    customerId,
                    request,
                    cancellationToken);

                if (result.Status == VerifyRazorpayPaymentStatus.Captured)
                {
                    // Payment state is already committed. Do not let a caller
                    // disconnect cancel the receipt/email side effect or turn
                    // a successful payment into an apparent checkout failure.
                    await emailService.SendAsync(
                        customerId,
                        result.Order,
                        CancellationToken.None);
                }

                return result.Status switch
                {
                    VerifyRazorpayPaymentStatus.Captured or
                    VerifyRazorpayPaymentStatus.AlreadyCaptured
                        => Results.Ok(result),

                    VerifyRazorpayPaymentStatus.PaymentPending
                        => Results.Ok(result),

                    VerifyRazorpayPaymentStatus.PaymentFailed
                        => Results.Ok(result),

                    VerifyRazorpayPaymentStatus.InvalidSignature
                        => Results.BadRequest(new
                        {
                            result.Status,
                            Message = "The Razorpay payment signature is invalid."
                        }),

                    VerifyRazorpayPaymentStatus.PaymentNotFound or
                    VerifyRazorpayPaymentStatus.OrderNotFound
                        => Results.NotFound(),

                    VerifyRazorpayPaymentStatus.InvalidPayment
                        => Results.BadRequest(new
                        {
                            result.Status,
                            Message = "The payment details do not match the expected order."
                        }),

                    VerifyRazorpayPaymentStatus.OrderNotPayable or
                    VerifyRazorpayPaymentStatus.ReconciliationRequired
                        => Results.Conflict(new
                        {
                            result.Status
                        }),

                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unable to verify payment.")
                };
            });

        var adminPayments = endpoints
            .MapGroup("/api/v1/admin/payments")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        adminPayments.MapGet(
            "/",
            async Task<IResult> (
                [AsParameters] PaginationQuery pagination,
                ClaimsPrincipal principal,
                GetPaymentHistoryService service,
                CancellationToken cancellationToken) =>
            {
                if (pagination.Page < 1)
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["page"] =
                            ["Page must be greater than or equal to 1."]
                        });
                }

                var adminUserId = principal.GetRequiredUserId();

                var result = await service.GetPageAsync(
                    adminUserId,
                    pagination.Page,
                    cancellationToken);

                return Results.Ok(result);
            });

        return endpoints;
    }
}
