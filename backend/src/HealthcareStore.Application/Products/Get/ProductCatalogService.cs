using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Pagination;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Location;

namespace HealthcareStore.Application.Products.Get;

public sealed class ProductCatalogService(
    IProductCatalogRepository repository,
    IProductVariantMediaRepository mediaRepository,
    IWarehouseServiceabilityRepository serviceabilityRepository,
    IWarehouseStockRepository warehouseStockRepository,
    ICacheService cacheService)
{
    public async Task<PagedResult<ProductCatalogItem>> GetPageAsync(
        ProductCatalogFilter filter,
        int page,
        PincodeContext? pincodeContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be greater than or equal to 1.");
        }

        var pageSize =
            PaginationDefaults.StoreFrontProductCatalogPageSize;

        var pincode =
            pincodeContext?.Pincode;

        var cacheKey =
            BuildCacheKey(
                filter,
                page,
                pageSize,
                pincode);

        return await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
                await BuildPageAsync(
                    filter,
                    page,
                    pageSize,
                    pincode,
                    ct),
            expiration: pincode is null
                ? TimeSpan.FromMinutes(2)
                : TimeSpan.FromSeconds(30),
            localCacheExpiration: TimeSpan.FromSeconds(30),
            tags:
            [
                CacheTags.ProductCatalog
            ],
            cancellationToken: cancellationToken);
    }

    private async Task<PagedResult<ProductCatalogItem>> BuildPageAsync(
        ProductCatalogFilter filter,
        int page,
        int pageSize,
        string? pincode,
        CancellationToken cancellationToken)
    {
        var skip =
            (page - 1) * pageSize;

        var now =
            DateTimeOffset.UtcNow;

        var (rows, totalCount) =
            await repository.GetPageAsync(
                filter,
                skip,
                pageSize,
                now,
                cancellationToken);

        var variantIds =
            rows
                .Select(static row => row.ProductVariantId)
                .ToArray();

        var imageUrls =
            await mediaRepository.GetPrimaryUrlsAsync(
                variantIds,
                cancellationToken);

        var availabilityByVariant =
            new Dictionary<Guid, ProductCatalogAvailabilityRow>();

        if (pincode is not null)
        {
            var warehouseIds =
                await serviceabilityRepository
                    .GetWarehouseIdsByPincodeAsync(
                        pincode,
                        cancellationToken);

            if (warehouseIds.Count > 0)
            {
                var availability =
                    await warehouseStockRepository
                        .GetAvailableCatalogVariantsAsync(
                            variantIds,
                            warehouseIds,
                            DateOnly.FromDateTime(
                                now.UtcDateTime),
                            cancellationToken);

                availabilityByVariant =
                    availability.ToDictionary(
                        static x => x.ProductVariantId);
            }
        }

        var items =
            rows
                .Select(row =>
                {
                    ProductCatalogAvailabilityRow? availability = null;

                    var isAvailable =
                        pincode is not null &&
                        availabilityByVariant.TryGetValue(
                            row.ProductVariantId,
                            out availability);

                    return new ProductCatalogItem(
                        row.ProductVariantId,
                        row.ProductName,
                        row.ProductSlug,
                        row.BrandName,
                        row.Sku,
                        row.PackageQuantity,
                        row.PackageUnit,
                        row.NetQuantity,
                        row.NetQuantityUnit,
                        isAvailable
                            ? availability!.Mrp
                            : null,
                        row.SellingPrice,
                        pincode is null
                            ? null
                            : isAvailable,
                        imageUrls.TryGetValue(
                            row.ProductVariantId,
                            out var imageUrl)
                            ? imageUrl
                            : null);
                })
                .ToList();

        return new PagedResult<ProductCatalogItem>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static string BuildCacheKey(
        ProductCatalogFilter filter,
        int page,
        int pageSize,
        string? pincode)
    {
        var builder = new StringBuilder();

        Append(builder, page.ToString(CultureInfo.InvariantCulture));
        Append(builder, pageSize.ToString(CultureInfo.InvariantCulture));
        Append(builder, filter.ProductType);
        Append(builder, filter.BrandSlug);
        Append(builder, filter.ManufacturerSlug);
        Append(builder, filter.CategorySlug);
        Append(builder, filter.Search);
        Append(builder, pincode);

        var fingerprint =
            Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(builder.ToString())));

        return CacheKeys.Products(fingerprint);
    }

    private static void Append(
        StringBuilder builder,
        string? value)
    {
        value ??= string.Empty;

        builder
            .Append(value.Length)
            .Append(':')
            .Append(value)
            .Append(';');
    }
}