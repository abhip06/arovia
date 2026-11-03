using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ProductVariantRepository
    : IProductVariantRepository
{
    private const string IdentityIndexName = "UX_product_variants_Identity";
    private const string SkuIndexName = "IX_product_variants_Sku";
    private const string BarcodeIndexName = "IX_product_variants_Barcode";

    private readonly ApplicationDbContext _dbContext;

    public ProductVariantRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IdentityExistsAsync(
        Guid productId,
        decimal packageQuantity,
        string packageUnit,
        decimal? netQuantity,
        string? netQuantityUnit,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductVariants
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ProductId == productId &&
                    x.PackageQuantity == packageQuantity &&
                    x.PackageUnit == packageUnit &&
                    x.NetQuantity == netQuantity &&
                    x.NetQuantityUnit == netQuantityUnit,
                cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(
        Guid productVariantId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductVariants
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == productVariantId,
                cancellationToken);
    }

    public async Task<ProductVariantInsertResult> TryCreateAsync(
        ProductVariant variant,
        CancellationToken cancellationToken)
    {
        _dbContext.ProductVariants.Add(variant);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return ProductVariantInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, IdentityIndexName))
        {
            _dbContext.Detach(variant);
            return ProductVariantInsertResult.DuplicateIdentity;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, SkuIndexName))
        {
            _dbContext.Detach(variant);
            return ProductVariantInsertResult.DuplicateSku;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, BarcodeIndexName))
        {
            _dbContext.Detach(variant);
            return ProductVariantInsertResult.DuplicateBarcode;
        }
    }

    public Task<ProductVariantPriceInfo?> GetForPriceCreationAsync(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.Id == variantId)
            .Select(x => new ProductVariantPriceInfo(
                x.ProductId,
                x.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ProductVariant?> GetForUpdateAsync(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductVariants
            .FirstOrDefaultAsync(
                x => x.Id == variantId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ProductVariantStatus?> GetStatusAsync(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.Id == variantId)
            .Select(x => (ProductVariantStatus?)x.Status)
            .FirstOrDefaultAsync(cancellationToken);
    }
}