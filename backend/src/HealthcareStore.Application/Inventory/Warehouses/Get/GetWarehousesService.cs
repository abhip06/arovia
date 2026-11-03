using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Pagination;
using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Admin.Warehouses.Get;

public sealed class GetWarehousesService(
    IWarehouseRepository warehouseRepository,
    ICacheService cache)
{
    public async Task<GetWarehousesResponse> GetAsync(
        GetWarehousesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.Page,
            "Page must be greater than zero.");

        const int pageSize =
            PaginationDefaults.WarehousePageSize;

        var fingerprint =
            CacheKeyFingerprint.Create(request.Page);

        var result =
            await cache.GetOrCreateAsync(
                CacheKeys.Warehouses(fingerprint),
                async token =>
                {
                    var page =
                        await warehouseRepository.GetPageForReadAsync(
                            request.Page,
                            pageSize,
                            token);

                    var items = page.Items
                        .Select(static warehouse =>
                            new WarehouseItem(
                                warehouse.Code,
                                warehouse.Name,
                                warehouse.AddressLine1,
                                warehouse.AddressLine2,
                                warehouse.City,
                                warehouse.State,
                                warehouse.PostalCode,
                                warehouse.Country,
                                warehouse.Status))
                        .ToList();

                    return new GetWarehousesResponse(
                        new PagedResult<WarehouseItem>(
                            items,
                            page.Page,
                            page.PageSize,
                            page.TotalCount));
                },
                expiration: TimeSpan.FromMinutes(10),
                localCacheExpiration: TimeSpan.FromSeconds(30),
                tags:
                [
                    CacheTags.Warehouses
                ],
                cancellationToken: cancellationToken);

        return result;
    }
}