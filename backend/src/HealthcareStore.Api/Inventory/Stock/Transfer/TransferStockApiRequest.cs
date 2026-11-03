using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Inventory.Stock.Transfer;

public sealed record TransferStockApiRequest(
    [property: Range(1, int.MaxValue)]
    int Quantity,

    [property: StringLength(500)]
    string? Note);