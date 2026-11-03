using HealthcareStore.Application.Products.Variants.Create;

namespace HealthcareStore.Api.Admin.Products.Variants.Create;

public static class CreateProductVariantEndpoint
{
    public static IEndpointRouteBuilder
        MapCreateProductVariantEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/admin/products/{productId:guid}/variants",
                async (
                    Guid productId,
                    CreateProductVariantApiRequest request,
                    CreateProductVariantService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.CreateAsync(
                            new CreateProductVariantRequest(
                                productId,
                                request.Barcode,
                                request.PackageQuantity,
                                request.PackageUnit,
                                request.NetQuantity,
                                request.NetQuantityUnit),
                            cancellationToken);

                    var response =
                        new CreateProductVariantApiResponse(
                            result.Id,
                            result.ProductId,
                            result.Sku,
                            result.Barcode,
                            result.PackageQuantity,
                            result.PackageUnit,
                            result.NetQuantity,
                            result.NetQuantityUnit,
                            result.Status);

                    return Results.Created(
                        $"/api/v1/admin/products/{productId}/variants/{result.Id}",
                        response);
                })
            .RequireAuthorization("AdminOnly")
            .WithName("CreateProductVariant")
            .WithTags("Admin - Product Variants")
            .Produces<CreateProductVariantApiResponse>(
                StatusCodes.Status201Created);

        return endpoints;
    }
}