using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private const string CatalogIdentityIndexName = "UX_products_CatalogIdentity";
    private const string SlugIndexName = "IX_products_Slug";

    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IdentityExistsAsync(
        ProductType productType,
        Guid manufacturerId,
        Guid? brandId,
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ProductType == productType &&
                    x.ManufacturerId == manufacturerId &&
                    x.BrandId == brandId &&
                    x.NormalizedName == normalizedName,
                cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == productId,
                cancellationToken);
    }

    public async Task<ProductInsertResult> TryCreateAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        _dbContext.Products.Add(product);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ProductInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, CatalogIdentityIndexName))
        {
            _dbContext.Detach(product);

            return ProductInsertResult.DuplicateCatalogIdentity;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, SlugIndexName))
        {
            _dbContext.Detach(product);

            return ProductInsertResult.DuplicateSlug;
        }
    }

    public Task<ProductVariantParentInfo?> GetForVariantCreationAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == productId)
            .Select(x => new ProductVariantParentInfo(
                x.Name,
                x.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Product?> GetWithIngredientsAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(
                x => x.Id == productId,
                cancellationToken);
    }

    public void AddIngredient(ProductIngredient ingredient)
    {
        _dbContext.ProductIngredients.Add(ingredient);
    }

    public void RemoveIngredient(ProductIngredient ingredient)
    {
        _dbContext.ProductIngredients.Remove(ingredient);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Product?> GetForUpdateAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .FirstOrDefaultAsync(
                x => x.Id == productId,
                cancellationToken);
    }

    public Task<bool> IdentityExistsForAnotherProductAsync(
        ProductType productType,
        Guid manufacturerId,
        Guid? brandId,
        string normalizedName,
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ProductType == productType &&
                    x.ManufacturerId == manufacturerId &&
                    x.BrandId == brandId &&
                    x.NormalizedName == normalizedName &&
                    x.Id != productId,
                cancellationToken);
    }

    public async Task<ProductUpdateResult> TryUpdateAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ProductUpdateResult.Updated;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(
                ex,
                CatalogIdentityIndexName))
        {
            return ProductUpdateResult.DuplicateCatalogIdentity;
        }
    }
}