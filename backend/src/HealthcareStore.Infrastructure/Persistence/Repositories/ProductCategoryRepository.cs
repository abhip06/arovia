using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ProductCategoryRepository(
    ApplicationDbContext _dbContext) : IProductCategoryRepository
{
    private const string UniqueIndexName = "IX_product_categories_ProductId_CategoryId";

    public Task<ProductType?> GetProductTypeAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == productId)
            .Select(x => (ProductType?)x.ProductType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<CategoryInfo?> GetCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.Id == categoryId)
            .Select(x => new CategoryInfo(
                x.ProductType,
                x.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> AssignmentExistsAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductCategories
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ProductId == productId &&
                    x.CategoryId == categoryId,
                cancellationToken);
    }

    public async Task<ProductCategoryInsertResult> TryCreateAsync(
        ProductCategory productCategory,
        CancellationToken cancellationToken)
    {
        _dbContext.ProductCategories.Add(productCategory);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ProductCategoryInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, UniqueIndexName))
        {
            _dbContext.Detach(productCategory);

            return ProductCategoryInsertResult.Duplicate;
        }
    }

    public Task<ProductCategory?> GetAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductCategories
            .FirstOrDefaultAsync(
                x =>
                    x.ProductId == productId &&
                    x.CategoryId == categoryId,
                cancellationToken);
    }

    public void Remove(ProductCategory productCategory)
    {
        _dbContext.ProductCategories.Remove(productCategory);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}