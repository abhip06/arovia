using HealthcareStore.Application.Products.Create;

namespace HealthcareStore.Api.Admin.Products.Create;

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/admin/products",
            async (
                CreateProductApiRequest request,
                CreateProductService service,
                CancellationToken cancellationToken) =>
            {
                var result = await service.CreateAsync(
                    new CreateProductRequest(
                            request.Name,
                            request.ProductType,
                            request.ManufacturerId,
                            request.BrandId,
                            request.RequiresPrescription,
                            request.HsnCode,
                            request.TaxRate,
                            request.Returnable,
                            request.ReturnWindowDays),
                        cancellationToken);

                var response = new CreateProductApiResponse(
                    result.Id,
                    result.Name,
                    result.Slug,
                    result.ProductType,
                    result.Status);

                return Results.Created(
                        $"/api/v1/admin/products/{result.Id}",
                        response);
            })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateProduct")
            .WithTags("Admin - Products")
            .Produces<CreateProductApiResponse>(StatusCodes.Status201Created);

        return endpoints;
    }
}
