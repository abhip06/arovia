using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Variants.Status;

public sealed class ProductVariantStatusService(
    IProductVariantRepository variantRepository)
{
    public Task ActivateAsync(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            variantId,
            variant => variant.Activate(),
            cancellationToken);
    }

    public Task DeactivateAsync(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            variantId,
            variant => variant.Deactivate(),
            cancellationToken);
    }

    public Task DiscontinueAsync(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            variantId,
            variant => variant.Discontinue(),
            cancellationToken);
    }

    private async Task ChangeStatusAsync(
        Guid variantId,
        Action<ProductVariant> changeStatus,
        CancellationToken cancellationToken)
    {
        var variant =
            await variantRepository.GetForUpdateAsync(
                variantId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified product variant was not found.");

        try
        {
            changeStatus(variant);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await variantRepository.SaveChangesAsync(
            cancellationToken);
    }
}