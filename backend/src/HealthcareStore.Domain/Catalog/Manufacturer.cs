using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Catalog;

public sealed class Manufacturer : Entity
{
    private Manufacturer() { }

    private Manufacturer(string name, string slug)
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

    public static Manufacturer Create(string name, string slug)
    {
        return new Manufacturer(name, slug);
    }

    public void Activate()
    {
        Status = CatalogEntityStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = CatalogEntityStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}