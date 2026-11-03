namespace HealthcareStore.Application.Orders.Receipts;

/// <summary>
/// A presentation snapshot used to render a customer order/payment receipt.
/// It deliberately contains only values persisted with the order/payment;
/// it does not look up current product prices or customer addresses.
/// </summary>
public sealed record OrderReceiptModel(
    string OrderNumber,
    string Currency,
    DateTimeOffset OrderPlacedAtUtc,
    DateTimeOffset PaymentConfirmedAtUtc,
    string PaymentProvider,
    OrderReceiptAddressModel ShippingAddress,
    IReadOnlyList<OrderReceiptLineModel> Items,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal ShippingFee,
    decimal GrandTotal);

public sealed record OrderReceiptAddressModel(
    string RecipientName,
    string RecipientPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record OrderReceiptLineModel(
    string ProductName,
    string Sku,
    decimal PackageQuantity,
    string PackageUnit,
    int Quantity,
    decimal UnitMrp,
    decimal UnitSellingPrice,
    decimal LineDiscountAmount,
    decimal LineTaxAmount,
    decimal LineSubtotal,
    decimal LineTotal);

public enum OrderReceiptGenerationStatus
{
    Generated = 1,
    OrderNotFound = 2,
    PaymentNotCaptured = 3,
    PaymentReconciliationRequired = 4
}

public sealed record OrderReceiptGenerationResult(
    OrderReceiptGenerationStatus Status,
    string? FileName = null);
