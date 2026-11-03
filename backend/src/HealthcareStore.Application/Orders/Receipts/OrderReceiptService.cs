using HealthcareStore.Application.Abstractions.Documents;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Payments;
using Microsoft.Extensions.Logging;

namespace HealthcareStore.Application.Orders.Receipts;

/// <summary>
/// Loads the customer-owned order and its payment, validates that a captured
/// payment matches the persisted order total, then delegates PDF rendering.
/// </summary>
public sealed class OrderReceiptService(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IOrderReceiptPdfGenerator pdfGenerator,
    ILogger<OrderReceiptService> logger)
{
    public async Task<OrderReceiptGenerationResult> GenerateForCustomerAsync(
        Guid orderId,
        Guid customerId,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        }

        ArgumentNullException.ThrowIfNull(destination);

        if (!destination.CanWrite)
        {
            throw new ArgumentException(
                "The destination stream must be writable.",
                nameof(destination));
        }

        var order = await orderRepository.GetCustomerOrderByIdAsync(
            orderId,
            customerId,
            cancellationToken);

        // Deliberately treat an order owned by another customer as not found.
        if (order is null)
        {
            return new OrderReceiptGenerationResult(
                OrderReceiptGenerationStatus.OrderNotFound);
        }

        var payment = await paymentRepository.GetByOrderIdAsync(
            orderId,
            cancellationToken);

        if (payment is null || payment.Status != PaymentStatus.Captured)
        {
            return new OrderReceiptGenerationResult(
                OrderReceiptGenerationStatus.PaymentNotCaptured);
        }

        var paymentConfirmedAtUtc = payment.CapturedAtUtc;

        if (paymentConfirmedAtUtc is null ||
            payment.Amount != order.GrandTotal ||
            !string.Equals(
                payment.Currency,
                order.Currency,
                StringComparison.OrdinalIgnoreCase) ||
            order.Items.Count == 0)
        {
            logger.LogError(
                "Receipt generation was refused because the captured payment does not match its order. " +
                "OrderId: {OrderId}, PaymentId: {PaymentId}, OrderTotal: {OrderTotal}, " +
                "PaymentAmount: {PaymentAmount}, OrderCurrency: {OrderCurrency}, PaymentCurrency: {PaymentCurrency}.",
                orderId,
                payment.Id,
                order.GrandTotal,
                payment.Amount,
                order.Currency,
                payment.Currency);

            return new OrderReceiptGenerationResult(
                OrderReceiptGenerationStatus.PaymentReconciliationRequired);
        }

        var receipt = new OrderReceiptModel(
            OrderNumber: order.OrderNumber,
            Currency: order.Currency,
            OrderPlacedAtUtc: order.CreatedAtUtc,
            PaymentConfirmedAtUtc: paymentConfirmedAtUtc.Value,
            PaymentProvider: payment.Provider.ToString(),
            ShippingAddress: new OrderReceiptAddressModel(
                order.ShippingAddress.RecipientName,
                order.ShippingAddress.RecipientPhone,
                order.ShippingAddress.AddressLine1,
                order.ShippingAddress.AddressLine2,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country),
            Items: order.Items.Select(item => new OrderReceiptLineModel(
                item.ProductName,
                item.Sku,
                item.PackageQuantity,
                item.PackageUnit,
                item.Quantity,
                item.UnitMrp,
                item.UnitSellingPrice,
                item.LineDiscountAmount,
                item.LineTaxAmount,
                item.LineSubtotal,
                item.LineTotal)).ToArray(),
            Subtotal: order.Subtotal,
            DiscountTotal: order.DiscountTotal,
            TaxTotal: order.TaxTotal,
            ShippingFee: order.ShippingFee,
            GrandTotal: order.GrandTotal);

        pdfGenerator.Generate(receipt, destination);

        var safeOrderNumber = new string(order.OrderNumber
            .Where(static character =>
                char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
            .ToArray());

        if (string.IsNullOrWhiteSpace(safeOrderNumber))
        {
            safeOrderNumber = order.Id.ToString("N");
        }

        return new OrderReceiptGenerationResult(
            OrderReceiptGenerationStatus.Generated,
            $"Arovia-Receipt-{safeOrderNumber}.pdf");
    }
}
