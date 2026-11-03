using HealthcareStore.Application.Catalog.Manufacturers.Delete;

namespace HealthcareStore.Api.Admin.Catalog.Manufacturers.Delete;

public static class DeleteManufacturerEndpoint
{
    public static IEndpointRouteBuilder MapDeleteManufacturerEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(
                "/api/v1/admin/manufacturers/{manufacturerId:guid}",
                async (
                    Guid manufacturerId,
                    DeleteManufacturerService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeleteAsync(
                        manufacturerId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteManufacturer")
            .WithTags("Admin - Manufacturers")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}