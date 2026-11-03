using System.Security.Claims;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Orders.Receipts;

namespace HealthcareStore.Api.Orders.Receipts;

public static class OrderReceiptEndpoints
{
    /// <summary>
    /// Map this on the existing authenticated customer orders route group.
    /// For example, /api/v1/orders/{orderId}/receipt.
    /// </summary>
    public static IEndpointRouteBuilder MapCustomerOrderReceipt(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/orders/{orderId:guid}/receipt", DownloadAsync)
            .RequireAuthorization()
            .WithName("DownloadCustomerOrderReceipt")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> DownloadAsync(
        Guid orderId,
        ClaimsPrincipal principal,
        OrderReceiptService receiptService,
        CancellationToken cancellationToken)
    {
        var customerId = principal.GetRequiredUserId();

        var stream = CreateTemporaryReceiptStream();

        try
        {
            var result = await receiptService.GenerateForCustomerAsync(
                orderId,
                customerId,
                stream,
                cancellationToken);

            if (result.Status != OrderReceiptGenerationStatus.Generated)
            {
                await stream.DisposeAsync();

                return result.Status switch
                {
                    OrderReceiptGenerationStatus.OrderNotFound =>
                        TypedResults.NotFound(),

                    OrderReceiptGenerationStatus.PaymentNotCaptured =>
                        TypedResults.Conflict(new
                        {
                            code = "PaymentNotCaptured",
                            message = "A receipt is available after payment is confirmed."
                        }),

                    OrderReceiptGenerationStatus.PaymentReconciliationRequired =>
                        TypedResults.Conflict(new
                        {
                            code = "PaymentReconciliationRequired",
                            message = "The payment needs verification before a receipt can be issued. Please contact support."
                        }),

                    _ => TypedResults.Problem(
                        title: "Receipt could not be generated.",
                        statusCode: StatusCodes.Status500InternalServerError)
                };
            }

            await stream.FlushAsync(cancellationToken);
            stream.Position = 0;

            // The file result owns and disposes the stream after writing the response.
            // DeleteOnClose removes the temporary file when that stream is disposed.
            return TypedResults.File(
                stream,
                contentType: "application/pdf",
                fileDownloadName: result.FileName);
        }
        catch
        {
            await stream.DisposeAsync();
            throw;
        }
    }

    private static FileStream CreateTemporaryReceiptStream()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"arovia-receipt-{Guid.NewGuid():N}.pdf");

        return new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous |
                     FileOptions.SequentialScan |
                     FileOptions.DeleteOnClose);
    }
}
