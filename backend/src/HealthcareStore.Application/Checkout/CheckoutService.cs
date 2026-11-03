using HealthcareStore.Application.Abstractions.Persistence;

namespace HealthcareStore.Application.Checkout;

public sealed class CheckoutService(
    ICheckoutProductReader productReader,
    ITaxCalculator taxCalculator,
    IShippingFeeCalculator shippingFeeCalculator,
    TimeProvider timeProvider)
{
    public async Task<CheckoutReadModel> GetAsync(
        Guid userId,
        string pincode,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        var normalizedPincode = NormalizePincode(pincode);

        var now = timeProvider.GetUtcNow();

        var data = await productReader.GetAsync(
            userId,
            normalizedPincode,
            now,
            cancellationToken);

        if (data.Items.Count == 0)
        {
            return new CheckoutReadModel(
                Id: data.CartId,
                Items: [],
                ItemCount: 0,
                Subtotal: 0m,
                DiscountTotal: 0m,
                TotalTax: 0m,
                ShippingFee: 0m,
                GrandTotal: 0m,
                HasUnavailableItems: false);
        }

        var checkoutItems = new List<CheckoutItemReadModel>(
            data.Items.Count);

        var itemCount = 0;
        var subtotal = 0m;
        var discountTotal = 0m;
        var totalTax = 0m;
        var hasUnavailableItems = false;

        foreach (var item in data.Items)
        {
            var mrp = item.Mrp;

            var isAvailable =
                item.IsProductActive &&
                item.IsVariantActive &&
                mrp >= 0m &&
                item.SellingPrice >= 0m &&
                item.SellingPrice <= mrp &&
                item.AvailableQuantity >= item.Quantity;

            hasUnavailableItems |= !isAvailable;

            decimal? unitPrice = isAvailable
                ? item.SellingPrice
                : null;

            decimal? lineTotal = null;
            decimal? lineDiscountAmount = null;
            decimal? lineTaxAmount = null;

            if (isAvailable)
            {
                var lineSubtotal = Math.Round(
                    mrp * item.Quantity,
                    2,
                    MidpointRounding.AwayFromZero);

                // Selling-price amount before tax.
                lineTotal = Math.Round(
                    unitPrice!.Value * item.Quantity,
                    2,
                    MidpointRounding.AwayFromZero);

                // Difference between MRP subtotal and selling-price total.
                lineDiscountAmount = Math.Round(
                    lineSubtotal - lineTotal.Value,
                    2,
                    MidpointRounding.AwayFromZero);

                // Calculate tax on the discounted selling-price amount.
                lineTaxAmount = Math.Round(
                    taxCalculator.Calculate(
                        lineTotal.Value,
                        item.TaxRate),
                    2,
                    MidpointRounding.AwayFromZero);

                subtotal += lineSubtotal;
                discountTotal += lineDiscountAmount.Value;
                totalTax += lineTaxAmount.Value;
            }

            itemCount += item.Quantity;

            checkoutItems.Add(
                new CheckoutItemReadModel(
                    Id: item.CartItemId,
                    ProductVariantId: item.ProductVariantId,
                    ProductId: item.ProductId,
                    ProductName: item.ProductName,
                    Sku: item.Sku,
                    PackageQuantity: item.PackageQuantity,
                    PackageUnit: item.PackageUnit,
                    RequiresPrescription: item.RequiresPrescription,
                    Quantity: item.Quantity,
                    Mrp: item.IsProductActive &&
                         item.IsVariantActive
                        ? mrp
                        : null,
                    UnitPrice: unitPrice,
                    LineTotal: lineTotal,
                    LineDiscountAmount: lineDiscountAmount,
                    LineTaxAmount: lineTaxAmount,
                    ImageUrl: item.ImageUrl,
                    IsAvailable: isAvailable));
        }

        subtotal = Math.Round(
            subtotal,
            2,
            MidpointRounding.AwayFromZero);

        discountTotal = Math.Round(
            discountTotal,
            2,
            MidpointRounding.AwayFromZero);

        totalTax = Math.Round(
            totalTax,
            2,
            MidpointRounding.AwayFromZero);

        var merchandiseTotal = Math.Round(
            subtotal - discountTotal,
            2,
            MidpointRounding.AwayFromZero);

        var shippingFee = shippingFeeCalculator.Calculate(
            new ShippingCalculationContext(
                merchandiseTotal,
                normalizedPincode));

        var grandTotal = Math.Round(
            merchandiseTotal + totalTax + shippingFee,
            2,
            MidpointRounding.AwayFromZero);

        return new CheckoutReadModel(
            Id: data.CartId,
            Items: checkoutItems,
            ItemCount: itemCount,
            Subtotal: subtotal,
            DiscountTotal: discountTotal,
            TotalTax: totalTax,
            ShippingFee: shippingFee,
            GrandTotal: grandTotal,
            HasUnavailableItems: hasUnavailableItems);
    }

    private static string NormalizePincode(string pincode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pincode);

        var normalized = pincode.Trim();

        if (normalized.Length != 6 ||
            normalized.Any(
                static character =>
                    character is < '0' or > '9'))
        {
            throw new ArgumentException(
                "Pincode must be a valid 6-digit Indian pincode.",
                nameof(pincode));
        }

        return normalized;
    }
}