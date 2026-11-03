using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Products;

public sealed class Product : AggregateRoot
{
    private Product() { }

    private Product(
        string name,
        string slug,
        ProductType productType,
        Guid manufacturerId,
        Guid? brandId,
        bool requiresPrescription,
        string hsnCode,
        decimal taxRate,
        bool returnable,
        int returnWindowDays)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        if (manufacturerId == Guid.Empty)
            throw new ArgumentException(
                "Manufacturer ID cannot be empty.",
                nameof(manufacturerId));

        ArgumentException.ThrowIfNullOrWhiteSpace(hsnCode);

        if (taxRate < 0)
            throw new ArgumentOutOfRangeException(
                nameof(taxRate),
                "Tax rate cannot be negative.");

        if (returnWindowDays < 0)
            throw new ArgumentOutOfRangeException(
                nameof(returnWindowDays),
                "Return window cannot be negative.");

        if (!returnable && returnWindowDays != 0)
            throw new ArgumentException(
                "Return window must be zero when the product is not returnable.",
                nameof(returnWindowDays));

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        Slug = slug.Trim().ToLowerInvariant();

        ProductType = productType;
        Status = ProductStatus.Draft;
        ManufacturerId = manufacturerId;
        BrandId = brandId;
        RequiresPrescription = requiresPrescription;
        HsnCode = hsnCode.Trim();
        TaxRate = taxRate;
        Returnable = returnable;
        ReturnWindowDays = returnWindowDays;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    public ProductType ProductType { get; private set; }
    public ProductStatus Status { get; private set; }

    public Guid ManufacturerId { get; private set; }
    public Guid? BrandId { get; private set; }

    private readonly List<ProductIngredient> _ingredients = [];
    public IReadOnlyCollection<ProductIngredient> Ingredients => _ingredients;

    public bool RequiresPrescription { get; private set; }

    public string HsnCode { get; private set; } = null!;
    public decimal TaxRate { get; private set; }

    public bool Returnable { get; private set; }
    public int ReturnWindowDays { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Product Create(
        string name,
        string slug,
        ProductType productType,
        Guid manufacturerId,
        Guid? brandId,
        bool requiresPrescription,
        string hsnCode,
        decimal taxRate,
        bool returnable,
        int returnWindowDays)
    {
        return new Product(
            name,
            slug,
            productType,
            manufacturerId,
            brandId,
            requiresPrescription,
            hsnCode,
            taxRate,
            returnable,
            returnWindowDays);
    }

    public void Activate()
    {
        if (Status == ProductStatus.Active)
        {
            throw new InvalidOperationException(
                "The product is already active.");
        }

        if (Status == ProductStatus.Discontinued)
        {
            throw new InvalidOperationException(
                "A discontinued product cannot be activated.");
        }

        Status = ProductStatus.Active;
        Touch();
    }

    public void Deactivate()
    {
        if (Status != ProductStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active product can be deactivated.");
        }

        Status = ProductStatus.Inactive;
        Touch();
    }

    public void Discontinue()
    {
        if (Status != ProductStatus.Active &&
            Status != ProductStatus.Inactive)
        {
            throw new InvalidOperationException(
                "Only an active or inactive product can be discontinued.");
        }

        Status = ProductStatus.Discontinued;
        Touch();
    }

    public void AddIngredient(ProductIngredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (_ingredients.Any(x =>
            x.ActiveIngredientId == ingredient.ActiveIngredientId &&
            x.Role == ingredient.Role))
        {
            throw new InvalidOperationException(
                "The product already contains this ingredient.");
        }

        _ingredients.Add(ingredient);
        Touch();
    }

    public void RemoveIngredient(Guid productIngredientId)
    {
        var ingredient = _ingredients
            .FirstOrDefault(x => x.Id == productIngredientId);

        if (ingredient is null)
        {
            throw new InvalidOperationException(
                "The specified product ingredient was not found.");
        }

        _ingredients.Remove(ingredient);
        Touch();
    }

    public static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return string.Join(
            ' ',
            name.Trim()
                .Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(static part => part.ToLowerInvariant()));
    }

    public void UpdateDetails(
        string name,
        Guid manufacturerId,
        Guid? brandId,
        bool requiresPrescription,
        string hsnCode,
        decimal taxRate,
        bool returnable,
        int returnWindowDays)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (manufacturerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Manufacturer ID cannot be empty.",
                nameof(manufacturerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(hsnCode);

        if (taxRate < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxRate),
                "Tax rate cannot be negative.");
        }

        if (returnWindowDays < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnWindowDays),
                "Return window cannot be negative.");
        }

        if (!returnable && returnWindowDays != 0)
        {
            throw new ArgumentException(
                "Return window must be zero when the product is not returnable.",
                nameof(returnWindowDays));
        }

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);

        ManufacturerId = manufacturerId;
        BrandId = brandId;
        RequiresPrescription = requiresPrescription;
        HsnCode = hsnCode.Trim();
        TaxRate = taxRate;
        Returnable = returnable;
        ReturnWindowDays = returnWindowDays;

        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}