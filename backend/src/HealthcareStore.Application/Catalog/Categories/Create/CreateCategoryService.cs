using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Categories.Create;

public sealed class CreateCategoryService(
    ICategoryRepository categoryRepository,
    ISlugGenerator slugGenerator,
    ICacheService cache)
{
    private const int MaxSlugAttempts = 100;

    public async Task<CreateCategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId.HasValue)
        {
            var parent =
                await categoryRepository.GetParentAsync(
                    request.ParentCategoryId.Value,
                    cancellationToken);

            if (parent is null)
            {
                throw new NotFoundException(
                    "The specified parent category was not found.");
            }

            if (parent.Status != CatalogEntityStatus.Active)
            {
                throw new ConflictException(
                    "The specified parent category is inactive.");
            }

            if (parent.ProductType != request.ProductType)
            {
                throw new ConflictException(
                    "A category cannot have a parent from another product type.");
            }
        }

        var baseSlug = slugGenerator.Generate(request.Name);

        for (
            var attempt = 1;
            attempt <= MaxSlugAttempts;
            attempt++)
        {
            var slug = attempt == 1
                ? baseSlug
                : slugGenerator.AppendSuffix(
                    baseSlug,
                    attempt);

            var category = Category.Create(
                request.ProductType,
                request.Name,
                slug,
                request.ParentCategoryId,
                request.SortOrder);

            var result =
                await categoryRepository.TryCreateAsync(
                    category,
                    cancellationToken);

            if (result == CategoryInsertResult.Created)
            {
                await cache.RemoveByTagAsync(
                    CacheTags.Categories,
                    cancellationToken);

                return new CreateCategoryResponse(
                    category.Id,
                    category.ProductType,
                    category.Name,
                    category.Slug,
                    category.ParentCategoryId,
                    category.Status,
                    category.SortOrder);
            }
        }

        throw new ConflictException(
            "Unable to generate a unique category slug.");
    }
}