using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Api.Admin.Warehouses.Create;

public sealed record CreateWarehouseApiResponse(
    Guid Id,
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    WarehouseStatus Status);