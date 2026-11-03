using HealthcareStore.Domain.Common;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Domain.Products;

public sealed class ProductIngredient : Entity
{
    private ProductIngredient() { }

    private ProductIngredient(
        Guid productId,
        Guid activeIngredientId,
        decimal strengthValue,
        string strengthUnit,
        IngredientRole role)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));

        if (activeIngredientId == Guid.Empty)
            throw new ArgumentException(
                "Active ingredient ID cannot be empty.",
                nameof(activeIngredientId));

        if (strengthValue <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(strengthValue),
                "Strength must be greater than zero.");

        ArgumentException.ThrowIfNullOrWhiteSpace(strengthUnit);

        ProductId = productId;
        ActiveIngredientId = activeIngredientId;
        StrengthValue = strengthValue;
        StrengthUnit = strengthUnit.Trim().ToLowerInvariant();
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid ProductId { get; private set; }
    public Guid ActiveIngredientId { get; private set; }

    public decimal StrengthValue { get; private set; }
    public string StrengthUnit { get; private set; } = null!;

    public IngredientRole Role { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProductIngredient Create(
        Guid productId,
        Guid activeIngredientId,
        decimal strengthValue,
        string strengthUnit,
        IngredientRole role = IngredientRole.Active)
    {
        return new ProductIngredient(
            productId,
            activeIngredientId,
            strengthValue,
            strengthUnit,
            role);
    }
}