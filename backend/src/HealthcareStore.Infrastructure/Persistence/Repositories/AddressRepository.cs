using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Location;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class AddressRepository(
    ApplicationDbContext dbContext) : IAddressRepository
{
    private readonly ApplicationDbContext _dbContext =
        dbContext;

    public async Task<IReadOnlyList<Address>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        return await _dbContext.Addresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetByIdAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateAddressId(addressId);

        return await _dbContext.Addresses
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id == addressId,
                cancellationToken);
    }

    public async Task<SavedAddressLocation?> GetPrimaryLocationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        return await _dbContext.Addresses
            .AsNoTracking()
            .Where(address =>
                address.UserId == userId &&
                address.IsPrimary)
            .Select(address => new SavedAddressLocation(
                address.PostalCode,
                address.City,
                address.State,
                address.Country))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        Address address,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(address);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var user = await _dbContext.Users
            .FromSqlInterpolated(
                $"SELECT * FROM users WHERE \"Id\" = {address.UserId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        var hasExistingAddress =
            await _dbContext.Addresses
                .AnyAsync(
                    x => x.UserId == address.UserId,
                    cancellationToken);

        if (!hasExistingAddress)
        {
            address.SetPrimary();
        }

        await _dbContext.Addresses.AddAsync(
            address,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateAddressId(addressId);

        var deleted = await _dbContext.Addresses
            .Where(
                x =>
                    x.UserId == userId &&
                    x.Id == addressId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task<bool> SetPrimaryAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateAddressId(addressId);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var now = DateTimeOffset.UtcNow;

        await _dbContext.Addresses
            .Where(
                x =>
                    x.UserId == userId &&
                    x.IsPrimary &&
                    x.Id != addressId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.IsPrimary,
                        false)
                    .SetProperty(
                        x => x.UpdatedAt,
                        now),
                cancellationToken);

        var updated = await _dbContext.Addresses
            .Where(
                x =>
                    x.UserId == userId &&
                    x.Id == addressId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.IsPrimary,
                        true)
                    .SetProperty(
                        x => x.UpdatedAt,
                        now),
                cancellationToken);

        if (updated == 0)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return false;
        }

        await transaction.CommitAsync(
            cancellationToken);

        return true;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
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