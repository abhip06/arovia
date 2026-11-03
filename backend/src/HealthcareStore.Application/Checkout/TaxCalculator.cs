namespace HealthcareStore.Application.Checkout;

public sealed class TaxCalculator : ITaxCalculator
{
    public decimal Calculate(
        decimal taxableAmount,
        decimal taxRate)
    {
        if (taxableAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxableAmount),
                "Taxable amount cannot be negative.");
        }

        if (taxRate is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxRate),
                "Tax rate must be between 0 and 100.");
        }

        return Math.Round(
            taxableAmount * taxRate / 100m,
            2,
            MidpointRounding.AwayFromZero);
    }
}