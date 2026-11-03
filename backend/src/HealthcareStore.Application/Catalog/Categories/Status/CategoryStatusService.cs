using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Categories.Status;

public sealed class CategoryStatusService(
    ICategoryRepository categoryRepository,
    ICacheService cache)
{
    public Task ActivateAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            categoryId,
            category => category.Activate(),
            cancellationToken);
    }

    public Task DeactivateAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            categoryId,
            category => category.Deactivate(),
            cancellationToken);
    }

    private async Task ChangeStatusAsync(
        Guid categoryId,
        Action<Category> changeStatus,
        CancellationToken cancellationToken)
    {
        var category =
            await categoryRepository.GetForUpdateAsync(
                categoryId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified category was not found.");

        try
        {
            changeStatus(category);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await categoryRepository.SaveChangesAsync(
            cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Categories,
            cancellationToken);
    }
}