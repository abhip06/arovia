using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Carts;

public sealed class Cart : AggregateRoot
{
    public const int MaxDistinctItems = 50;

    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    private Cart(Guid userId)
    {
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        return new Cart(userId);
    }

    public CartItem AddItem(
        Guid productVariantId,
        int quantity)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }

        var existingItem =
            _items.SingleOrDefault(
                item => item.ProductVariantId == productVariantId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            Touch();

            return existingItem;
        }

        if (_items.Count >= MaxDistinctItems)
        {
            throw new InvalidOperationException(
                $"A cart cannot contain more than {MaxDistinctItems} distinct items.");
        }

        var item =
            CartItem.Create(
                Id,
                productVariantId,
                quantity);

        _items.Add(item);

        Touch();

        return item;
    }

    public bool SetItemQuantity(
        Guid cartItemId,
        int quantity)
    {
        var item =
            _items.SingleOrDefault(
                x => x.Id == cartItemId);

        if (item is null)
        {
            return false;
        }

        item.SetQuantity(quantity);

        Touch();

        return true;
    }

    public bool RemoveItem(Guid cartItemId)
    {
        var item =
            _items.SingleOrDefault(
                x => x.Id == cartItemId);

        if (item is null)
        {
            return false;
        }

        _items.Remove(item);

        Touch();

        return true;
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}