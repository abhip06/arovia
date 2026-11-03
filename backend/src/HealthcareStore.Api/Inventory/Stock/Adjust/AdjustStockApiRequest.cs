using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Inventory.Stock.Adjust;

public sealed record AdjustStockApiRequest(
    [property: Range(
        int.MinValue + 1,
        int.MaxValue)]
    int QuantityDelta,

    [property: Required]
    [property: StringLength(500, MinimumLength = 3)]
    string Reason);