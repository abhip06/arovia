using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Auth.ExternalLogin;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Auth.AuthSession;

public sealed class ExternalLoginUserService(
    IExternalIdentityRepository externalIdentityRepository,
    IUserRepository userRepository,
    IAuthenticationSessionService authenticationSessionService)
{
    public async Task<ExternalLoginUserResponse> LoginAsync(
        ExternalLoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.ProviderSubject,
            nameof(request.ProviderSubject));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Email,
            nameof(request.Email));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.FirstName,
            nameof(request.FirstName));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.LastName,
            nameof(request.LastName));

        var existingIdentity =
            await externalIdentityRepository.FindAsync(
                ExternalIdentityProvider.Google,
                request.ProviderSubject,
                cancellationToken);

        if (existingIdentity is not null)
        {
            return await CreateResponseAsync(
                existingIdentity.Value.User,
                cancellationToken);
        }

        var normalizedEmail =
            request.Email.Trim();

        var existingUser =
            await userRepository.FindByEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException(
                "An account with this email already exists. " +
                "Sign in with your existing account and link Google.");
        }

        var user = User.Create(
            request.FirstName,
            request.LastName,
            normalizedEmail);

        user.VerifyEmail();

        var externalIdentity =
            ExternalIdentity.Create(
                user.Id,
                ExternalIdentityProvider.Google,
                request.ProviderSubject);

        await userRepository.AddExternalUserAsync(
            user,
            externalIdentity,
            cancellationToken);

        return await CreateResponseAsync(
            user,
            cancellationToken);
    }

    private async Task<ExternalLoginUserResponse> CreateResponseAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var session =
            await authenticationSessionService.CreateAsync(
                user,
                cancellationToken);

        return new ExternalLoginUserResponse(
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