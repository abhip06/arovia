using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Application.Orders.Models;

public sealed record OrderSummaryModel(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus Status,
    string Currency,
    decimal GrandTotal,
    DateTimeOffset CreatedAtUtc,
    int ItemCount,
    int TotalQuantity);