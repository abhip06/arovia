using HealthcareStore.Infrastructure.Security;

namespace HealthcareStore.Infrastructure.Tests.Security;

public sealed class VerificationTokenGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnNonEmptyToken()
    {
        var generator = new TokenGenerator();

        var token = generator.Generate();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void Generate_ShouldGenerateDifferentTokens()
    {
        var generator = new TokenGenerator();

        var firstToken = generator.Generate();
        var secondToken = generator.Generate();

        Assert.NotEqual(firstToken, secondToken);
    }

    [Fact]
    public void Hash_ShouldReturnDeterministicHash()
    {
        var generator = new TokenGenerator();
        const string token = "verification-token";

        var firstHash = generator.Hash(token);
        var secondHash = generator.Hash(token);

        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashForDifferentTokens()
    {
        var generator = new TokenGenerator();

        var firstHash = generator.Hash("verification-token-1");
        var secondHash = generator.Hash("verification-token-2");

        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void Hash_ShouldThrow_WhenTokenIsEmpty()
    {
        var generator = new TokenGenerator();

        var exception = Assert.Throws<ArgumentException>(
            () => generator.Hash(""));

        Assert.Equal("token", exception.ParamName);
    }

    [Fact]
    public void Hash_ShouldThrow_WhenTokenIsWhitespace()
    {
        var generator = new TokenGenerator();

        var exception = Assert.Throws<ArgumentException>(
            () => generator.Hash("   "));

        Assert.Equal("token", exception.ParamName);
    }

    [Fact]
    public void Generate_ShouldReturnUrlSafeToken()
    {
        var generator = new TokenGenerator();

        var token = generator.Generate();

        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void Generate_ShouldProduceExpectedTokenLength()
    {
        var generator = new TokenGenerator();

        var token = generator.Generate();

        Assert.Equal(43, token.Length);
    }

    [Fact]
    public void Hash_ShouldReturnExpectedHashLength()
    {
        var generator = new TokenGenerator();

        var hash = generator.Hash("verification-token");

        Assert.Equal(64, hash.Length);
    }
}
