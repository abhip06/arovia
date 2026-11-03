namespace HealthcareStore.Infrastructure.Payments.Razorpay;

public sealed class RazorpayOptions
{
    public const string SectionName = "Payments:Razorpay";

    public string KeyId { get; set; } = string.Empty;

    public string KeySecret { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;
}