using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ManufacturerRepository : IManufacturerRepository
{
    private const string SlugIndexName = "IX_manufacturers_Slug";

    private readonly ApplicationDbContext _dbContext;

    public ManufacturerRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsActiveAsync(
        Guid manufacturerId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Manufacturers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == manufacturerId &&
                    x.Status == CatalogEntityStatus.Active,
                cancellationToken);
    }

    public async Task<ManufacturerInsertResult> TryCreateAsync(
        Manufacturer manufacturer,
        CancellationToken cancellationToken)
    {
        _dbContext.Manufacturers.Add(manufacturer);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ManufacturerInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, SlugIndexName))
        {
            _dbContext.Detach(manufacturer);

            return ManufacturerInsertResult.DuplicateSlug;
        }
    }

    public Task<Manufacturer?> GetForDeleteAsync(
    Guid manufacturerId,
    CancellationToken cancellationToken)
    {
        return _dbContext.Manufacturers
            .FirstOrDefaultAsync(
                x => x.Id == manufacturerId,
                cancellationToken);
    }

    public async Task<ManufacturerDeleteResult> TryDeleteAsync(
        Manufacturer manufacturer,
        CancellationToken cancellationToken)
    {
        _dbContext.Manufacturers.Remove(manufacturer);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return ManufacturerDeleteResult.Deleted;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsForeignKeyViolation(ex))
        {
            _dbContext.Detach(manufacturer);

            return ManufacturerDeleteResult.Referenced;
        }
    }

    public async Task<IReadOnlyList<ManufacturerReadModel>> GetAllForReadAsync(
    CancellationToken cancellationToken)
    {
        return await _dbContext.Manufacturers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ManufacturerReadModel(
                x.Slug,
                x.Name))
            .ToListAsync(cancellationToken);
    }
}