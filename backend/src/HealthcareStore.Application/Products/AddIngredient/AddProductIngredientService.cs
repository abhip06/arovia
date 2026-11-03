using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.AddIngredient;

public sealed class AddProductIngredientService(
    IProductRepository productRepository,
    IActiveIngredientRepository activeIngredientRepository)
{
    public async Task<AddProductIngredientResponse> AddAsync(
        AddProductIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var product =
            await productRepository.GetWithIngredientsAsync(
                request.ProductId,
                cancellationToken) ?? throw new NotFoundException(
                    "The specified product was not found.");

        if (product.Status == ProductStatus.Discontinued)
        {
            throw new ConflictException(
                "An ingredient cannot be added to a discontinued product.");
        }

        var activeIngredient =
            await activeIngredientRepository.GetActiveAsync(
                request.ActiveIngredientId,
                cancellationToken) ?? throw new NotFoundException(
                    "The specified active ingredient was not found or is inactive.");

        var ingredient = ProductIngredient.Create(
            product.Id,
            activeIngredient.Id,
            request.StrengthValue,
            request.StrengthUnit,
            request.Role);

        try
        {
            product.AddIngredient(ingredient);
        }
        catch (InvalidOperationException)
        {
            throw new ConflictException(
                "The product already contains this ingredient.");
        }

        productRepository.AddIngredient(ingredient);

        await productRepository.SaveChangesAsync(
            cancellationToken);

        return new AddProductIngredientResponse(
            ingredient.Id,
            ingredient.ProductId,
            ingredient.ActiveIngredientId,
            ingredient.StrengthValue,
            ingredient.StrengthUnit,
            ingredient.Role);
    }
}