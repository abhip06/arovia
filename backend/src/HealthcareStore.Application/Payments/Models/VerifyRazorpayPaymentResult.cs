using HealthcareStore.Application.Orders.Models;

namespace HealthcareStore.Application.Payments.Models;

public sealed record VerifyRazorpayPaymentResult(
    VerifyRazorpayPaymentStatus Status,
    OrderDetailsModel? Order = null);