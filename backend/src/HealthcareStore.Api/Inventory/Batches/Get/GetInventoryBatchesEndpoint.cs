using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Batches.Get;

namespace HealthcareStore.Api.Inventory.Batches.Get;

public static class GetInventoryBatchesEndpoint
{
    public static IEndpointRouteBuilder MapGetInventoryBatchesEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/api/v1/inventory/batches",
                async (
                    GetInventoryBatchesService service,
                    int page = 1,
                    CancellationToken cancellationToken = default) =>
                {
                    var result =
                        await service.GetAllAsync(
                            new GetInventoryBatchesRequest(page),
                            cancellationToken);

                    return Results.Ok(
                        MapResponse(result));
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOrWarehouseStaff)
            .Produces<InventoryBatchListResponse>(StatusCodes.Status200OK);

        endpoints
            .MapGet(
                "/api/v1/inventory/variants/{sku}/batches",
                async (
                    string sku,
                    GetInventoryBatchesService service,
                    int page = 1,
                    CancellationToken cancellationToken = default) =>
                {
                    var result =
                        await service.GetByVariantAsync(
                            new GetInventoryVariantBatchesRequest(sku, page),
                            cancellationToken);

                    return Results.Ok(
                        MapResponse(result));
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOrWarehouseStaff)
            .Produces<InventoryBatchListResponse>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static InventoryBatchListResponse MapResponse(
        GetInventoryBatchesResponse result)
    {
        return new InventoryBatchListResponse(
            result.Result.Items
                .Select(static batch =>
                    new InventoryBatchResponse(
                        batch.Sku,
                        batch.BatchNumber,
                        batch.ExpiryDate,
                        batch.SellableUntil,
                        batch.Mrp,
                        batch.Status))
                .ToList(),
            result.Result.Page,
            result.Result.PageSize,
            result.Result.TotalCount,
            result.Result.TotalPages);
    }
}