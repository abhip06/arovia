using HealthcareStore.Application.Catalog.Manufacturers.Create;

namespace HealthcareStore.Api.Admin.Catalog.Manufacturers.Create;

public static class CreateManufacturerEndpoint
{
    public static IEndpointRouteBuilder MapCreateManufacturerEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/manufacturers",
                async (
                    CreateManufacturerApiRequest request,
                    CreateManufacturerService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.CreateAsync(
                        new CreateManufacturerRequest(
                            request.Name),
                        cancellationToken);

                    var response =
                        new CreateManufacturerApiResponse(
                            result.Id,
                            result.Name,
                            result.Slug,
                            result.Status);

                    return Results.Created(
                        $"/api/v1/admin/manufacturers/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateManufacturer")
            .WithTags("Admin - Manufacturers")
            .Produces<CreateManufacturerApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}