using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Helpers;

public sealed record ProductVariantPriceInfo(
    Guid ProductId,
    ProductVariantStatus Status);