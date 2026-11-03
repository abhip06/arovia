using HealthcareStore.Application.Inventory.Warehouses.Status;

namespace HealthcareStore.Api.Admin.Warehouses.Status;

public static class WarehouseStatusEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseStatusEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/warehouses/{warehouseId:guid}/activate",
                async (
                    Guid warehouseId,
                    WarehouseStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.ActivateAsync(
                        warehouseId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("ActivateWarehouse")
            .WithTags("Admin - Warehouses")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/warehouses/{warehouseId:guid}/deactivate",
                async (
                    Guid warehouseId,
                    WarehouseStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeactivateAsync(
                        warehouseId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeactivateWarehouse")
            .WithTags("Admin - Warehouses")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}