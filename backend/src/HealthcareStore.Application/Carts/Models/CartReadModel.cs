namespace HealthcareStore.Application.Carts.Models;

public sealed record CartReadModel(
    Guid? Id,
    IReadOnlyList<CartItemReadModel> Items,
    int ItemCount,
    decimal Subtotal,
    bool HasUnavailableItems);