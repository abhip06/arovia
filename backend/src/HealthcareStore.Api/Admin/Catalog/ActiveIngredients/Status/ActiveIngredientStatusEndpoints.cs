using HealthcareStore.Application.Catalog.ActiveIngredients.Status;

namespace HealthcareStore.Api.Admin.ActiveIngredients.Status;

public static class ActiveIngredientStatusEndpoints
{
    public static IEndpointRouteBuilder
        MapActiveIngredientStatusEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/active-ingredients/{activeIngredientId:guid}/activate",
                async (
                    Guid activeIngredientId,
                    ActiveIngredientStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.ActivateAsync(
                        activeIngredientId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("ActivateActiveIngredient")
            .WithTags("Admin - Active Ingredients")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/active-ingredients/{activeIngredientId:guid}/deactivate",
                async (
                    Guid activeIngredientId,
                    ActiveIngredientStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeactivateAsync(
                        activeIngredientId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeactivateActiveIngredient")
            .WithTags("Admin - Active Ingredients")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}