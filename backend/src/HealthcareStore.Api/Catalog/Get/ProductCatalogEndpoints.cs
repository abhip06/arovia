using HealthcareStore.Api.Common.Pagination;
using HealthcareStore.Application.Abstractions.Location;
using HealthcareStore.Application.Location;
using HealthcareStore.Application.Products.Get;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareStore.Api.Catalog.Get;

public static class ProductCatalogEndpoints
{
    public static IEndpointRouteBuilder MapProductCatalogEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/catalog");

        group.MapGet("/products", GetProductsAsync)
            .WithName("GetProductCatalog")
            .WithTags("Catalog")
            .Produces<ProductCatalogResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> GetProductsAsync(
        [AsParameters] PaginationQuery pagination,
        [FromQuery(Name = "type")] string? productType,
        [FromQuery(Name = "brand")] string? brandSlug,
        [FromQuery(Name = "manufacturer")] string? manufacturerSlug,
        [FromQuery(Name = "category")] string? categorySlug,
        [FromQuery(Name = "search")] string? search,
        [FromQuery(Name = "pincode")] string? pincode,
        HttpContext httpContext,
        IPincodeContextResolver pincodeContextResolver,
        ProductCatalogService service,
        CancellationToken cancellationToken)
    {
        if (pagination.Page < 1)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["page"] =
                    ["Page must be greater than or equal to 1."]
                });
        }

        var filter = new ProductCatalogFilter(
            Normalize(productType),
            Normalize(brandSlug),
            Normalize(manufacturerSlug),
            Normalize(categorySlug),
            Normalize(search));

        var normalizedPincode = Normalize(pincode);

        var pincodeContext =
            await pincodeContextResolver.ResolveAsync(
                normalizedPincode,
                httpContext.Connection.RemoteIpAddress,
                cancellationToken);

        var result = await service.GetPageAsync(
            filter,
            pagination.Page,
            pincodeContext,
            cancellationToken);

        var response = new ProductCatalogResponse(
            result.Items
                .Select(Map)
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);

        return Results.Ok(response);
    }

    private static ProductCatalogItemResponse Map(
        ProductCatalogItem item)
    {
        return new ProductCatalogItemResponse(
            item.ProductVariantId,
            item.ProductName,
            item.ProductSlug,
            item.BrandName,
            item.Sku,
            item.PackageQuantity,
            item.PackageUnit,
            item.NetQuantity,
            item.NetQuantityUnit,
            item.Mrp,
            item.SellingPrice,
            item.ImageUrl);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}