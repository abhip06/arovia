using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.ActiveIngredients.Status;

public sealed class ActiveIngredientStatusService(
    IActiveIngredientRepository repository)
{
    public Task ActivateAsync(
        Guid activeIngredientId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            activeIngredientId,
            ingredient => ingredient.Activate(),
            cancellationToken);
    }

    public Task DeactivateAsync(
        Guid activeIngredientId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            activeIngredientId,
            ingredient => ingredient.Deactivate(),
            cancellationToken);
    }

    private async Task ChangeStatusAsync(
        Guid activeIngredientId,
        Action<ActiveIngredient> changeStatus,
        CancellationToken cancellationToken)
    {
        var ingredient =
            await repository.GetForUpdateAsync(
                activeIngredientId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified active ingredient was not found.");

        try
        {
            changeStatus(ingredient);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await repository.SaveChangesAsync(
            cancellationToken);
    }
}