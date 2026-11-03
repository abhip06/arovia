using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ActiveIngredientRepository(
    ApplicationDbContext dbContext) : IActiveIngredientRepository
{
    private const string SlugIndexName =
        "IX_active_ingredients_Slug";

    public Task<ActiveIngredient?> GetActiveAsync(
        Guid activeIngredientId,
        CancellationToken cancellationToken)
    {
        return dbContext.ActiveIngredients
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == activeIngredientId &&
                    x.Status == CatalogEntityStatus.Active,
                cancellationToken);
    }

    public async Task<ActiveIngredientInsertResult> TryCreateAsync(
        ActiveIngredient activeIngredient,
        CancellationToken cancellationToken)
    {
        dbContext.ActiveIngredients.Add(activeIngredient);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return ActiveIngredientInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, SlugIndexName))
        {
            dbContext.Detach(activeIngredient);

            return ActiveIngredientInsertResult.DuplicateSlug;
        }
    }

    public Task<ActiveIngredient?> GetForUpdateAsync(
        Guid activeIngredientId,
        CancellationToken cancellationToken)
    {
        return dbContext.ActiveIngredients
            .FirstOrDefaultAsync(
                x => x.Id == activeIngredientId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }
}