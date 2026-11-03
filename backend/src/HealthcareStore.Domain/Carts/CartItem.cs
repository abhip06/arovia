using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Carts;

public sealed class CartItem : Entity
{
    public const int MaxQuantity = 25;

    private CartItem()
    {
    }

    private CartItem(
        Guid cartId,
        Guid productVariantId,
        int quantity)
    {
        CartId = cartId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid CartId { get; private set; }

    public Guid ProductVariantId { get; private set; }

    public int Quantity { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static CartItem Create(
        Guid cartId,
        Guid productVariantId,
        int quantity)
    {
        if (cartId == Guid.Empty)
        {
            throw new ArgumentException(
                "Cart ID cannot be empty.",
                nameof(cartId));
        }

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }

        ValidateQuantity(quantity);

        return new CartItem(
            cartId,
            productVariantId,
            quantity);
    }

    public void IncreaseQuantity(int quantity)
    {
        ValidateQuantity(quantity);

        var newQuantity =
            Quantity + quantity;

        if (newQuantity > MaxQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                $"Cart item quantity cannot exceed {MaxQuantity}.");
        }

        Quantity = newQuantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetQuantity(int quantity)
    {
        ValidateQuantity(quantity);

        Quantity = quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > MaxQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                $"Quantity must be between 1 and {MaxQuantity}.");
        }
    }
}