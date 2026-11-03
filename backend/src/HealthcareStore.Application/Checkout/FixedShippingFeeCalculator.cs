using Microsoft.Extensions.Options;
namespace HealthcareStore.Application.Checkout;

public sealed class FixedShippingFeeCalculator(
    IOptions<ShippingOptions> options)
    : IShippingFeeCalculator
{
    private readonly ShippingOptions _options =
        options.Value;

    public decimal Calculate(
        ShippingCalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Subtotal <= 0m)
        {
            return 0m;
        }

        return context.Subtotal <
               _options.FreeShippingThreshold
            ? _options.StandardFee
            : 0m;
    }
}