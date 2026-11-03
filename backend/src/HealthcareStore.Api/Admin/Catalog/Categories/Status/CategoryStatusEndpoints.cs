using HealthcareStore.Application.Catalog.Categories.Status;

namespace HealthcareStore.Api.Admin.Categories.Status;

public static class CategoryStatusEndpoints
{
    public static IEndpointRouteBuilder MapCategoryStatusEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/categories/{categoryId:guid}/activate",
                async (
                    Guid categoryId,
                    CategoryStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.ActivateAsync(
                        categoryId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("ActivateCategory")
            .WithTags("Admin - Categories")
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost(
                "/api/v1/admin/categories/{categoryId:guid}/deactivate",
                async (
                    Guid categoryId,
                    CategoryStatusService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeactivateAsync(
                        categoryId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeactivateCategory")
            .WithTags("Admin - Categories")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}