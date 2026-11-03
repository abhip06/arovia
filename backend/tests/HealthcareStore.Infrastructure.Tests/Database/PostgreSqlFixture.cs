using HealthcareStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace HealthcareStore.Infrastructure.Tests.Database;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
    new PostgreSqlBuilder("postgres:15.1")
    .WithDatabase("healthcare_store_test")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .Build();

    public string ConnectionString =>
    _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        return new ApplicationDbContext(options);
    }

}
