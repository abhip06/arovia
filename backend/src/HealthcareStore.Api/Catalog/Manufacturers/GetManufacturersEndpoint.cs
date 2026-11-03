using HealthcareStore.Application.Catalog.Manufacturers.Get;

namespace HealthcareStore.Api.Catalog.Manufacturers;

public static class GetManufacturersEndpoint
{
    public static IEndpointRouteBuilder MapGetManufacturersEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/api/v1/catalog/manufacturers",
                async (
                    GetManufacturersService service,
                    CancellationToken cancellationToken) =>
                {
                    var manufacturers =
                        await service.GetAsync(cancellationToken);

                    var response = manufacturers
                        .Select(static manufacturer =>
                            new ManufacturerApiResponse(
                                manufacturer.Slug,
                                manufacturer.Name))
                        .ToList();

                    return Results.Ok(response);
                })
            .Produces<IReadOnlyList<ManufacturerApiResponse>>(
                StatusCodes.Status200OK);

        return endpoints;
    }
}