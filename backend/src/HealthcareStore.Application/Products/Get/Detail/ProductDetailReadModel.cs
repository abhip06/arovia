namespace HealthcareStore.Application.Products.Get.Detail;

public sealed record ProductDetailBaseReadModel(
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string ProductSlug,
    string ProductType,
    bool RequiresPrescription,
    string? BrandName,
    string? BrandSlug,
    string ManufacturerName,
    string ManufacturerSlug,
    string Sku,
    string? Barcode,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit,
    decimal SellingPrice,
    bool Returnable,
    int ReturnWindowDays,
    IReadOnlyList<ProductDetailCategoryReadModel> Categories,
    IReadOnlyList<ProductDetailIngredientReadModel> Ingredients);

public sealed record ProductDetailCategoryReadModel(
    string Name,
    string Slug);

public sealed record ProductDetailIngredientReadModel(
    string Name,
    string Slug,
    decimal StrengthValue,
    string StrengthUnit,
    string Role);

public sealed record ProductDetailAvailabilityReadModel(
    string Pincode,
    bool IsServiceable,
    bool IsAvailable,
    int AvailableQuantity,
    decimal? Mrp);

public sealed record ProductDetailStockAvailabilityReadModel(
    int AvailableQuantity,
    decimal Mrp);

public sealed record ProductDetailReadModel(
    ProductDetailBaseReadModel Base,
    ProductInformationReadModel? Information,
    ProductDetailAvailabilityReadModel? Availability,
    IReadOnlyList<ProductDetailImageReadModel> Images);

public sealed record ProductInformationReadModel(
    string? Description,
    IReadOnlyList<string> Highlights,
    string? Directions,
    IReadOnlyList<string> Warnings,
    string? StorageInstructions,
    IReadOnlyList<string> SideEffects,
    string? HowToUse,
    IReadOnlyList<ProductInformationSpecificationReadModel> Specifications,
    IReadOnlyList<ProductInformationFaqReadModel> Faqs);

public sealed record ProductInformationSpecificationReadModel(
    string Key,
    string Value);

public sealed record ProductInformationFaqReadModel(
    string Question,
    string Answer);

public sealed record ProductDetailImageReadModel(
    string? AltText,
    int SortOrder,
    bool IsPrimary,
    string Url);
