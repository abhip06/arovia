using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Inventory;

public sealed class Warehouse : Entity
{
    private Warehouse() { }

    private Warehouse(
        string code,
        string name,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();

        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2)
            ? null
            : addressLine2.Trim();

        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();

        Status = WarehouseStatus.Active;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    public WarehouseStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Warehouse Create(
        string code,
        string name,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        return new Warehouse(
            code,
            name,
            addressLine1,
            addressLine2,
            city,
            state,
            postalCode,
            country);
    }

    public void Activate()
    {
        if (Status == WarehouseStatus.Active)
        {
            throw new InvalidOperationException(
                "The warehouse is already active.");
        }

        Status = WarehouseStatus.Active;
        Touch();
    }

    public void Deactivate()
    {
        if (Status != WarehouseStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active warehouse can be deactivated.");
        }

        Status = WarehouseStatus.Inactive;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}