using HealthcareStore.Application.Catalog.Categories.Create;

namespace HealthcareStore.Api.Admin.Catalog.Categories.Create;

public static class CreateCategoryEndpoint
{
    public static IEndpointRouteBuilder
        MapCreateCategoryEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/categories",
                async (
                    CreateCategoryApiRequest request,
                    CreateCategoryService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.CreateAsync(
                            new CreateCategoryRequest(
                                request.ProductType,
                                request.Name,
                                request.ParentCategoryId,
                                request.SortOrder),
                            cancellationToken);

                    var response =
                        new CreateCategoryApiResponse(
                            result.Id,
                            result.ProductType,
                            result.Name,
                            result.Slug,
                            result.ParentCategoryId,
                            result.Status,
                            result.SortOrder);

                    return Results.Created(
                        $"/api/v1/admin/categories/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateCategory")
            .WithTags("Admin - Categories")
            .Produces<CreateCategoryApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}