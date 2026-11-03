using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Products;

public sealed class ProductPrice : Entity
{
    private ProductPrice() { }

    private ProductPrice(
        Guid productVariantId,
        decimal sellingPrice,
        DateTimeOffset effectiveFrom)
    {
        if (productVariantId == Guid.Empty)
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));

        if (sellingPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(sellingPrice),
                "Selling price cannot be negative.");

        if (effectiveFrom == default)
            throw new ArgumentException(
                "Effective from must be specified.",
                nameof(effectiveFrom));

        ProductVariantId = productVariantId;
        SellingPrice = sellingPrice;
        EffectiveFrom = effectiveFrom;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid ProductVariantId { get; private set; }

    public decimal SellingPrice { get; private set; }

    public DateTimeOffset EffectiveFrom { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProductPrice Create(
        Guid productVariantId,
        decimal sellingPrice,
        DateTimeOffset effectiveFrom)
    {
        return new ProductPrice(
            productVariantId,
            sellingPrice,
            effectiveFrom);
    }
}