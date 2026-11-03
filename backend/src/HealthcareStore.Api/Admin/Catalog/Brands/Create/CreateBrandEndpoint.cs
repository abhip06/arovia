using HealthcareStore.Application.Catalog.Brands.Create;

namespace HealthcareStore.Api.Admin.Catalog.Brands.Create;

public static class CreateBrandEndpoint
{
    public static IEndpointRouteBuilder MapCreateBrandEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/brands",
                async (
                    CreateBrandApiRequest request,
                    CreateBrandService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.CreateAsync(
                        new CreateBrandRequest(
                            request.Name),
                        cancellationToken);

                    var response =
                        new CreateBrandApiResponse(
                            result.Id,
                            result.Name,
                            result.Slug,
                            result.Status);

                    return Results.Created(
                        $"/api/v1/admin/brands/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateBrand")
            .WithTags("Admin - Brands")
            .Produces<CreateBrandApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}