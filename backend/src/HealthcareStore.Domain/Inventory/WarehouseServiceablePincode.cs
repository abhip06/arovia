namespace HealthcareStore.Domain.Inventory;

public sealed class WarehouseServiceablePincode
{
    private WarehouseServiceablePincode()
    {
    }

    private WarehouseServiceablePincode(
        Guid warehouseId,
        string pincode)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));
        }

        WarehouseId = warehouseId;
        Pincode = NormalizePincode(pincode);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid WarehouseId { get; private set; }

    public string Pincode { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public static WarehouseServiceablePincode Create(
        Guid warehouseId,
        string pincode)
    {
        return new WarehouseServiceablePincode(
            warehouseId,
            pincode);
    }

    private static string NormalizePincode(
        string pincode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pincode);

        var normalized =
            pincode.Trim();

        if (normalized.Length != 6 ||
            normalized.Any(
                static x => x < '0' || x > '9'))
        {
            throw new ArgumentException(
                "Pincode must contain exactly 6 digits.",
                nameof(pincode));
        }

        return normalized;
    }
}