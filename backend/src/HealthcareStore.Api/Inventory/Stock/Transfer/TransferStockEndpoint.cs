using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Stock.Transfer;

namespace HealthcareStore.Api.Inventory.Stock.Transfer;

public static class TransferStockEndpoint
{
    public static IEndpointRouteBuilder MapTransferStockEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/inventory/batches/{batchId:guid}/transfer/{sourceWarehouseId:guid}/{destinationWarehouseId:guid}",
                async (
                    Guid batchId,
                    Guid sourceWarehouseId,
                    Guid destinationWarehouseId,
                    TransferStockApiRequest request,
                    TransferStockService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.TransferAsync(
                            new TransferStockRequest(
                                batchId,
                                sourceWarehouseId,
                                destinationWarehouseId,
                                request.Quantity,
                                request.Note),
                            cancellationToken);

                    var response =
                        new TransferStockApiResponse(
                            result.SourceWarehouseStockId,
                            result.DestinationWarehouseStockId,
                            result.InventoryBatchId,
                            result.SourceWarehouseId,
                            result.DestinationWarehouseId,
                            result.QuantityTransferred,
                            result.SourceQuantityOnHand,
                            result.DestinationQuantityOnHand,
                            result.TransferReferenceId,
                            result.TransferOutMovementId,
                            result.TransferInMovementId);

                    return Results.Ok(response);
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("TransferInventoryStock")
            .WithTags("Inventory - Stock")
            .Produces<TransferStockApiResponse>(
                StatusCodes.Status200OK);

        return endpoints;
    }
}