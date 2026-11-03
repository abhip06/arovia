using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Movements.Get;

namespace HealthcareStore.Api.Inventory.Movements.Get;

public static class GetInventoryMovementHistoryEndpoint
{
    public static IEndpointRouteBuilder
        MapGetInventoryMovementHistoryEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/inventory/stock/{warehouseStockId:guid}/movements",
                async (
                    Guid warehouseStockId,
                    int? page,
                    int? pageSize,
                    GetInventoryMovementHistoryService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.GetAsync(
                        new GetInventoryMovementHistoryRequest(
                            warehouseStockId,
                            page ?? 1),
                        cancellationToken);

                    var response =
                        new GetInventoryMovementHistoryApiResponse(
                            result.Items
                                .Select(x =>
                                    new InventoryMovementApiItem(
                                        x.Id,
                                        x.WarehouseStockId,
                                        x.MovementType,
                                        x.QuantityDelta,
                                        x.CreatedByUserId,
                                        x.ReferenceType,
                                        x.ReferenceId,
                                        x.Note,
                                        x.CreatedAt))
                                .ToList(),
                            result.Page,
                            result.PageSize,
                            result.HasNextPage);

                    return Results.Ok(response);
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("GetInventoryMovementHistory")
            .WithTags("Inventory - Movements")
            .Produces<GetInventoryMovementHistoryApiResponse>(
                StatusCodes.Status200OK);

        return endpoints;
    }
}