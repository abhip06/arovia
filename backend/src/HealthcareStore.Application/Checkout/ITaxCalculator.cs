namespace HealthcareStore.Application.Checkout;

public interface ITaxCalculator
{
    decimal Calculate(
        decimal taxableAmount,
        decimal taxRate);
}