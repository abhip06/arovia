using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Stock.Get;

namespace HealthcareStore.Api.Inventory.Stock.Get;

public static class GetWarehouseStockEndpoint
{
    public static IEndpointRouteBuilder MapGetWarehouseStockEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/inventory/batches/{batchId:guid}/warehouses/{warehouseId:guid}/stock",
                async (
                    Guid batchId,
                    Guid warehouseId,
                    GetWarehouseStockService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.GetAsync(
                        batchId,
                        warehouseId,
                        cancellationToken);

                    var response =
                        new GetWarehouseStockApiResponse(
                            result.Id,
                            result.InventoryBatchId,
                            result.WarehouseId,
                            result.CostPrice,
                            result.QuantityOnHand,
                            result.QuantityReserved,
                            result.AvailableQuantity,
                            result.ReceivedAt);

                    return Results.Ok(response);
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("GetWarehouseStock")
            .WithTags("Inventory - Stock")
            .Produces<GetWarehouseStockApiResponse>(
                StatusCodes.Status200OK);

        endpoints.MapGet(
        "/api/v1/inventory/warehouses/{warehouseId:guid}/stock",
        async (
            Guid warehouseId,
            GetWarehouseStockService service,
            CancellationToken cancellationToken,
            int page = 1) =>
        {
            var result =
                await service.GetPageAsync(
                    warehouseId,
                    page,
                    cancellationToken);

            var response =
                new WarehouseStockListApiResponse(
                    result.Result.Items
                        .Select(static stock =>
                            new WarehouseStockApiResponse(
                                stock.Id,
                                stock.InventoryBatchId,
                                stock.WarehouseId,
                                stock.Sku,
                                stock.BatchNumber,
                                stock.ExpiryDate,
                                stock.CostPrice,
                                stock.QuantityOnHand,
                                stock.QuantityReserved,
                                stock.AvailableQuantity,
                                stock.ReceivedAt))
                        .ToList(),
                    result.Result.Page,
                    result.Result.PageSize,
                    result.Result.TotalCount,
                    result.Result.TotalPages);

            return Results.Ok(response);
        })
    .RequireAuthorization(
        AuthorizationPolicies.AdminOrWarehouseStaff)
    .WithName("GetWarehouseStockPage")
    .WithTags("Inventory - Stock")
    .Produces<WarehouseStockListApiResponse>(
        StatusCodes.Status200OK);

        return endpoints;
    }
}