using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Products.Get;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ProductCatalogRepository(
    ApplicationDbContext dbContext) : IProductCatalogRepository
{
    public async Task<(IReadOnlyList<ProductCatalogListingRow> Items, int TotalCount)>
        GetPageAsync(
            ProductCatalogFilter filter,
            int skip,
            int pageSize,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var today =
            DateOnly.FromDateTime(now.UtcDateTime);

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
                product.Status.ToString() == "Active" &&
                variant.Status.ToString() == "Active" &&
                sellingPrice.HasValue
            select new
            {
                ProductVariantId = variant.Id,
                ProductName = product.Name,
                ProductSlug = product.Slug,

                BrandName =
                    brand == null
                        ? null
                        : brand.Name,

                Sku = variant.Sku,
                PackageQuantity = variant.PackageQuantity,
                PackageUnit = variant.PackageUnit,
                NetQuantity = variant.NetQuantity,
                NetQuantityUnit = variant.NetQuantityUnit,

                SellingPrice = sellingPrice.Value,

                ProductType = product.ProductType,
                BrandSlug =
                    brand == null
                        ? null
                        : brand.Slug,
                ManufacturerSlug = manufacturer.Slug,
                ManufacturerName = manufacturer.Name,
                ProductId = product.Id
            };

        if (!string.IsNullOrWhiteSpace(filter.ProductType))
        {
            query = query.Where(x =>
                x.ProductType.ToString() == filter.ProductType);
        }

        if (!string.IsNullOrWhiteSpace(filter.BrandSlug))
        {
            query = query.Where(x =>
                x.BrandSlug == filter.BrandSlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.ManufacturerSlug))
        {
            query = query.Where(x =>
                x.ManufacturerSlug == filter.ManufacturerSlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            query = query.Where(x =>
                dbContext.ProductCategories
                    .Join(
                        dbContext.Categories,
                        productCategory => productCategory.CategoryId,
                        category => category.Id,
                        (productCategory, category) => new
                        {
                            productCategory.ProductId,
                            category.Slug,
                            category.Status
                        })
                    .Any(category =>
                        category.ProductId == x.ProductId &&
                        category.Slug == filter.CategorySlug &&
                        category.Status.ToString() == "Active"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search}%";

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.ProductName,
                    pattern) ||
                EF.Functions.ILike(
                    x.Sku,
                    pattern) ||
                (x.BrandName != null &&
                 EF.Functions.ILike(
                     x.BrandName,
                     pattern)) ||
                EF.Functions.ILike(
                    x.ManufacturerName,
                    pattern));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
                .OrderBy(x => x.ProductName)
                .ThenBy(x => x.PackageQuantity)
                .ThenBy(x => x.PackageUnit)
                .ThenBy(x => x.Sku)
                .Skip(skip)
                .Take(pageSize)
                .Select(x =>
                    new ProductCatalogListingRow(
                        x.ProductVariantId,
                        x.ProductName,
                        x.ProductSlug,
                        x.BrandName,
                        x.Sku,
                        x.PackageQuantity,
                        x.PackageUnit,
                        x.NetQuantity,
                        x.NetQuantityUnit,
                        x.SellingPrice))
                .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}