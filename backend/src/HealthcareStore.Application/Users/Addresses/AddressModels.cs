namespace HealthcareStore.Application.Users.Addresses;

public sealed record CreateAddressRequest(
    string Label,
    string RecipientName,
    string? RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record UpdateAddressRequest(
    string Label,
    string RecipientName,
    string? RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record AddressReadModel(
    Guid Id,
    Guid UserId,
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