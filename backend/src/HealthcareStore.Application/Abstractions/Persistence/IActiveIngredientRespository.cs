using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IActiveIngredientRepository
{
    Task<ActiveIngredient?> GetActiveAsync(
        Guid activeIngredientId,
        CancellationToken cancellationToken);

    Task<ActiveIngredientInsertResult> TryCreateAsync(
        ActiveIngredient activeIngredient,
        CancellationToken cancellationToken);

    Task<ActiveIngredient?> GetForUpdateAsync(
        Guid activeIngredientId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}

public enum ActiveIngredientInsertResult
{
    Created = 1,
    DuplicateSlug = 2
}