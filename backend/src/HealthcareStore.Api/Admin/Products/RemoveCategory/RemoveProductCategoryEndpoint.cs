using HealthcareStore.Application.Products.RemoveCategory;

namespace HealthcareStore.Api.Admin.Products.RemoveCategory;

public static class RemoveProductCategoryEndpoint
{
    public static IEndpointRouteBuilder
        MapRemoveProductCategoryEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(
                "/api/v1/admin/products/{productId:guid}/categories/{categoryId:guid}",
                async (
                    Guid productId,
                    Guid categoryId,
                    RemoveProductCategoryService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.RemoveAsync(
                        productId,
                        categoryId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("RemoveProductCategory")
            .WithTags("Admin - Product Categories")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}