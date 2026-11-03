using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Users.GetCurrentUser;

public sealed class GetCurrentUserService(IUserRepository userRepository)
{
    public async Task<GetCurrentUserResponse> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        var user =
            await userRepository.FindByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        return new GetCurrentUserResponse(
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