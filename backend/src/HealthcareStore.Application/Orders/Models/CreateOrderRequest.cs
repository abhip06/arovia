namespace HealthcareStore.Application.Orders.Models;

public sealed record CreateOrderRequest(
    Guid ShippingAddressId);