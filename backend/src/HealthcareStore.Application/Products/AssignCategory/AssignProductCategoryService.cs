using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Products.AssignCategory;

public sealed class AssignProductCategoryService(
    IProductCategoryRepository repository)
{
    public async Task AssignAsync(
        AssignProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var productType =
            await repository.GetProductTypeAsync(
                request.ProductId,
                cancellationToken);

        if (productType is null)
        {
            throw new NotFoundException(
                "The specified product was not found.");
        }

        var category =
            await repository.GetCategoryAsync(
                request.CategoryId,
                cancellationToken) ?? throw new NotFoundException(
                    "The specified category was not found.");

        if (category.Status != CatalogEntityStatus.Active)
        {
            throw new ConflictException(
                "The specified category is inactive.");
        }

        if (category.ProductType != productType.Value)
        {
            throw new ConflictException(
                "A product can only be assigned to a category with the same product type.");
        }

        if (await repository.AssignmentExistsAsync(
                request.ProductId,
                request.CategoryId,
                cancellationToken))
        {
            throw new ConflictException(
                "The product is already assigned to this category.");
        }

        var productCategory = ProductCategory.Create(
            request.ProductId,
            request.CategoryId);

        var result = await repository.TryCreateAsync(
            productCategory,
            cancellationToken);

        if (result == ProductCategoryInsertResult.Duplicate)
        {
            throw new ConflictException(
                "The product is already assigned to this category.");
        }
    }
}