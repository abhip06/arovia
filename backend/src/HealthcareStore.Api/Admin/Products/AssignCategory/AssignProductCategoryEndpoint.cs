using HealthcareStore.Application.Products.AssignCategory;

namespace HealthcareStore.Api.Admin.Products.AssignCategory;

public static class AssignProductCategoryEndpoint
{
    public static IEndpointRouteBuilder
        MapAssignProductCategoryEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/categories/{categoryId:guid}",
                async (
                    Guid productId,
                    Guid categoryId,
                    AssignProductCategoryService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.AssignAsync(
                        new AssignProductCategoryRequest(
                            productId,
                            categoryId),
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("AssignProductCategory")
            .WithTags("Admin - Product Categories")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}