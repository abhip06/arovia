using HealthcareStore.Application.Catalog.Brands.Get;

namespace HealthcareStore.Api.Catalog.Brands;

public static class GetBrandsEndpoint
{
    public static IEndpointRouteBuilder MapGetBrandsEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/catalog/brands",
            async (
                GetBrandsService service,
                CancellationToken cancellationToken) =>
            {
                var brands =
                    await service.GetAsync(cancellationToken);

                var response = brands
                    .Select(static brand => new BrandItemApiResponse(
                        brand.Slug,
                        brand.Name))
                    .ToList();

                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<BrandItemApiResponse>>(StatusCodes.Status200OK);

        return endpoints;
    }
}
