namespace HealthcareStore.Api.Catalog.Get.Detail;

public sealed record ProductDetailResponse(
    ProductDetailProductResponse Product,
    ProductDetailVariantResponse Variant,
    ProductInformationResponse? Information,
    ProductDetailAvailabilityResponse? Availability,
    IReadOnlyList<ProductDetailImageResponse> Images);

public sealed record ProductDetailProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string Type,
    bool RequiresPrescription,
    ProductDetailCatalogReferenceResponse? Brand,
    ProductDetailCatalogReferenceResponse Manufacturer,
    bool Returnable,
    int ReturnWindowDays,
    IReadOnlyList<ProductDetailCategoryResponse> Categories,
    IReadOnlyList<ProductDetailIngredientResponse> Ingredients);

public sealed record ProductDetailVariantResponse(
    Guid Id,
    string Sku,
    string? Barcode,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit,
    decimal SellingPrice);

public sealed record ProductDetailCatalogReferenceResponse(
    string Name,
    string Slug);

public sealed record ProductDetailCategoryResponse(
    string Name,
    string Slug);

public sealed record ProductDetailIngredientResponse(
    string Name,
    string Slug,
    decimal StrengthValue,
    string StrengthUnit,
    string Role);

public sealed record ProductInformationResponse(
    string? Description,
    IReadOnlyList<string> Highlights,
    string? Directions,
    IReadOnlyList<string> Warnings,
    string? StorageInstructions,
    IReadOnlyList<string> SideEffects,
    string? HowToUse,
    IReadOnlyList<ProductInformationSpecificationResponse> Specifications,
    IReadOnlyList<ProductInformationFaqResponse> Faqs);

public sealed record ProductInformationSpecificationResponse(
    string Key,
    string Value);

public sealed record ProductInformationFaqResponse(
    string Question,
    string Answer);

public sealed record ProductDetailAvailabilityResponse(
    string Pincode,
    bool IsServiceable,
    bool IsAvailable,
    int AvailableQuantity,
    decimal? Mrp);

public sealed record ProductDetailImageResponse(
    string? AltText,
    int SortOrder,
    bool IsPrimary,
    string Url);
