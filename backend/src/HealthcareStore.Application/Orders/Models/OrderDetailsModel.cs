using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Application.Orders.Models;

public sealed record OrderDetailsModel(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus Status,
    string Currency,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal ShippingFee,
    decimal GrandTotal,
    OrderAddressModel ShippingAddress,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<OrderItemDetailsModel> Items);

public sealed record OrderAddressModel(
    string RecipientName,
    string RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record OrderItemDetailsModel(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string Sku,
    string? ImageUrl,
    decimal PackageQuantity,
    string PackageUnit,
    int Quantity,
    decimal UnitMrp,
    decimal UnitSellingPrice,
    decimal LineTaxAmount,
    decimal LineDiscountAmount,
    decimal LineSubtotal,
    decimal LineTotal,
    bool RequiresPrescription);