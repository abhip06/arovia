namespace HealthcareStore.Application.Inventory.Warehouses.Create;

public sealed record CreateWarehouseRequest(
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);