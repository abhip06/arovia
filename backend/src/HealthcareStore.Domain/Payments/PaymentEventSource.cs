namespace HealthcareStore.Domain.Payments;

public enum PaymentEventSource
{
    Application = 1,
    RazorpayCheckout = 2,
    RazorpayApi = 3,
    RazorpayWebhook = 4,
    Reconciliation = 5
}