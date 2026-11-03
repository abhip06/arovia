namespace HealthcareStore.Application.Helpers;

public sealed record WarehouseReadModel(
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Status);