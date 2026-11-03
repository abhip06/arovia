using HealthcareStore.Application.Orders.Models;

namespace HealthcareStore.Application.Orders;

public enum CreateOrderStatus
{
    Created = 1,
    Replayed = 2,
    CartEmpty = 3,
    ShippingAddressNotFound = 4,
    RecipientPhoneRequired = 5,
    InvalidShippingAddress = 6,
    ItemsUnavailable = 7,
    InsufficientStock = 8,
    IdempotencyConflict = 9
}

public sealed record CreateOrderResult(
    CreateOrderStatus Status,
    OrderDetailsModel? Order = null);