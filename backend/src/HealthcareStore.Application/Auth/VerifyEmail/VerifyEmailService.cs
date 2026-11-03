using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Auth.VerifyEmail;

public sealed class VerifyEmailService
{
    private readonly IEmailVerificationRepository _repository;
    private readonly ITokenGenerator _tokenGenerator;

    public VerifyEmailService(
        IEmailVerificationRepository repository,
        ITokenGenerator tokenGenerator)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(tokenGenerator);

        _repository = repository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<VerifyEmailResponse> VerifyAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Token,
            nameof(request.Token));

        var tokenHash = _tokenGenerator.Hash(request.Token);

        var result = await _repository.FindAsync(
            tokenHash,
            cancellationToken);

        if (result is null)
        {
            throw new InvalidVerificationTokenException(
                "The email verification token is invalid.");
        }

        var (user, token) = result.Value;
        var now = DateTimeOffset.UtcNow;

        if (token.UsedAt.HasValue)
        {
            throw new InvalidVerificationTokenException(
                "The email verification token has already been used.");
        }

        if (token.RevokedAt.HasValue)
        {
            throw new InvalidVerificationTokenException(
                "The email verification token has been revoked.");
        }

        if (token.ExpiresAt <= now)
        {
            throw new InvalidVerificationTokenException(
                "The email verification token has expired.");
        }

        if (user.IsEmailVerified)
        {
            return new VerifyEmailResponse(
                user.Id,
                user.Email,
                true);
        }

        user.VerifyEmail();
        token.MarkUsed();

        var completed = await _repository.TryCompleteAsync(
            user,
            token,
            cancellationToken);

        if (!completed)
        {
            throw new InvalidVerificationTokenException(
                "The email verification token is no longer valid.");
        }

        return new VerifyEmailResponse(
            user.Id,
            user.Email,
            user.IsEmailVerified);
    }
}