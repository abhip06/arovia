using HealthcareStore.Application.Location;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IAddressRepository
{
    Task<IReadOnlyList<Address>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Address?> GetByIdAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<SavedAddressLocation?> GetPrimaryLocationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Address address,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<bool> SetPrimaryAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}