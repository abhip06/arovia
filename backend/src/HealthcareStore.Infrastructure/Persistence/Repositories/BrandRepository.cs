using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class BrandRepository : IBrandRepository
{
    private const string SlugIndexName = "IX_brands_Slug";

    private readonly ApplicationDbContext _dbContext;

    public BrandRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsActiveAsync(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Brands
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == brandId &&
                    x.Status == CatalogEntityStatus.Active,
                cancellationToken);
    }

    public async Task<BrandInsertResult> TryCreateAsync(
        Brand brand,
        CancellationToken cancellationToken)
    {
        _dbContext.Brands.Add(brand);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return BrandInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, SlugIndexName))
        {
            _dbContext.Detach(brand);

            return BrandInsertResult.DuplicateSlug;
        }
    }

    public Task<Brand?> GetForDeleteAsync(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Brands
            .FirstOrDefaultAsync(
                x => x.Id == brandId,
                cancellationToken);
    }

    public async Task<BrandDeleteResult> TryDeleteAsync(
        Brand brand,
        CancellationToken cancellationToken)
    {
        _dbContext.Brands.Remove(brand);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return BrandDeleteResult.Deleted;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsForeignKeyViolation(ex))
        {
            _dbContext.Detach(brand);

            return BrandDeleteResult.Referenced;
        }
    }

    public async Task<IReadOnlyList<BrandReadModel>> GetAllForReadAsync(
    CancellationToken cancellationToken)
    {
        return await _dbContext.Brands
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BrandReadModel(
                x.Slug,
                x.Name))
            .ToListAsync(cancellationToken);
    }
}