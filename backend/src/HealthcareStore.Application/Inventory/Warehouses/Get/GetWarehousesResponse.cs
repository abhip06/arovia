using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Admin.Warehouses.Get;

public sealed record GetWarehousesResponse(
    PagedResult<WarehouseItem> Result);

public sealed record WarehouseItem(
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Status);