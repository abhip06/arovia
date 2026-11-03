using HealthcareStore.Application.Catalog.Categories.Get;

namespace HealthcareStore.Api.Catalog.Categories;

public static class GetCategoriesEndpoint
{
    public static IEndpointRouteBuilder MapGetCategoriesEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/api/v1/catalog/categories",
                async (
                    GetCategoriesService service,
                    CancellationToken cancellationToken) =>
                {
                    var categories =
                        await service.GetAsync(cancellationToken);

                    var response = categories
                        .Select(Map)
                        .ToList();

                    return Results.Ok(response);
                })
            .Produces<IReadOnlyList<CategoryTreeResponse>>(
                StatusCodes.Status200OK);

        return endpoints;
    }

    private static CategoryTreeResponse Map(
        CategoryTreeItem category)
    {
        return new CategoryTreeResponse(
            category.ProductType,
            category.Slug,
            category.Name,
            category.Children
                .Select(Map)
                .ToList());
    }
}