using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Users.Update;

public sealed class UpdateCurrentUserService(
    IUserRepository userRepository,
    ICacheService cache)
{
    public async Task<UpdateCurrentUserResponse> UpdateAsync(
        UpdateCurrentUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(request.UserId));
        }

        var user =
            await userRepository.FindByIdAsync(
                request.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        var phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : request.Phone.Trim();

        if (phone is not null &&
            !string.Equals(
                phone,
                user.Phone,
                StringComparison.Ordinal))
        {
            var phoneExists =
                await userRepository.ExistsByPhoneAsync(
                    phone,
                    cancellationToken);

            if (phoneExists)
            {
                throw new ConflictException(
                    "This phone number is already associated with another account.");
            }
        }

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            phone,
            request.Gender,
            request.DateOfBirth);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Users,
            cancellationToken);

        return new UpdateCurrentUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Gender,
            user.DateOfBirth,
            user.Role.ToString(),
            user.IsEmailVerified);
    }
}