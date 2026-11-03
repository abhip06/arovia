namespace HealthcareStore.Api.Admin.Warehouses.Get;

public sealed record WarehouseListResponse(
    IReadOnlyList<WarehouseResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record WarehouseResponse(
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Status);