using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.Addresses;

public sealed class AddressService(
    IAddressRepository addressRepository)
{
    private readonly IAddressRepository _addressRepository = addressRepository;

    public async Task<IReadOnlyList<AddressReadModel>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var addresses =
            await _addressRepository.GetAllByUserIdAsync(
                userId,
                cancellationToken);

        return addresses
            .Select(Map)
            .ToList();
    }

    public async Task<AddressReadModel> CreateAsync(
        Guid userId,
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(request);

        var address = Address.Create(
            userId,
            request.Label,
            request.RecipientName,
            request.RecipientPhone,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            false);

        await _addressRepository.AddAsync(
            address,
            cancellationToken);

        return Map(address);
    }

    public async Task<AddressReadModel> UpdateAsync(
        Guid userId,
        Guid addressId,
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateAddressId(addressId);
        ArgumentNullException.ThrowIfNull(request);

        var address =
            await _addressRepository.GetByIdAsync(
                userId,
                addressId,
                cancellationToken);

        if (address is null)
        {
            throw new NotFoundException(
                "Address was not found.");
        }

        address.Update(
            request.Label,
            request.RecipientName,
            request.RecipientPhone,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        await _addressRepository.SaveChangesAsync(
            cancellationToken);

        return Map(address);
    }

    public async Task RemoveAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateAddressId(addressId);

        var deleted =
            await _addressRepository.DeleteAsync(
                userId,
                addressId,
                cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException(
                "Address was not found.");
        }
    }

    public async Task SetPrimaryAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateAddressId(addressId);

        var updated =
            await _addressRepository.SetPrimaryAsync(
                userId,
                addressId,
                cancellationToken);

        if (!updated)
        {
            throw new NotFoundException(
                "Address was not found.");
        }
    }

    private static AddressReadModel Map(Address address)
    {
        return new AddressReadModel(
            address.Id,
            address.UserId,
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

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }
    }

    private static void ValidateAddressId(Guid addressId)
    {
        if (addressId == Guid.Empty)
        {
            throw new ArgumentException(
                "Address ID cannot be empty.",
                nameof(addressId));
        }
    }
}