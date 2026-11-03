using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Products.Status;

public sealed class ProductStatusService(
    IProductRepository productRepository)
{
    public Task ActivateAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            productId,
            product => product.Activate(),
            cancellationToken);
    }

    public Task DeactivateAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            productId,
            product => product.Deactivate(),
            cancellationToken);
    }

    public Task DiscontinueAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            productId,
            product => product.Discontinue(),
            cancellationToken);
    }

    private async Task ChangeStatusAsync(
        Guid productId,
        Action<Domain.Products.Product> changeStatus,
        CancellationToken cancellationToken)
    {
        var product =
            await productRepository.GetForUpdateAsync(
                productId,
                cancellationToken) ?? throw new NotFoundException(
                    "The specified product was not found.");

        try
        {
            changeStatus(product);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await productRepository.SaveChangesAsync(
            cancellationToken);
    }
}