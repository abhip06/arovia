using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Application.Users.Update;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using Moq;

namespace HealthcareStore.Application.Tests.Users.Update;

public sealed class UpdateCurrentUserServiceTests
{
    [Fact]
    public async Task UpdateAsync_ShouldUpdateProfile_WhenRequestIsValid()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var repository = new FakeUserRepository
        {
            User = user
        };

        var service = CreateService(repository, out var cache);

        var response = await service.UpdateAsync(
            new UpdateCurrentUserRequest(
                user.Id,
                "Johnny",
                "Smith",
                "+919876543210",
                Gender.Male,
                new DateOnly(1998, 5, 20)));

        Assert.Equal("Johnny", response.FirstName);
        Assert.Equal("Smith", response.LastName);
        Assert.Equal("+919876543210", response.Phone);
        Assert.Equal(Gender.Male, response.Gender);
        Assert.Equal(
            new DateOnly(1998, 5, 20),
            response.DateOfBirth);

        Assert.Equal(1, repository.SaveChangesCallCount);

        cache.Verify(
            service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldAllowSamePhoneNumber()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.UpdateProfile(
            "John",
            "Doe",
            "+919876543210",
            null,
            null);

        var repository = new FakeUserRepository
        {
            User = user,
            PhoneExists = true
        };

        var service = CreateService(repository, out var cache);

        await service.UpdateAsync(
            new UpdateCurrentUserRequest(
                user.Id,
                "John",
                "Doe",
                "+919876543210",
                null,
                null));

        Assert.Equal(1, repository.SaveChangesCallCount);

        cache.Verify(
            service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectDuplicatePhoneNumber()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var repository = new FakeUserRepository
        {
            User = user,
            PhoneExists = true
        };

        var service = CreateService(repository, out var cache);

        var exception =
            await Assert.ThrowsAsync<ConflictException>(
                () => service.UpdateAsync(
                    new UpdateCurrentUserRequest(
                        user.Id,
                        "John",
                        "Doe",
                        "+919876543210",
                        null,
                        null)));

        Assert.Equal(
            "This phone number is already associated with another account.",
            exception.Message);

        Assert.Equal(0, repository.SaveChangesCallCount);

        cache.Verify(
            service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldClearPhone_WhenPhoneIsEmpty()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.UpdateProfile(
            "John",
            "Doe",
            "+919876543210",
            null,
            null);

        var repository = new FakeUserRepository
        {
            User = user
        };

        var service = CreateService(repository, out var cache);

        var response = await service.UpdateAsync(
            new UpdateCurrentUserRequest(
                user.Id,
                "John",
                "Doe",
                "   ",
                null,
                null));

        Assert.Null(response.Phone);
        Assert.Null(user.Phone);
        Assert.Equal(1, repository.SaveChangesCallCount);

        cache.Verify(
            service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectUnknownUser()
    {
        var repository = new FakeUserRepository();

        var service = CreateService(repository, out var cache);

        var exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => service.UpdateAsync(
                    new UpdateCurrentUserRequest(
                        Guid.NewGuid(),
                        "John",
                        "Doe",
                        null,
                        null,
                        null)));

        Assert.Equal("User was not found.", exception.Message);
        Assert.Equal(0, repository.SaveChangesCallCount);

        cache.Verify(
            service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectEmptyUserId()
    {
        var repository = new FakeUserRepository();

        var service = CreateService(repository, out var cache);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(
                new UpdateCurrentUserRequest(
                    Guid.Empty,
                    "John",
                    "Doe",
                    null,
                    null,
                    null)));

        Assert.Equal(0, repository.SaveChangesCallCount);

        cache.Verify(
            service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static UpdateCurrentUserService CreateService(
        FakeUserRepository repository,
        out Mock<ICacheService> cache)
    {
        cache = new Mock<ICacheService>();

        cache
            .Setup(service => service.RemoveByTagAsync(
                CacheTags.Users,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        return new UpdateCurrentUserService(
            repository,
            cache.Object);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; set; }

        public bool PhoneExists { get; set; }

        public int SaveChangesCallCount { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ExistsByPhoneAsync(
            string phone,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PhoneExists);
        }

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<string?> GetPhoneAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<GetAllUsersDto> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<(User User, PasswordCredential Credential)?>
            FindForAuthenticationAsync(
                string email,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<User?> FindByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(User);
        }

        public Task AddAsync(
            User user,
            PasswordCredential credential,
            EmailVerificationToken emailVerificationToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddExternalUserAsync(
            User user,
            ExternalIdentity externalIdentity,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }

        public Task<PasswordCredential?> FindPasswordCredentialByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}