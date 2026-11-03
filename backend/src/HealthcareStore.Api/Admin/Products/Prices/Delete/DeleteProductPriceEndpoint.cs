using HealthcareStore.Application.Products.Prices.Delete;

namespace HealthcareStore.Api.Admin.Products.Prices.Delete;

public static class DeleteProductPriceEndpoint
{
    public static IEndpointRouteBuilder
        MapDeleteProductPriceEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(
                "/api/v1/admin/products/prices/{priceId:guid}",
                async (
                    Guid priceId,
                    DeleteProductPriceService service,
                    CancellationToken cancellationToken) =>
                {
                    await service.DeleteAsync(
                        priceId,
                        cancellationToken);

                    return Results.NoContent();
                })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteProductPrice")
            .WithTags("Admin - Product Prices")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }
}