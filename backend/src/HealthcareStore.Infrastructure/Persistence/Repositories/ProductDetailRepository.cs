using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Products.Get.Detail;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ProductDetailRepository(
    ApplicationDbContext dbContext) : IProductDetailRepository
{
    public async Task<ProductDetailBaseReadModel?> GetAsync(
        string productSlug,
        string sku,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productSlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        var query =
            from variant in dbContext.ProductVariants.AsNoTracking()
            join product in dbContext.Products.AsNoTracking()
                on variant.ProductId equals product.Id
            join manufacturer in dbContext.Manufacturers.AsNoTracking()
                on product.ManufacturerId equals manufacturer.Id
            join brand in dbContext.Brands.AsNoTracking()
                on product.BrandId equals brand.Id into brandGroup
            from brand in brandGroup.DefaultIfEmpty()
            let sellingPrice =
                dbContext.ProductPrices
                    .AsNoTracking()
                    .Where(price =>
                        price.ProductVariantId == variant.Id &&
                        price.EffectiveFrom <= now)
                    .OrderByDescending(price => price.EffectiveFrom)
                    .Select(price => (decimal?)price.SellingPrice)
                    .FirstOrDefault()
            where
                product.Slug == productSlug &&
                variant.Sku == sku &&
                product.Status == ProductStatus.Active &&
                variant.Status == ProductVariantStatus.Active &&
                sellingPrice.HasValue
            select new ProductDetailBaseReadModel(
                product.Id,
                variant.Id,
                product.Name,
                product.Slug,
                product.ProductType.ToString(),
                product.RequiresPrescription,
                brand == null ? null : brand.Name,
                brand == null ? null : brand.Slug,
                manufacturer.Name,
                manufacturer.Slug,
                variant.Sku,
                variant.Barcode,
                variant.PackageQuantity,
                variant.PackageUnit,
                variant.NetQuantity,
                variant.NetQuantityUnit,
                sellingPrice!.Value,
                product.Returnable,
                product.ReturnWindowDays,
                (
                    from productCategory in dbContext.ProductCategories.AsNoTracking()
                    join category in dbContext.Categories.AsNoTracking()
                        on productCategory.CategoryId equals category.Id
                    where
                        productCategory.ProductId == product.Id &&
                        category.Status == CatalogEntityStatus.Active
                    orderby category.SortOrder, category.Name
                    select new ProductDetailCategoryReadModel(
                        category.Name,
                        category.Slug)
                ).ToList(),
                (
                    from productIngredient in dbContext.ProductIngredients.AsNoTracking()
                    join ingredient in dbContext.ActiveIngredients.AsNoTracking()
                        on productIngredient.ActiveIngredientId equals ingredient.Id
                    where productIngredient.ProductId == product.Id
                    orderby ingredient.Name, productIngredient.Role
                    select new ProductDetailIngredientReadModel(
                        ingredient.Name,
                        ingredient.Slug,
                        productIngredient.StrengthValue,
                        productIngredient.StrengthUnit,
                        productIngredient.Role.ToString())
                ).ToList());

        return await query.SingleOrDefaultAsync(cancellationToken);
    }
}
