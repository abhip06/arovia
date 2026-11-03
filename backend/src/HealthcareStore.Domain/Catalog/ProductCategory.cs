using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Catalog;

public sealed class ProductCategory : Entity
{
    private ProductCategory() { }

    private ProductCategory(Guid productId, Guid categoryId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));

        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "Category ID cannot be empty.",
                nameof(categoryId));

        ProductId = productId;
        CategoryId = categoryId;
    }

    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }

    public static ProductCategory Create(
        Guid productId,
        Guid categoryId)
    {
        return new ProductCategory(productId, categoryId);
    }
}