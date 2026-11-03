using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Batches.Status;

namespace HealthcareStore.Api.Inventory.Batches.Status;

public static class InventoryBatchStatusEndpoints
{
    public static IEndpointRouteBuilder
        MapInventoryBatchStatusEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/inventory/batches/{batchId:guid}/quarantine",
                async (
                    Guid batchId,
                    InventoryBatchStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.QuarantineAsync(
                        batchId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("QuarantineInventoryBatch")
            .WithTags("Inventory - Batches")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/inventory/batches/{batchId:guid}/recall",
                async (
                    Guid batchId,
                    InventoryBatchStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.RecallAsync(
                        batchId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("RecallInventoryBatch")
            .WithTags("Inventory - Batches")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/inventory/batches/{batchId:guid}/activate",
                async (
                    Guid batchId,
                    InventoryBatchStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.ActivateAsync(
                        batchId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization(
                AuthorizationPolicies.AdminOrWarehouseStaff)
            .WithName("ActivateInventoryBatch")
            .WithTags("Inventory - Batches")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}