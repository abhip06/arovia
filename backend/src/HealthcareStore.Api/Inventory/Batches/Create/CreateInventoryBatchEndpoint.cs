using HealthcareStore.Application.Inventory.Batches.Create;

namespace HealthcareStore.Api.Inventory.Batches.Create;

public static class CreateInventoryBatchEndpoint
{
    public static IEndpointRouteBuilder
        MapCreateInventoryBatchEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/inventory/variants/{variantId:guid}/batches",
                async (
                    Guid variantId,
                    CreateInventoryBatchApiRequest request,
                    CreateInventoryBatchService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.CreateAsync(
                            new CreateInventoryBatchRequest(
                                variantId,
                                request.BatchNumber,
                                request.ExpiryDate,
                                request.SellableUntil,
                                request.Mrp),
                            cancellationToken);

                    var response =
                        new CreateInventoryBatchApiResponse(
                            result.Id,
                            result.ProductVariantId,
                            result.BatchNumber,
                            result.ExpiryDate,
                            result.SellableUntil,
                            result.Mrp,
                            result.Status);

                    return Results.Created(
                        $"/api/v1/inventory/batches/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOrWarehouseStaff")
            .WithName("CreateInventoryBatch")
            .WithTags("Inventory - Inventory Batches")
            .Produces<CreateInventoryBatchApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}