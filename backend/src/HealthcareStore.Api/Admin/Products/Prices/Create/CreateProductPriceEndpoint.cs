using HealthcareStore.Application.Products.Prices.Create;

namespace HealthcareStore.Api.Admin.Products.Prices.Create;

public static class CreateProductPriceEndpoint
{
    public static IEndpointRouteBuilder
        MapCreateProductPriceEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/variants/{variantId:guid}/prices",
                async (
                    Guid productId,
                    Guid variantId,
                    CreateProductPriceApiRequest request,
                    CreateProductPriceService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.CreateAsync(
                            new CreateProductPriceRequest(
                                productId,
                                variantId,
                                request.SellingPrice,
                                request.EffectiveFrom),
                            cancellationToken);

                    var response =
                        new CreateProductPriceApiResponse(
                            result.Id,
                            result.ProductVariantId,
                            result.SellingPrice,
                            result.EffectiveFrom);

                    return Results.Created(
                        $"/api/v1/admin/products/{productId}/variants/{variantId}/prices/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateProductPrice")
            .WithTags("Admin - Product Prices")
            .Produces<CreateProductPriceApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}