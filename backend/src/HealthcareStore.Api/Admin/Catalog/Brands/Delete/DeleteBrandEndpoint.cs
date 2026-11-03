using HealthcareStore.Application.Catalog.Brands.Delete;

namespace HealthcareStore.Api.Admin.Catalog.Brands.Delete;

public static class DeleteBrandEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBrandEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(
                "/api/v1/admin/brands/{brandId:guid}",
                async (
                    Guid brandId,
                    DeleteBrandService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeleteAsync(
                        brandId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteBrand")
            .WithTags("Admin - Brands")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}