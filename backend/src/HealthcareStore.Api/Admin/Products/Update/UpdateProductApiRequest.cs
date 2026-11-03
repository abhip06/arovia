using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Admin.Products.Update;

public sealed record UpdateProductApiRequest(
    [property: Required]
    [property: StringLength(200, MinimumLength = 2)]
    string Name,

    Guid ManufacturerId,

    Guid? BrandId,

    bool RequiresPrescription,

    [property: Required]
    [property: StringLength(8, MinimumLength = 1)]
    string HsnCode,

    [property: Range(0, 100)]
    decimal TaxRate,

    bool Returnable,

    [property: Range(0, int.MaxValue)]
    int ReturnWindowDays);