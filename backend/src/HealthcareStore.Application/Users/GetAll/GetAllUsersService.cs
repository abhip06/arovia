using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.GetAll;

public sealed class GetAllUsersService(
    IUserRepository userRepository,
    ICacheService cache)
{
    private static readonly TimeSpan CacheTimeout = TimeSpan.FromMinutes(5);

    public async Task<GetAllUsersDto> GetAllUsersAsync(
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(
            adminUserId,
            cancellationToken)
            ?? throw new ForbiddenException(
                "You are not allowed to perform this operation.");

        if (user.Role != UserRole.Admin)
        {
            throw new ForbiddenException(
                "You are not allowed to perform this operation.");
        }

        return await cache.GetOrCreateAsync<GetAllUsersDto>(
            CacheKeys.Users,
            ct => new ValueTask<GetAllUsersDto>(
                userRepository.GetAllAsync(ct)),
            CacheTimeout,
            null,
            [CacheTags.Users],
            cancellationToken);
    }
}