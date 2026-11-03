namespace HealthcareStore.Domain.Products;

public sealed class ProductInformation
{
    private readonly List<string> _highlights = [];
    private readonly List<string> _warnings = [];
    private readonly List<string> _sideEffects = [];
    private readonly Dictionary<string, string> _specifications = [];
    private readonly List<ProductFaq> _faqs = [];

    private ProductInformation()
    {
    }

    private ProductInformation(
        Guid productId,
        string? description,
        IEnumerable<string> highlights,
        string? directions,
        IEnumerable<string> warnings,
        string? storageInstructions,
        IEnumerable<string> sideEffects,
        string? howToUse,
        IEnumerable<KeyValuePair<string, string>> specifications,
        IEnumerable<ProductFaq> faqs)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));
        }

        ProductId = productId;

        SetContent(
            description,
            highlights,
            directions,
            warnings,
            storageInstructions,
            sideEffects,
            howToUse,
            specifications,
            faqs);

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid ProductId { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyList<string> Highlights =>
        _highlights;

    public string? Directions { get; private set; }

    public IReadOnlyList<string> Warnings =>
        _warnings;

    public string? StorageInstructions { get; private set; }

    public IReadOnlyList<string> SideEffects =>
        _sideEffects;

    public string? HowToUse { get; private set; }

    public IReadOnlyDictionary<string, string> Specifications =>
        _specifications;

    public IReadOnlyList<ProductFaq> Faqs =>
        _faqs;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProductInformation Create(
        Guid productId,
        string? description,
        IEnumerable<string> highlights,
        string? directions,
        IEnumerable<string> warnings,
        string? storageInstructions,
        IEnumerable<string> sideEffects,
        string? howToUse,
        IEnumerable<KeyValuePair<string, string>> specifications,
        IEnumerable<ProductFaq> faqs)
    {
        ArgumentNullException.ThrowIfNull(highlights);
        ArgumentNullException.ThrowIfNull(warnings);
        ArgumentNullException.ThrowIfNull(sideEffects);
        ArgumentNullException.ThrowIfNull(specifications);
        ArgumentNullException.ThrowIfNull(faqs);

        return new ProductInformation(
            productId,
            description,
            highlights,
            directions,
            warnings,
            storageInstructions,
            sideEffects,
            howToUse,
            specifications,
            faqs);
    }

    public void Update(
        string? description,
        IEnumerable<string> highlights,
        string? directions,
        IEnumerable<string> warnings,
        string? storageInstructions,
        IEnumerable<string> sideEffects,
        string? howToUse,
        IEnumerable<KeyValuePair<string, string>> specifications,
        IEnumerable<ProductFaq> faqs)
    {
        ArgumentNullException.ThrowIfNull(highlights);
        ArgumentNullException.ThrowIfNull(warnings);
        ArgumentNullException.ThrowIfNull(sideEffects);
        ArgumentNullException.ThrowIfNull(specifications);
        ArgumentNullException.ThrowIfNull(faqs);

        SetContent(
            description,
            highlights,
            directions,
            warnings,
            storageInstructions,
            sideEffects,
            howToUse,
            specifications,
            faqs);

        Touch();
    }

    public static ProductInformation Reconstitute(
        Guid productId,
        string? description,
        IEnumerable<string> highlights,
        string? directions,
        IEnumerable<string> warnings,
        string? storageInstructions,
        IEnumerable<string> sideEffects,
        string? howToUse,
        IEnumerable<KeyValuePair<string, string>> specifications,
        IEnumerable<ProductFaq> faqs,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(highlights);
        ArgumentNullException.ThrowIfNull(warnings);
        ArgumentNullException.ThrowIfNull(sideEffects);
        ArgumentNullException.ThrowIfNull(specifications);
        ArgumentNullException.ThrowIfNull(faqs);

        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));
        }

        var information = new ProductInformation
        {
            ProductId = productId,
            Description = Normalize(description),
            Directions = Normalize(directions),
            StorageInstructions = Normalize(storageInstructions),
            HowToUse = Normalize(howToUse),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        information._highlights.AddRange(
            highlights
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x.Trim()));

        information._warnings.AddRange(
            warnings
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x.Trim()));

        information._sideEffects.AddRange(
            sideEffects
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x.Trim()));

        foreach (var specification in specifications)
        {
            if (string.IsNullOrWhiteSpace(specification.Key) ||
                string.IsNullOrWhiteSpace(specification.Value))
            {
                continue;
            }

            information._specifications[specification.Key.Trim()] =
                specification.Value.Trim();
        }

        information._faqs.AddRange(faqs);

        return information;
    }

    private void SetContent(
        string? description,
        IEnumerable<string> highlights,
        string? directions,
        IEnumerable<string> warnings,
        string? storageInstructions,
        IEnumerable<string> sideEffects,
        string? howToUse,
        IEnumerable<KeyValuePair<string, string>> specifications,
        IEnumerable<ProductFaq> faqs)
    {
        Description = Normalize(description);
        Directions = Normalize(directions);
        StorageInstructions = Normalize(storageInstructions);
        HowToUse = Normalize(howToUse);

        _highlights.Clear();
        _highlights.AddRange(
            highlights
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x.Trim()));

        _warnings.Clear();
        _warnings.AddRange(
            warnings
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x.Trim()));

        _sideEffects.Clear();
        _sideEffects.AddRange(
            sideEffects
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x.Trim()));

        _specifications.Clear();

        foreach (var specification in specifications)
        {
            if (string.IsNullOrWhiteSpace(specification.Key) ||
                string.IsNullOrWhiteSpace(specification.Value))
            {
                continue;
            }

            _specifications[specification.Key.Trim()] =
                specification.Value.Trim();
        }

        _faqs.Clear();
        _faqs.AddRange(faqs);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed record ProductFaq(
    string Question,
    string Answer);