using HealthcareStore.Application.Products.AddIngredient;

namespace HealthcareStore.Api.Admin.Products.Ingredients.Add;

public static class AddProductIngredientEndpoint
{
    public static IEndpointRouteBuilder
        MapAddProductIngredientEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/ingredients",
                async (
                    Guid productId,
                    AddProductIngredientApiRequest request,
                    AddProductIngredientService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.AddAsync(
                            new AddProductIngredientRequest(
                                productId,
                                request.ActiveIngredientId,
                                request.StrengthValue,
                                request.StrengthUnit,
                                request.Role),
                            cancellationToken);

                    var response =
                        new AddProductIngredientApiResponse(
                            result.Id,
                            result.ProductId,
                            result.ActiveIngredientId,
                            result.StrengthValue,
                            result.StrengthUnit,
                            result.Role);

                    return Results.Created(
                        $"/api/v1/admin/products/{productId}/ingredients/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("AddProductIngredient")
            .WithTags("Admin - Product Ingredients")
            .Produces<AddProductIngredientApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}