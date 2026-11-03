export function formatPrice(
    value: number,
    currency = "INR",
): string {
    return new Intl.NumberFormat("en-IN", {
        style: "currency",
        currency,
        maximumFractionDigits: 2,
    }).format(value);
}