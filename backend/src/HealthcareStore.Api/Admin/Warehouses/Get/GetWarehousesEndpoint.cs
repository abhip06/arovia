using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Admin.Warehouses.Get;

namespace HealthcareStore.Api.Admin.Warehouses.Get;

public static class GetWarehousesEndpoint
{
    public static IEndpointRouteBuilder MapGetWarehousesEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/api/v1/admin/warehouses",
                async (
                    int page,
                    GetWarehousesService service,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await service.GetAsync(
                            new GetWarehousesRequest(page),
                            cancellationToken);

                    var response =
                        new WarehouseListResponse(
                            result.Result.Items
                                .Select(static warehouse =>
                                    new WarehouseResponse(
                                        warehouse.Code,
                                        warehouse.Name,
                                        warehouse.AddressLine1,
                                        warehouse.AddressLine2,
                                        warehouse.City,
                                        warehouse.State,
                                        warehouse.PostalCode,
                                        warehouse.Country,
                                        warehouse.Status))
                                .ToList(),
                            result.Result.Page,
                            result.Result.PageSize,
                            result.Result.TotalCount,
                            result.Result.TotalPages);

                    return Results.Ok(response);
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .Produces<WarehouseListResponse>(StatusCodes.Status200OK);

        return endpoints;
    }
}