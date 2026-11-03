using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Auth.Register;

public sealed class RegisterUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _verificationTokenGenerator;
    private readonly IEmailSender _emailSender;

    public RegisterUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator verificationTokenGenerator,
        IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(verificationTokenGenerator);
        ArgumentNullException.ThrowIfNull(emailSender);

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _verificationTokenGenerator = verificationTokenGenerator;
        _emailSender = emailSender;
    }

    public async Task<RegisterUserResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // validation
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();

        // check user already exist
        if (await _userRepository.ExistsByEmailAsync(
                email,
                cancellationToken))
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var user = User.Create(
            request.FirstName,
            request.LastName,
            email);

        var hashedPassword = _passwordHasher.Hash(
            request.Password);

        var passwordCredential = PasswordCredential.Create(
            user.Id,
            hashedPassword);

        var verificationToken =
            _verificationTokenGenerator.Generate();

        var verificationTokenHash =
            _verificationTokenGenerator.Hash(
                verificationToken);

        var emailVerificationToken =
            EmailVerificationToken.Create(
                user.Id,
                verificationTokenHash,
                DateTimeOffset.UtcNow.AddHours(24));

        await _userRepository.AddAsync(
            user,
            passwordCredential,
            emailVerificationToken,
            cancellationToken);

        await _emailSender.SendEmailVerificationAsync(
            user.Email,
            user.FirstName,
            verificationToken,
            cancellationToken);

        return new RegisterUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString(),
            user.IsEmailVerified);
    }
}