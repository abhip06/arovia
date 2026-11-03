using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Users;

public sealed class User : AggregateRoot
{
    private User()
    {
        // Required by EF Core.
    }

    private User(
        string firstName,
        string lastName,
        string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;

        Role = UserRole.Customer;
        IsEmailVerified = false;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string? Phone { get; private set; }

    public Gender? Gender { get; private set; }

    public DateOnly? DateOfBirth { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsEmailVerified { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static User Create(
        string firstName,
        string lastName,
        string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new User(
            firstName.Trim(),
            lastName.Trim(),
            email.Trim());
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? phone,
        Gender? gender,
        DateOnly? dateOfBirth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
        {
            return;
        }

        IsEmailVerified = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}