using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Domain.Tests.Auth;

public class PasswordCredentialTests
{
    [Fact]
    public void Create_ShouldCreateCredential_WithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string hashedPassword = "hashed-password";

        // Act
        var credential = PasswordCredential.Create(
            userId,
            hashedPassword);

        // Assert
        Assert.NotEqual(Guid.Empty, credential.Id);
        Assert.Equal(userId, credential.UserId);
        Assert.Equal(hashedPassword, credential.HashedPassword);
        Assert.NotEqual(default, credential.CreatedAt);
        Assert.Equal(
            credential.CreatedAt,
            credential.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordCredential.Create(
                Guid.Empty,
                "hashed-password"));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenPasswordHashIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordCredential.Create(
                Guid.NewGuid(),
                ""));

        Assert.Equal("hashedPassword", exception.ParamName);
    }

    [Fact]
    public void ChangePassword_ShouldUpdateHashedPassword()
    {
        // Arrange
        var credential = PasswordCredential.Create(
            Guid.NewGuid(),
            "old-hash");

        var originalUpdatedAt = credential.UpdatedAt;

        // Act
        credential.ChangePassword("new-hash");

        // Assert
        Assert.Equal("new-hash", credential.HashedPassword);
        Assert.True(
            credential.UpdatedAt >= originalUpdatedAt);
    }
}