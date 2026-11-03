using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Stock.Adjust;

namespace HealthcareStore.Api.Inventory.Stock.Adjust;

public static class AdjustStockEndpoint
{
    public static IEndpointRouteBuilder MapAdjustStockEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/inventory/batches/{batchId:guid}/warehouses/{warehouseId:guid}/adjust",
                async (
                    Guid batchId,
                    Guid warehouseId,
                    AdjustStockApiRequest request,
                    AdjustStockService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.AdjustAsync(
                            new AdjustStockRequest(
                                batchId,
                                warehouseId,
                                request.QuantityDelta,
                                request.Reason),
                            cancellationToken);

                    var response =
                        new AdjustStockApiResponse(
                            result.WarehouseStockId,
                            result.InventoryBatchId,
                            result.WarehouseId,
                            result.QuantityDelta,
                            result.QuantityOnHand,
                            result.QuantityReserved,
                            result.AvailableQuantity,
                            result.MovementId);

                    return Results.Ok(response);
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("AdjustInventoryStock")
            .WithTags("Inventory - Stock")
            .Produces<AdjustStockApiResponse>(
                StatusCodes.Status200OK);

        return endpoints;
    }
}