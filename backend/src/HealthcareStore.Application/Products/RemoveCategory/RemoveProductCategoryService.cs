using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Products.RemoveCategory;

public sealed class RemoveProductCategoryService(
    IProductCategoryRepository repository)
{
    public async Task RemoveAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var productCategory =
            await repository.GetAsync(
                productId,
                categoryId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified product-category assignment was not found.");

        repository.Remove(productCategory);

        await repository.SaveChangesAsync(
            cancellationToken);
    }
}