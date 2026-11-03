using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Application.Orders.Receipts;
using Microsoft.Extensions.Logging;

namespace HealthcareStore.Application.Payments;

/// <summary>
/// Sends the customer a payment confirmation and the generated order receipt.
/// Email delivery is best-effort after payment has been committed; a mail
/// transport failure must never make a captured payment appear unsuccessful.
/// </summary>
public sealed class SendOrderPaymentConfirmationEmailService(
    IUserRepository userRepository,
    OrderReceiptService receiptService,
    IEmailSender emailSender,
    ILogger<SendOrderPaymentConfirmationEmailService> logger)
{
    public async Task SendAsync(
        Guid customerId,
        OrderDetailsModel? order,
        CancellationToken cancellationToken = default)
    {
        if (order is null)
        {
            logger.LogError(
                "Payment was captured for customer {CustomerId}, but the confirmed order details were unavailable for its email.",
                customerId);
            return;
        }

        if (order.UserId != customerId)
        {
            logger.LogError(
                "Payment confirmation email was refused because order {OrderId} does not belong to customer {CustomerId}.",
                order.Id,
                customerId);
            return;
        }

        try
        {
            var customer = await userRepository.FindByIdAsync(
                customerId,
                cancellationToken);

            if (customer is null)
            {
                logger.LogError(
                    "Payment confirmation email was not sent because customer {CustomerId} was not found for order {OrderId}.",
                    customerId,
                    order.Id);
                return;
            }

            using var receiptPdf = new MemoryStream();
            var receipt = await receiptService.GenerateForCustomerAsync(
                order.Id,
                customerId,
                receiptPdf,
                cancellationToken);

            if (receipt.Status != OrderReceiptGenerationStatus.Generated ||
                string.IsNullOrWhiteSpace(receipt.FileName))
            {
                logger.LogError(
                    "Payment confirmation email was not sent because the receipt could not be generated for order {OrderId}. Receipt status: {ReceiptStatus}.",
                    order.Id,
                    receipt.Status);
                return;
            }

            var confirmation = new OrderPaymentConfirmationEmail(
                customer.FirstName,
                order.OrderNumber,
                order.Currency,
                order.GrandTotal,
                order.UpdatedAtUtc);

            await emailSender.SendOrderPaymentConfirmationAsync(
                customer.Email,
                confirmation,
                receiptPdf,
                receipt.FileName,
                cancellationToken);

            logger.LogInformation(
                "Payment confirmation email with receipt sent for order {OrderId} to customer {CustomerId}.",
                order.Id,
                customerId);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Payment was captured for order {OrderId}, but its confirmation email could not be sent to customer {CustomerId}.",
                order.Id,
                customerId);
        }
    }
}
