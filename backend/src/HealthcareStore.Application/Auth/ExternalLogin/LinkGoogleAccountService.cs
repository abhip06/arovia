using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Application.Auth.ExternalLogin;

public sealed class LinkGoogleAccountService(
    IUserRepository userRepository,
    IExternalIdentityRepository externalIdentityRepository)
{
    public async Task LinkAsync(
        LinkGoogleAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(request.UserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.ProviderSubject,
            nameof(request.ProviderSubject));

        var user =
            await userRepository.FindByIdAsync(
                request.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        var existingIdentity =
            await externalIdentityRepository.FindAsync(
                ExternalIdentityProvider.Google,
                request.ProviderSubject,
                cancellationToken);

        if (existingIdentity is not null)
        {
            if (existingIdentity.Value.Identity.UserId ==
                request.UserId)
            {
                return;
            }

            throw new ConflictException(
                "This Google account is already linked to another account.");
        }

        var existingUserGoogleIdentity =
            await externalIdentityRepository.FindByUserAsync(
                request.UserId,
                ExternalIdentityProvider.Google,
                cancellationToken);

        if (existingUserGoogleIdentity is not null)
        {
            throw new ConflictException(
                "A Google account is already linked to this account.");
        }

        var externalIdentity =
            ExternalIdentity.Create(
                request.UserId,
                ExternalIdentityProvider.Google,
                request.ProviderSubject);

        var added =
            await externalIdentityRepository.TryAddAsync(
                externalIdentity,
                cancellationToken);

        if (!added)
        {
            throw new ConflictException(
                "This Google account is already linked to another account.");
        }
    }
}