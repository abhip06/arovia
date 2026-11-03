using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Users;

public sealed class Address : Entity
{
    private Address() { }

    private Address(
        Guid userId,
        string label,
        string recipientName,
        string? recipientPhone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country,
        bool isPrimary)
    {
        UserId = userId;
        Label = label;
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        IsPrimary = isPrimary;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; private set; }

    public string Label { get; private set; } = null!;

    public string RecipientName { get; private set; } = null!;

    public string? RecipientPhone { get; private set; }

    public string AddressLine1 { get; private set; } = null!;

    public string? AddressLine2 { get; private set; }

    public string City { get; private set; } = null!;

    public string State { get; private set; } = null!;

    public string PostalCode { get; private set; } = null!;

    public string Country { get; private set; } = null!;

    public bool IsPrimary { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Address Create(
        Guid userId,
        string label,
        string recipientName,
        string? recipientPhone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country,
        bool isPrimary)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(label, nameof(label));
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName, nameof(recipientName));
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine1, nameof(addressLine1));
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));
        ArgumentException.ThrowIfNullOrWhiteSpace(state, nameof(state));
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode, nameof(postalCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(country, nameof(country));

        return new Address(
            userId,
            label.Trim(),
            recipientName.Trim(),
            string.IsNullOrWhiteSpace(recipientPhone)
                ? null
                : recipientPhone.Trim(),
            addressLine1.Trim(),
            string.IsNullOrWhiteSpace(addressLine2)
                ? null
                : addressLine2.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode.Trim(),
            country.Trim(),
            isPrimary);
    }

    public void Update(
        string label,
        string recipientName,
        string? recipientPhone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label, nameof(label));
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName, nameof(recipientName));
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine1, nameof(addressLine1));
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));
        ArgumentException.ThrowIfNullOrWhiteSpace(state, nameof(state));
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode, nameof(postalCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(country, nameof(country));

        Label = label.Trim();
        RecipientName = recipientName.Trim();
        RecipientPhone = string.IsNullOrWhiteSpace(recipientPhone)
            ? null
            : recipientPhone.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2)
            ? null
            : addressLine2.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPrimary()
    {
        IsPrimary = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}