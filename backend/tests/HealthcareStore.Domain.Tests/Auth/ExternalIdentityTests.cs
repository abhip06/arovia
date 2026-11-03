using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Domain.Tests.Auth;

public sealed class ExternalIdentityTests
{
    [Fact]
    public void Create_ShouldCreateIdentity_WithValidData()
    {
        var userId = Guid.NewGuid();
        const string providerSubject = "google-subject-123";

        var identity = ExternalIdentity.Create(
            userId,
            ExternalIdentityProvider.Google,
            providerSubject);

        Assert.NotEqual(Guid.Empty, identity.Id);
        Assert.Equal(userId, identity.UserId);
        Assert.Equal(
            ExternalIdentityProvider.Google,
            identity.Provider);
        Assert.Equal(providerSubject, identity.ProviderSubject);
        Assert.NotEqual(default, identity.CreatedAt);
        Assert.Equal(identity.CreatedAt, identity.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldTrimProviderSubject()
    {
        var identity = ExternalIdentity.Create(
            Guid.NewGuid(),
            ExternalIdentityProvider.Google,
            "  google-subject-123  ");

        Assert.Equal(
            "google-subject-123",
            identity.ProviderSubject);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ExternalIdentity.Create(
                Guid.Empty,
                ExternalIdentityProvider.Google,
                "google-subject-123"));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenProviderSubjectIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ExternalIdentity.Create(
                Guid.NewGuid(),
                ExternalIdentityProvider.Google,
                ""));

        Assert.Equal("providerSubject", exception.ParamName);
    }
}
