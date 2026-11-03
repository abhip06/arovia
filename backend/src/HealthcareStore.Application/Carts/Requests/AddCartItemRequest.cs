namespace HealthcareStore.Application.Carts.Requests;

public sealed record AddCartItemRequest(
    Guid ProductVariantId,
    int Quantity);