using HealthcareStore.Application.Products.Update;

namespace HealthcareStore.Api.Admin.Products.Update;

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder
        MapUpdateProductEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/v1/admin/products/{productId:guid}",
            async (
                Guid productId,
                UpdateProductApiRequest request,
                UpdateProductService service,
                CancellationToken cancellationToken) =>
            {
                var result =
                    await service.UpdateAsync(
                        new UpdateProductRequest(
                            productId,
                            request.Name,
                            request.ManufacturerId,
                            request.BrandId,
                            request.RequiresPrescription,
                            request.HsnCode,
                            request.TaxRate,
                            request.Returnable,
                            request.ReturnWindowDays),
                        cancellationToken);

                var response =
                    new UpdateProductApiResponse(
                        result.Id,
                        result.Name,
                        result.Slug,
                        result.ProductType,
                        result.Status,
                        result.ManufacturerId,
                        result.BrandId,
                        result.RequiresPrescription,
                        result.HsnCode,
                        result.TaxRate,
                        result.Returnable,
                        result.ReturnWindowDays);

                return Results.Ok(response);
            })
            .RequireAuthorization("AdminOnly")
            .WithName("UpdateProduct")
            .WithTags("Admin - Products")
            .Produces<UpdateProductApiResponse>(
                StatusCodes.Status200OK);

        return endpoints;
    }
}