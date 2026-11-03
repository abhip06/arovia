using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Inventory;

public sealed class InventoryBatch : Entity
{
    private InventoryBatch() { }

    private InventoryBatch(
        Guid productVariantId,
        string batchNumber,
        DateOnly? expiryDate,
        DateOnly? sellableUntil,
        decimal mrp)
    {
        if (productVariantId == Guid.Empty)
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));

        ArgumentException.ThrowIfNullOrWhiteSpace(batchNumber);

        if (mrp < 0)
            throw new ArgumentOutOfRangeException(
                nameof(mrp),
                "MRP cannot be negative.");

        if (sellableUntil.HasValue &&
            expiryDate.HasValue &&
            sellableUntil.Value > expiryDate.Value)
        {
            throw new ArgumentException(
                "Sellable-until date cannot be later than expiry date.",
                nameof(sellableUntil));
        }

        ProductVariantId = productVariantId;
        BatchNumber = batchNumber.Trim().ToUpperInvariant();

        ExpiryDate = expiryDate;
        SellableUntil = sellableUntil;
        Mrp = mrp;

        Status = InventoryBatchStatus.Active;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid ProductVariantId { get; private set; }

    public string BatchNumber { get; private set; } = null!;

    public DateOnly? ExpiryDate { get; private set; }
    public DateOnly? SellableUntil { get; private set; }

    public decimal Mrp { get; private set; }

    public InventoryBatchStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static InventoryBatch Create(
        Guid productVariantId,
        string batchNumber,
        DateOnly? expiryDate,
        DateOnly? sellableUntil,
        decimal mrp)
    {
        return new InventoryBatch(
            productVariantId,
            batchNumber,
            expiryDate,
            sellableUntil,
            mrp);
    }

    public void Activate()
    {
        if (Status == InventoryBatchStatus.Recalled)
        {
            throw new InvalidOperationException(
                "Recalled inventory batch cannot be activated.");
        }

        Status = InventoryBatchStatus.Active;
        Touch();
    }
    
    public void Quarantine()
    {
        if (Status != InventoryBatchStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active inventory batch can be quarantined.");
        }

        Status = InventoryBatchStatus.Quarantined;
        Touch();
    }

    public void ReleaseFromQuarantine()
    {
        if (Status != InventoryBatchStatus.Quarantined)
        {
            throw new InvalidOperationException(
                "Only a quarantined inventory batch can be released.");
        }

        Status = InventoryBatchStatus.Active;
        Touch();
    }

    public void Recall()
    {
        if (Status != InventoryBatchStatus.Active &&
            Status != InventoryBatchStatus.Quarantined)
        {
            throw new InvalidOperationException(
                "Only an active or quarantined inventory batch can be recalled.");
        }

        Status = InventoryBatchStatus.Recalled;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}