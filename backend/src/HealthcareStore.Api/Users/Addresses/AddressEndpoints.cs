using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Users.Addresses;
using System.Security.Claims;

namespace HealthcareStore.Api.Users.Addresses;

public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/me/addresses")
            .RequireAuthorization()
            .WithTags("Addresses");

        group.MapGet(
            "",
            GetAllAsync);

        group.MapPost(
            "",
            CreateAsync);

        group.MapPut(
            "/{addressId:guid}",
            UpdateAsync);

        group.MapDelete(
            "/{addressId:guid}",
            DeleteAsync);

        group.MapPut(
            "/{addressId:guid}/primary",
            SetPrimaryAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        ClaimsPrincipal user,
        AddressService addressService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserId();

        var addresses =
            await addressService.GetAllAsync(
                userId,
                cancellationToken);

        var response = addresses
            .Select(Map)
            .ToList();

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> CreateAsync(
        ClaimsPrincipal user,
        CreateAddressApiRequest request,
        AddressService addressService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserId();

        var result =
            await addressService.CreateAsync(
                userId,
                new Application.Users.Addresses.CreateAddressRequest(
                    request.Label,
                    request.RecipientName,
                    request.RecipientPhone,
                    request.AddressLine1,
                    request.AddressLine2,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country),
                cancellationToken);

        return TypedResults.Created(
            "/api/v1/me/addresses",
            Map(result));
    }

    private static async Task<IResult> UpdateAsync(
        ClaimsPrincipal user,
        Guid addressId,
        UpdateAddressApiRequest request,
        AddressService addressService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserId();

        var result =
            await addressService.UpdateAsync(
                userId,
                addressId,
                new Application.Users.Addresses.UpdateAddressRequest(
                    request.Label,
                    request.RecipientName,
                    request.RecipientPhone,
                    request.AddressLine1,
                    request.AddressLine2,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country),
                cancellationToken);

        return TypedResults.Ok(Map(result));
    }

    private static async Task<IResult> DeleteAsync(
        ClaimsPrincipal user,
        Guid addressId,
        AddressService addressService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserId();

        await addressService.RemoveAsync(
            userId,
            addressId,
            cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<IResult> SetPrimaryAsync(
        ClaimsPrincipal user,
        Guid addressId,
        AddressService addressService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserId();

        await addressService.SetPrimaryAsync(
            userId,
            addressId,
            cancellationToken);

        return TypedResults.NoContent();
    }

    private static AddressApiResponse Map(
        AddressReadModel address)
    {
        return new AddressApiResponse(
            address.Id,
            address.Label,
            address.RecipientName,
            address.RecipientPhone,
            address.AddressLine1,
            address.AddressLine2,
            address.City,
            address.State,
            address.PostalCode,
            address.Country,
            address.IsPrimary,
            address.CreatedAt,
            address.UpdatedAt);
    }
}