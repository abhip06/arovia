namespace HealthcareStore.Application.Checkout;

public interface IShippingFeeCalculator
{
    decimal Calculate(
        ShippingCalculationContext context);
}

public sealed record ShippingCalculationContext(
    decimal Subtotal,
    string Pincode);