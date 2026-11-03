using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.Categories.Get;

public sealed class GetCategoriesService(
    ICategoryRepository categoryRepository,
    ICacheService cache)
{
    public async Task<IReadOnlyList<CategoryTreeItem>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            CacheKeys.Categories,
            async token =>
            {
                var categories =
                    await categoryRepository.GetAllForReadAsync(token);

                return BuildTree(categories);
            },
            expiration: TimeSpan.FromMinutes(15),
            localCacheExpiration: TimeSpan.FromSeconds(30),
            tags:
            [
                CacheTags.Categories
            ],
            cancellationToken: cancellationToken);
    }

    private static IReadOnlyList<CategoryTreeItem> BuildTree(
        IReadOnlyList<CategoryReadModel> categories)
    {
        var activeCategories = categories
            .Where(x => x.Status == CatalogEntityStatus.Active)
            .ToDictionary(x => x.Id);

        var childrenByParent = activeCategories.Values
            .Where(x =>
                x.ParentCategoryId.HasValue &&
                activeCategories.ContainsKey(x.ParentCategoryId.Value))
            .GroupBy(x => x.ParentCategoryId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .ToList());

        var roots = activeCategories.Values
            .Where(x => !x.ParentCategoryId.HasValue)
            .OrderBy(x => x.ProductType)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToList();

        return roots
            .Select(root =>
                BuildNode(
                    root,
                    childrenByParent))
            .ToList();
    }

    private static CategoryTreeItem BuildNode(
        CategoryReadModel category,
        IReadOnlyDictionary<Guid, List<CategoryReadModel>>
            childrenByParent)
    {
        var children =
            childrenByParent.TryGetValue(
                category.Id,
                out var childCategories)
                ? childCategories
                    .Select(child =>
                        BuildNode(
                            child,
                            childrenByParent))
                    .ToList()
                : [];

        return new CategoryTreeItem(
            category.ProductType.ToString(),
            category.Slug,
            category.Name,
            children);
    }
}