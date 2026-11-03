using HealthcareStore.Api.Admin.Warehouses.Serviceable;
using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Inventory.Warehouses.Serviceable;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Api.Admin.Warehouses.Serviceable;

public static class WarehouseServiceabilityEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseServiceabilityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group =
            endpoints
                .MapGroup(
                    "/api/v1/admin/warehouses")
                .RequireAuthorization(
                    AuthorizationPolicies.AdminOnly);

        group.MapPost(
                "/{warehouseId:guid}/serviceable-pincodes",
                AddAsync);

        group.MapDelete(
                "/{warehouseId:guid}/serviceable-pincodes/{pincode}",
                RemoveAsync);

        return endpoints;
    }

    private static async Task<
        IResult>
        AddAsync(
            Guid warehouseId,
            AddWarehouseServiceablePincodesApiRequest request,
            ManageWarehouseServiceabilityService service,
            CancellationToken cancellationToken)
    {
        var applicationRequest =
            new AddWarehouseServiceablePincodesRequest(
                request.Pincodes);

        var result =
            await service.AddAsync(
                warehouseId,
                applicationRequest,
                cancellationToken);

        return Results.Ok(
            new AddWarehouseServiceablePincodesApiResponse(
                result.RequestedCount,
                result.AddedCount));
    }

    private static async Task<
        IResult>
        RemoveAsync(
            Guid warehouseId,
            string pincode,
            ManageWarehouseServiceabilityService service,
            CancellationToken cancellationToken)
    {
        var result =
            await service.RemoveAsync(
                warehouseId,
                pincode,
                cancellationToken);

        return Results.Ok(
            new RemoveWarehouseServiceablePincodeApiResponse(
                result.Pincode,
                result.Removed));
    }
}