namespace HealthcareStore.Api.Users.Addresses;

public sealed record CreateAddressApiRequest(
    string Label,
    string RecipientName,
    string? RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record UpdateAddressApiRequest(
    string Label,
    string RecipientName,
    string? RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record AddressApiResponse(
    Guid Id,
    string Label,
    string RecipientName,
    string? RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsPrimary,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);