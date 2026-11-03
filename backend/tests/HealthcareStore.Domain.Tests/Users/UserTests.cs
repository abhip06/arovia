using HealthcareStore.Domain.Users;

namespace HealthcareStore.Domain.Tests.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldCreateCustomer_WithValidData()
    {
        const string firstName = "Ankit";
        const string lastName = "Sharma";
        const string email = "ankit.sharma@example.com";

        var user = User.Create(
            firstName,
            lastName,
            email);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(email, user.Email);
        Assert.Null(user.Phone);
        Assert.Null(user.Gender);
        Assert.Null(user.DateOfBirth);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.False(user.IsEmailVerified);
        Assert.NotEqual(default, user.CreatedAt);
        Assert.Equal(user.CreatedAt, user.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldTrimInput()
    {
        var user = User.Create(
            "  John  ",
            "  Doe  ",
            "  john@example.com  ");

        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("john@example.com", user.Email);
    }

    [Fact]
    public void Create_ShouldThrow_WhenFirstNameIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(
                "",
                "Doe",
                "john@example.com"));

        Assert.Equal("firstName", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenLastNameIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(
                "John",
                "",
                "john@example.com"));

        Assert.Equal("lastName", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmailIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(
                "John",
                "Doe",
                ""));

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateProfileInformation()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var originalUpdatedAt = user.UpdatedAt;
        var dateOfBirth = new DateOnly(1998, 5, 15);

        user.UpdateProfile(
            "Jane",
            "Smith",
            "9123456780",
            Gender.Female,
            dateOfBirth);

        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal("9123456780", user.Phone);
        Assert.Equal(Gender.Female, user.Gender);
        Assert.Equal(dateOfBirth, user.DateOfBirth);
        Assert.True(user.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void UpdateProfile_ShouldAllowOptionalProfileDataToRemainNull()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.UpdateProfile(
            "John",
            "Doe",
            null,
            null,
            null);

        Assert.Null(user.Phone);
        Assert.Null(user.Gender);
        Assert.Null(user.DateOfBirth);
    }

    [Fact]
    public void VerifyEmail_ShouldMarkEmailAsVerified()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        Assert.False(user.IsEmailVerified);

        user.VerifyEmail();

        Assert.True(user.IsEmailVerified);
    }

    [Fact]
    public void VerifyEmail_ShouldBeIdempotent()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();
        var updatedAt = user.UpdatedAt;

        user.VerifyEmail();

        Assert.True(user.IsEmailVerified);
        Assert.Equal(updatedAt, user.UpdatedAt);
    }

    [Fact]
    public void ChangeRole_ShouldChangeUserRole()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.ChangeRole(UserRole.Pharmacist);

        Assert.Equal(UserRole.Pharmacist, user.Role);
    }
}