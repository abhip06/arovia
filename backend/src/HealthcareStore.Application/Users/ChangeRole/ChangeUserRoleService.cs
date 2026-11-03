using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.ChangeRole;

public sealed class ChangeUserRoleService(
    IUserRepository userRepository,
    ICacheService cache)
{
    public async Task ChangeUserRole(
        Guid adminUserId,
        ChangeUserRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(request.UserId));
        }

        var user = await userRepository.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new ArgumentException(
                "Invalid User ID.");

        if (!Enum.IsDefined(request.Role))
        {
            throw new ArgumentException(
                "The requested user role is invalid.",
                nameof(request.Role));
        }

        var admin = await userRepository.FindByIdAsync(
            adminUserId,
            cancellationToken);

        if (admin is null || admin.Role != UserRole.Admin)
        {
            throw new ForbiddenException(
                "You are not allowed to perform this operation.");
        }

        if (adminUserId == request.UserId)
        {
            throw new ForbiddenException(
                "You are not allowed to perform this operation.");
        }

        user.ChangeRole(request.Role);

        await userRepository.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Users,
            CancellationToken.None);
    }
}