using HealthcareStore.Application.Payments.Models;

namespace HealthcareStore.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    string PublicKeyId { get; }

    Task<GatewayOrderResult> CreateOrderAsync(
        CreateGatewayOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<GatewayOrderResult?> FindOrderByReceiptAsync(
        string receipt,
        CancellationToken cancellationToken = default);

    Task<GatewayPaymentResult> GetPaymentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken = default);

    bool VerifyCheckoutSignature(
        string trustedProviderOrderId,
        string providerPaymentId,
        string signature);

    bool VerifyWebhookSignature(
        string rawBody,
        string signature);
}