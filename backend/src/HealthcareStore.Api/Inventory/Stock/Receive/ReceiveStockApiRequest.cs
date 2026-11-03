using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Inventory.Stock.Receive;

public sealed record ReceiveStockApiRequest(
    [property: Range(1, int.MaxValue)]
    int Quantity,

    [property: Range(typeof(decimal), "0", "999999999999")]
    decimal CostPrice);