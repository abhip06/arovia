using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Helpers;

public sealed record CategoryParentInfo(
    ProductType ProductType,
    CatalogEntityStatus Status);