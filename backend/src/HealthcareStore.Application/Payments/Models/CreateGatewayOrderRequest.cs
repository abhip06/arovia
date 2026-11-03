namespace HealthcareStore.Application.Payments.Models;

public sealed record CreateGatewayOrderRequest(
    long AmountSubunits,
    string Currency,
    string Receipt);