using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Products.RemoveIngredient;

public sealed class RemoveProductIngredientService(
    IProductRepository productRepository)
{
    public async Task RemoveAsync(
        Guid productId,
        Guid productIngredientId,
        CancellationToken cancellationToken)
    {
        var product =
            await productRepository.GetWithIngredientsAsync(
                productId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified product was not found.");

        var ingredient = product.Ingredients
            .FirstOrDefault(x => x.Id == productIngredientId);

        if (ingredient is null)
        {
            throw new NotFoundException(
                "The specified product ingredient was not found.");
        }

        product.RemoveIngredient(productIngredientId);

        productRepository.RemoveIngredient(ingredient);

        await productRepository.SaveChangesAsync(
            cancellationToken);
    }
}