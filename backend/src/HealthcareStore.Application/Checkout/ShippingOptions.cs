namespace HealthcareStore.Application.Checkout;

public sealed class ShippingOptions
{
    public decimal FreeShippingThreshold { get; init; } = 1000m;

    public decimal StandardFee { get; init; } = 70m;
}