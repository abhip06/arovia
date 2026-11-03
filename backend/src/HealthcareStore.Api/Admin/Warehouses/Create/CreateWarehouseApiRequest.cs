using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Admin.Warehouses.Create;

public sealed record CreateWarehouseApiRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 2)]
    string Code,

    [property: Required]
    [property: StringLength(150, MinimumLength = 2)]
    string Name,

    [property: Required]
    [property: StringLength(200, MinimumLength = 2)]
    string AddressLine1,

    [property: StringLength(200)]
    string? AddressLine2,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string City,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string State,

    [property: Required]
    [property: StringLength(20, MinimumLength = 2)]
    string PostalCode,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string Country);