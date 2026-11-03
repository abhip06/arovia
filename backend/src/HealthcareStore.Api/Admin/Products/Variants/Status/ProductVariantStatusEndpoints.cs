using HealthcareStore.Application.Products.Variants.Status;

namespace HealthcareStore.Api.Admin.Products.Variants.Status;

public static class ProductVariantStatusEndpoints
{
    public static IEndpointRouteBuilder MapProductVariantStatusEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/products/variants/{variantId:guid}/activate",
                async (
                    Guid variantId,
                    ProductVariantStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.ActivateAsync(
                        variantId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("ActivateProductVariant")
            .WithTags("Admin - Product Variants")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/products/variants/{variantId:guid}/deactivate",
                async (
                    Guid variantId,
                    ProductVariantStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeactivateAsync(
                        variantId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeactivateProductVariant")
            .WithTags("Admin - Product Variants")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/products/variants/{variantId:guid}/discontinue",
                async (
                    Guid variantId,
                    ProductVariantStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DiscontinueAsync(
                        variantId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DiscontinueProductVariant")
            .WithTags("Admin - Product Variants")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}