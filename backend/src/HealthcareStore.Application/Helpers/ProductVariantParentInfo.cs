using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Helpers;

public sealed record ProductVariantParentInfo(
    string Name,
    ProductStatus Status);