namespace HealthcareStore.Domain.Orders;

public sealed class OrderAddress
{
    private OrderAddress() { }

    public string RecipientName { get; } = null!;
    public string RecipientPhone { get; } = null!;
    public string AddressLine1 { get; } = null!;
    public string? AddressLine2 { get; }
    public string City { get; } = null!;
    public string State { get; } = null!;
    public string PostalCode { get; } = null!;
    public string Country { get; } = null!;

    private OrderAddress(
        string recipientName,
        string recipientPhone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;      
        Country = country;
    }

    public static OrderAddress Create(
        string recipientName,
        string recipientPhone,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        Required(recipientName, nameof(recipientName));
        Required(recipientPhone, nameof(recipientPhone));
        Required(addressLine1, nameof(addressLine1));
        var normalizedAddressLine2 = Optional(addressLine2);
        Required(city, nameof(city));
        Required(state, nameof(state));
        Required(postalCode, nameof(postalCode));
        Required(country, nameof(country));

        if (country.Length < 2)
        {
            throw new ArgumentException(
                "Country must contain atleast 2 characters.",
                country);
        }

        return new OrderAddress(
            recipientName.Trim(),
            recipientPhone.Trim(),
            addressLine1.Trim(),
            normalizedAddressLine2,
            city.Trim(),
            state.Trim(),
            postalCode.Trim(),
            country.Trim());
    }

    private static void Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}