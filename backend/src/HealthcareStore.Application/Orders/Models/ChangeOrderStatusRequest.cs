using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Application.Orders.Models;

public sealed record ChangeOrderStatusRequest(OrderStatus Status);