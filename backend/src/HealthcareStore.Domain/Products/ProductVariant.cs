using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Products;

public sealed class ProductVariant : Entity
{
    private ProductVariant() { }

    private ProductVariant(
        Guid productId,
        string sku,
        string? barcode,
        decimal packageQuantity,
        string packageUnit,
        decimal? netQuantity,
        string? netQuantityUnit)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));

        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageUnit);

        if (packageQuantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(packageQuantity),
                "Package quantity must be greater than zero.");

        if (netQuantity is <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(netQuantity),
                "Net quantity must be greater than zero.");

        ProductId = productId;
        Sku = sku.Trim().ToUpperInvariant();

        Barcode = string.IsNullOrWhiteSpace(barcode)
            ? null
            : barcode.Trim();

        PackageQuantity = packageQuantity;
        PackageUnit = packageUnit.Trim();

        NetQuantity = netQuantity;
        NetQuantityUnit = string.IsNullOrWhiteSpace(netQuantityUnit)
            ? null
            : netQuantityUnit.Trim();

        Status = ProductVariantStatus.Active;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = null!;
    public string? Barcode { get; private set; }

    public decimal PackageQuantity { get; private set; }
    public string PackageUnit { get; private set; } = null!;

    public decimal? NetQuantity { get; private set; }
    public string? NetQuantityUnit { get; private set; }

    public ProductVariantStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProductVariant Create(
        Guid productId,
        string sku,
        string? barcode,
        decimal packageQuantity,
        string packageUnit,
        decimal? netQuantity = null,
        string? netQuantityUnit = null)
    {
        return new ProductVariant(
            productId,
            sku,
            barcode,
            packageQuantity,
            packageUnit,
            netQuantity,
            netQuantityUnit);
    }

    public void Activate()
    {
        if (Status == ProductVariantStatus.Active)
        {
            throw new InvalidOperationException(
                "The product variant is already active.");
        }

        if (Status == ProductVariantStatus.Discontinued)
        {
            throw new InvalidOperationException(
                "A discontinued product variant cannot be activated.");
        }

        Status = ProductVariantStatus.Active;
        Touch();
    }

    public void Deactivate()
    {
        if (Status != ProductVariantStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active product variant can be deactivated.");
        }

        Status = ProductVariantStatus.Inactive;
        Touch();
    }

    public void Discontinue()
    {
        if (Status != ProductVariantStatus.Active &&
            Status != ProductVariantStatus.Inactive)
        {
            throw new InvalidOperationException(
                "Only an active or inactive product variant can be discontinued.");
        }

        Status = ProductVariantStatus.Discontinued;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}