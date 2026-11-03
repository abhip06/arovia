using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<CategoryParentInfo?> GetParentAsync(
        Guid parentCategoryId,
        CancellationToken cancellationToken);

    Task<CategoryInsertResult> TryCreateAsync(
        Category category,
        CancellationToken cancellationToken);

    Task<Category?> GetForUpdateAsync(
        Guid categoryId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryReadModel>> GetAllForReadAsync(
        CancellationToken cancellationToken);
}

public enum CategoryInsertResult
{
    Created = 1,
    DuplicateSlug = 2
}