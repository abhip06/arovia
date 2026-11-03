using HealthcareStore.Application.Products.Status;

namespace HealthcareStore.Api.Admin.Products.Status;

public static class ProductStatusEndpoints
{
    public static IEndpointRouteBuilder MapProductStatusEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/activate",
                async (
                    Guid productId,
                    ProductStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.ActivateAsync(
                        productId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("ActivateProduct")
            .WithTags("Admin - Products")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/deactivate",
                async (
                    Guid productId,
                    ProductStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeactivateAsync(
                        productId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeactivateProduct")
            .WithTags("Admin - Products")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/discontinue",
                async (
                    Guid productId,
                    ProductStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DiscontinueAsync(
                        productId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DiscontinueProduct")
            .WithTags("Admin - Products")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}