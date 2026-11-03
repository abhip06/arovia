using HealthcareStore.Application.Inventory.Warehouses.Create;

namespace HealthcareStore.Api.Admin.Warehouses.Create;

public static class CreateWarehouseEndpoint
{
    public static IEndpointRouteBuilder MapCreateWarehouseEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/warehouses",
                async (
                    CreateWarehouseApiRequest request,
                    CreateWarehouseService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.CreateAsync(
                        new CreateWarehouseRequest(
                            request.Code,
                            request.Name,
                            request.AddressLine1,
                            request.AddressLine2,
                            request.City,
                            request.State,
                            request.PostalCode,
                            request.Country),
                        cancellationToken);

                    var response = new CreateWarehouseApiResponse(
                        result.Id,
                        result.Code,
                        result.Name,
                        result.AddressLine1,
                        result.AddressLine2,
                        result.City,
                        result.State,
                        result.PostalCode,
                        result.Country,
                        result.Status);

                    return Results.Created(
                        $"/api/v1/admin/warehouses/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateWarehouse")
            .WithTags("Admin - Warehouses")
            .Produces<CreateWarehouseApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}