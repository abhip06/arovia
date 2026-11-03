using HealthcareStore.Application.Products.Prices.Get;

namespace HealthcareStore.Api.Admin.Products.Prices.Get;

public static class GetProductPriceHistoryEndpoint
{
    public static IEndpointRouteBuilder
        MapGetProductPriceHistoryEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/admin/products/variants/{variantId:guid}/prices",
                async (
                    Guid variantId,
                    GetProductPriceHistoryService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.GetAsync(
                        variantId,
                        cancellationToken);

                    var response = result
                    .Select(price => new GetProductPriceHistoryApiResponse(
                        price.Id,
                        price.ProductVariantId,
                        price.SellingPrice,
                        price.EffectiveFrom,
                        price.IsCurrent,
                        price.IsFuture))
                    .ToList();

                    return Results.Ok(response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("GetProductPriceHistory")
            .WithTags("Admin - Product Prices")
            .Produces<IReadOnlyList<GetProductPriceHistoryApiResponse>>(StatusCodes.Status200OK);

        return endpoints;
    }
}