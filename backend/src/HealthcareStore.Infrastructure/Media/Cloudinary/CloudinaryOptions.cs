namespace HealthcareStore.Infrastructure.Media.Cloudinary;

public sealed class CloudinaryOptions
{
    public string CloudName { get; init; } = null!;

    public string ApiKey { get; init; } = null!;

    public string ApiSecret { get; init; } = null!;

    public string ProductVariantMediaFolder { get; init; } = "arovia/products";
}