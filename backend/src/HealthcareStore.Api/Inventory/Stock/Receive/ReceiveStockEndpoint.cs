using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Stock.Receive;

namespace HealthcareStore.Api.Inventory.Stock.Receive;

public static class ReceiveStockEndpoint
{
    public static IEndpointRouteBuilder MapReceiveStockEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/inventory/batches/{batchId:guid}/warehouses/{warehouseId:guid}/receive",
                async (
                    Guid batchId,
                    Guid warehouseId,
                    ReceiveStockApiRequest request,
                    ReceiveStockService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.ReceiveAsync(
                            new ReceiveStockRequest(
                                batchId,
                                warehouseId,
                                request.Quantity,
                                request.CostPrice),
                            cancellationToken);

                    var response =
                        new ReceiveStockApiResponse(
                            result.WarehouseStockId,
                            result.InventoryBatchId,
                            result.WarehouseId,
                            result.CostPrice,
                            result.QuantityOnHand,
                            result.QuantityReserved,
                            result.AvailableQuantity,
                            result.MovementId);

                    return Results.Created(
                        $"/api/v1/inventory/stock/{result.WarehouseStockId}",
                        response);
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("ReceiveInventoryStock")
            .WithTags("Inventory - Stock")
            .Produces<ReceiveStockApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}