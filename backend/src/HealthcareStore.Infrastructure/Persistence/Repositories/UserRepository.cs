using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AnyAsync(user => user.Phone == phone, cancellationToken);
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return _dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);
    }

    public async Task<User?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
    }

    public async Task<GetAllUsersDto> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Select(user => new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.IsEmailVerified
            ))
            .ToListAsync(cancellationToken);

        return new GetAllUsersDto(
            users,
            users.Count
        );
    }

    public Task<string?> GetPhoneAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.Phone)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(User User, PasswordCredential Credential)?> FindForAuthenticationAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var result = await _dbContext.Users
            .Join(
                _dbContext.PasswordCredentials,
                user => user.Id,
                credential => credential.UserId,
                (user, credential) => new
                {
                    User = user,
                    Credential = credential
                })
            .SingleOrDefaultAsync(
                x => x.User.Email == email,
                cancellationToken);

        return result is null
            ? null
            : (result.User, result.Credential);
    }

    public async Task<PasswordCredential?> FindPasswordCredentialByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordCredentials
            .SingleOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(
    User user,
    PasswordCredential passwordCredential,
    EmailVerificationToken emailVerificationToken,
    CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken);

        await _dbContext.PasswordCredentials.AddAsync(
            passwordCredential,
            cancellationToken);

        await _dbContext.EmailVerificationTokens.AddAsync(
            emailVerificationToken,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task AddExternalUserAsync(
        User user,
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(identity);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        _dbContext.Users.Add(user);
        _dbContext.ExternalIdentities.Add(identity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}