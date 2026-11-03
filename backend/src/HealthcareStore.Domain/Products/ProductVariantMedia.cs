namespace HealthcareStore.Domain.Products;

public sealed class ProductVariantMedia
{
    private readonly List<ProductMedia> _media = [];

    private ProductVariantMedia()
    {
    }

    private ProductVariantMedia(
        Guid productVariantId,
        IEnumerable<ProductMedia> media)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }

        ProductVariantId = productVariantId;

        SetMedia(media);

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid ProductVariantId { get; private set; }

    public IReadOnlyList<ProductMedia> Media =>
        _media;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProductVariantMedia Create(
        Guid productVariantId,
        IEnumerable<ProductMedia> media)
    {
        ArgumentNullException.ThrowIfNull(media);

        return new ProductVariantMedia(
            productVariantId,
            media);
    }

    public static ProductVariantMedia Reconstitute(
        Guid productVariantId,
        IEnumerable<ProductMedia> media,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(media);

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }

        var variantMedia = new ProductVariantMedia
        {
            ProductVariantId = productVariantId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        variantMedia.SetMedia(media);

        return variantMedia;
    }

    public void AddMedia(ProductMedia media)
    {
        ArgumentNullException.ThrowIfNull(media);

        if (_media.Any(x => x.Id == media.Id))
        {
            throw new InvalidOperationException(
                "The specified media already exists.");
        }

        if (_media.Count == 0)
        {
            media.SetPrimaryStatus();
        }
        else if (media.IsPrimary)
        {
            foreach (var item in _media)
            {
                item.RemovePrimaryStatus();
            }
        }

        _media.Add(media);

        Touch();
    }

    public void ReplaceMedia(
        Guid mediaId,
        ProductMedia replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        var index =
            _media.FindIndex(x => x.Id == mediaId);

        if (index < 0)
        {
            throw new InvalidOperationException(
                "The specified media was not found.");
        }

        if (replacement.Id != mediaId)
        {
            throw new ArgumentException(
                "Replacement media ID must match the existing media ID.",
                nameof(replacement));
        }

        var existing = _media[index];

        if (replacement.IsPrimary)
        {
            foreach (var item in _media)
            {
                item.RemovePrimaryStatus();
            }
        }
        else if (existing.IsPrimary)
        {
            replacement.SetPrimaryStatus();
        }

        _media[index] = replacement;

        Touch();
    }

    public void RemoveMedia(Guid mediaId)
    {
        var index =
            _media.FindIndex(x => x.Id == mediaId);

        if (index < 0)
        {
            throw new InvalidOperationException(
                "The specified media was not found.");
        }

        var wasPrimary =
            _media[index].IsPrimary;

        _media.RemoveAt(index);

        if (wasPrimary && _media.Count > 0)
        {
            _media[0].SetPrimaryStatus();
        }

        Touch();
    }

    private void SetMedia(IEnumerable<ProductMedia> media)
    {
        var items = media.ToList();

        if (items.Any(static x => x is null))
        {
            throw new ArgumentException(
                "Product variant media cannot contain null items.",
                nameof(media));
        }

        if (items
            .Select(static x => x.Id)
            .Distinct()
            .Count() != items.Count)
        {
            throw new InvalidOperationException(
                "Product variant media contains duplicate media IDs.");
        }

        if (items.Count(x => x.IsPrimary) > 1)
        {
            throw new InvalidOperationException(
                "Product variant media cannot contain multiple primary media items.");
        }

        if (items.Count > 0 &&
            items.All(static x => !x.IsPrimary))
        {
            items[0].SetPrimaryStatus();
        }

        _media.Clear();
        _media.AddRange(items);
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class ProductMedia
{
    private ProductMedia(
        Guid id,
        string? altText,
        int sortOrder,
        bool isPrimary,
        string? provider,
        string? assetId,
        string? publicId,
        string url)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Media ID cannot be empty.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");
        }

        Id = id;
        AltText = Normalize(altText);
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
        Provider = Normalize(provider);
        AssetId = Normalize(assetId);
        PublicId = Normalize(publicId);
        Url = url.Trim();
    }

    public Guid Id { get; }

    public string? AltText { get; }

    public int SortOrder { get; }

    public bool IsPrimary { get; private set; }

    public string? Provider { get; }

    public string? AssetId { get; }

    public string? PublicId { get; }

    public string Url { get; }

    public static ProductMedia Create(
        Guid id,
        string? altText,
        int sortOrder,
        bool isPrimary,
        string? provider,
        string? assetId,
        string? publicId,
        string url)
    {
        return new ProductMedia(
            id,
            altText,
            sortOrder,
            isPrimary,
            provider,
            assetId,
            publicId,
            url);
    }

    public static ProductMedia Reconstitute(
        Guid id,
        string? altText,
        int sortOrder,
        bool isPrimary,
        string? provider,
        string? assetId,
        string? publicId,
        string url)
    {
        return new ProductMedia(
            id,
            altText,
            sortOrder,
            isPrimary,
            provider,
            assetId,
            publicId,
            url);
    }

    public void SetPrimaryStatus()
    {
        IsPrimary = true;
    }

    public void RemovePrimaryStatus()
    {
        IsPrimary = false;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}