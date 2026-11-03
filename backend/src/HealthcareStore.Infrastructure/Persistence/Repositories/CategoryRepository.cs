using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    private const string SlugIndexName = "IX_categories_ProductType_Slug";

    public Task<CategoryParentInfo?> GetParentAsync(
        Guid parentCategoryId,
        CancellationToken cancellationToken)
    {
        return dbContext.Categories
            .AsNoTracking()
            .Where(x => x.Id == parentCategoryId)
            .Select(x => new CategoryParentInfo(
                x.ProductType,
                x.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryInsertResult> TryCreateAsync(
        Category category,
        CancellationToken cancellationToken)
    {
        dbContext.Categories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);

            return CategoryInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, SlugIndexName))
        {
            dbContext.Detach(category);

            return CategoryInsertResult.DuplicateSlug;
        }
    }

    public Task<Category?> GetForUpdateAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return dbContext.Categories
            .FirstOrDefaultAsync(
                x => x.Id == categoryId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryReadModel>> GetAllForReadAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.ProductType)
            .ThenBy(x => x.ParentCategoryId)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CategoryReadModel(
                x.Id,
                x.ProductType,
                x.Name,
                x.Slug,
                x.ParentCategoryId,
                x.Status,
                x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}