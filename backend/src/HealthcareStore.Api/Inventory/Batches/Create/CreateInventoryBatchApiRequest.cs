using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Inventory.Batches.Create;

public sealed record CreateInventoryBatchApiRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 1)]
    string BatchNumber,

    DateOnly? ExpiryDate,

    DateOnly? SellableUntil,

    [property: Required]
    [property: Range(0, double.MaxValue)]
    decimal Mrp);