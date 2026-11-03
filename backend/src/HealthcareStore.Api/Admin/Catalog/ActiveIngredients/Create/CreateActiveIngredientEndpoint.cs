using HealthcareStore.Application.Catalog.ActiveIngredients.Create;

namespace HealthcareStore.Api.Admin.Catalog.ActiveIngredients.Create;

public static class CreateActiveIngredientEndpoint
{
    public static IEndpointRouteBuilder
        MapCreateActiveIngredientEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/active-ingredients",
                async (
                    CreateActiveIngredientApiRequest request,
                    CreateActiveIngredientService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.CreateAsync(
                            new CreateActiveIngredientRequest(
                                request.Name),
                            cancellationToken);

                    var response =
                        new CreateActiveIngredientApiResponse(
                            result.Id,
                            result.Name,
                            result.Slug,
                            result.Status);

                    return Results.Created(
                        $"/api/v1/admin/active-ingredients/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateActiveIngredient")
            .WithTags("Admin - Active Ingredients")
            .Produces<CreateActiveIngredientApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}