using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Catalog;

public sealed class ActiveIngredient : Entity
{
    private ActiveIngredient() { }

    private ActiveIngredient(string name, string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Status = CatalogEntityStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public CatalogEntityStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ActiveIngredient Create(string name, string slug)
    {
        return new ActiveIngredient(name, slug);
    }

    public void Activate()
    {
        if (Status == CatalogEntityStatus.Active)
            throw new InvalidOperationException(
                "The active ingredient is already active.");

        Status = CatalogEntityStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == CatalogEntityStatus.Inactive)
            throw new InvalidOperationException(
                "The active ingredient is already inactive.");

        Status = CatalogEntityStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}