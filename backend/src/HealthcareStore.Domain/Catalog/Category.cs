using HealthcareStore.Domain.Products;
using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Catalog;

public sealed class Category : Entity
{
    private Category() { }

    private Category(
        ProductType productType,
        string name,
        string slug,
        Guid? parentCategoryId,
        int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");

        if (parentCategoryId.HasValue &&
            parentCategoryId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Parent category ID cannot be empty.",
                nameof(parentCategoryId));
        }

        ProductType = productType;
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ParentCategoryId = parentCategoryId;
        SortOrder = sortOrder;
        Status = CatalogEntityStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public ProductType ProductType { get; private set; }

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    public Guid? ParentCategoryId { get; private set; }

    public CatalogEntityStatus Status { get; private set; }
    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Category Create(
        ProductType productType,
        string name,
        string slug,
        Guid? parentCategoryId = null,
        int sortOrder = 0)
    {
        return new Category(
            productType,
            name,
            slug,
            parentCategoryId,
            sortOrder);
    }

    public void Activate()
    {
        if (Status == CatalogEntityStatus.Active)
            throw new InvalidOperationException(
                "The category is already active.");

        Status = CatalogEntityStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == CatalogEntityStatus.Inactive)
            throw new InvalidOperationException(
                "The category is already inactive.");

        Status = CatalogEntityStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}