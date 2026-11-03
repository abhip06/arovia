using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Abstractions.Authentication;

namespace HealthcareStore.Application.Auth.Login;

public sealed class LoginUserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAuthenticationSessionService authenticationSessionService)
{
    public async Task<LoginUserResponse> LoginAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Email,
            nameof(request.Email));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Password,
            nameof(request.Password));

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var result =
            await userRepository.FindForAuthenticationAsync(
                email,
                cancellationToken);

        if (result is null)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var (user, credential) = result.Value;

        if (!passwordHasher.Verify(
                request.Password,
                credential.HashedPassword))
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedException(
                "Email address must be verified before signing in.");
        }

        var session =
            await authenticationSessionService.CreateAsync(
                user,
                cancellationToken);

        return new LoginUserResponse(
            session.AccessToken,
            session.ExpiresIn,
            session.RefreshToken,
            session.RefreshTokenExpiresAt,
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString());
    }
}