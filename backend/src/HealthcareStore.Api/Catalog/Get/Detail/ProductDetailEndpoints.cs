using HealthcareStore.Application.Abstractions.Location;
using HealthcareStore.Application.Products.Get.Detail;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareStore.Api.Catalog.Get.Detail;

public static class ProductDetailEndpoints
{
    public static IEndpointRouteBuilder MapProductDetailEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/api/v1/catalog/products/{productSlug}/variants/{sku}",
                GetProductDetailAsync)
            .WithName("GetProductDetail")
            .WithTags("Catalog")
            .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> GetProductDetailAsync(
        string productSlug,
        string sku,
        [FromQuery(Name = "pincode")] string? pincode,
        HttpContext httpContext,
        IPincodeContextResolver pincodeContextResolver,
        GetProductDetailService service,
        CancellationToken cancellationToken)
    {
        var normalizedPincode = Normalize(pincode);

        if (normalizedPincode is not null &&
            !IsValidPincode(normalizedPincode))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["pincode"] =
                    ["Pincode must contain exactly 6 digits."]
                });
        }

        var pincodeContext =
            await pincodeContextResolver.ResolveAsync(
                normalizedPincode,
                httpContext.Connection.RemoteIpAddress,
                cancellationToken);

        var detail = await service.GetAsync(
            productSlug,
            sku,
            pincodeContext,
            cancellationToken);

        return Results.Ok(Map(detail));
    }

    private static bool IsValidPincode(string pincode)
    {
        return pincode.Length == 6 &&
            pincode.All(static digit =>
                digit >= '0' && digit <= '9');
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static ProductDetailResponse Map(
        ProductDetailReadModel detail)
    {
        var product = detail.Base;

        return new ProductDetailResponse(
            new ProductDetailProductResponse(
                product.ProductId,
                product.ProductName,
                product.ProductSlug,
                product.ProductType,
                product.RequiresPrescription,
                product.BrandName is null || product.BrandSlug is null
                    ? null
                    : new ProductDetailCatalogReferenceResponse(
                        product.BrandName,
                        product.BrandSlug),
                new ProductDetailCatalogReferenceResponse(
                    product.ManufacturerName,
                    product.ManufacturerSlug),
                product.Returnable,
                product.ReturnWindowDays,
                product.Categories
                    .Select(static category =>
                        new ProductDetailCategoryResponse(
                            category.Name,
                            category.Slug))
                    .ToList(),
                product.Ingredients
                    .Select(static ingredient =>
                        new ProductDetailIngredientResponse(
                            ingredient.Name,
                            ingredient.Slug,
                            ingredient.StrengthValue,
                            ingredient.StrengthUnit,
                            ingredient.Role))
                    .ToList()),
            new ProductDetailVariantResponse(
                product.ProductVariantId,
                product.Sku,
                product.Barcode,
                product.PackageQuantity,
                product.PackageUnit,
                product.NetQuantity,
                product.NetQuantityUnit,
                product.SellingPrice),
            detail.Information is null
                ? null
                : new ProductInformationResponse(
                    detail.Information.Description,
                    detail.Information.Highlights,
                    detail.Information.Directions,
                    detail.Information.Warnings,
                    detail.Information.StorageInstructions,
                    detail.Information.SideEffects,
                    detail.Information.HowToUse,
                    detail.Information.Specifications
                        .Select(static specification =>
                            new ProductInformationSpecificationResponse(
                                specification.Key,
                                specification.Value))
                        .ToList(),
                    detail.Information.Faqs
                        .Select(static faq =>
                            new ProductInformationFaqResponse(
                                faq.Question,
                                faq.Answer))
                        .ToList()),
            detail.Availability is null
                ? null
                : new ProductDetailAvailabilityResponse(
                    detail.Availability.Pincode,
                    detail.Availability.IsServiceable,
                    detail.Availability.IsAvailable,
                    detail.Availability.AvailableQuantity,
                    detail.Availability.Mrp),
            detail.Images
                .Select(static image =>
                    new ProductDetailImageResponse(
                        image.AltText,
                        image.SortOrder,
                        image.IsPrimary,
                        image.Url))
                .ToList());
    }
}