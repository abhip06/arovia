using HealthcareStore.Application.Products.RemoveIngredient;

namespace HealthcareStore.Api.Admin.Products.Ingredients.Remove;

public static class RemoveProductIngredientEndpoint
{
    public static IEndpointRouteBuilder
        MapRemoveProductIngredientEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(
                "/api/v1/admin/products/{productId:guid}/ingredients/{productIngredientId:guid}",
                async (
                    Guid productId,
                    Guid productIngredientId,
                    RemoveProductIngredientService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.RemoveAsync(
                        productId,
                        productIngredientId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("RemoveProductIngredient")
            .WithTags("Admin - Product Ingredients")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}