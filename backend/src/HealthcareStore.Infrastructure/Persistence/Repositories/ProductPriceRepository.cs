using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Products;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ProductPriceRepository : IProductPriceRepository
{
    private const string EffectiveFromIndexName = "IX_product_prices_ProductVariantId_EffectiveFrom";

    private readonly ApplicationDbContext _dbContext;

    public ProductPriceRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductPrice?> GetForDeleteAsync(
        Guid priceId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProductPrices
            .FirstOrDefaultAsync(
                x => x.Id == priceId,
                cancellationToken);
    }

    public void Remove(ProductPrice price)
    {
        _dbContext.ProductPrices.Remove(price);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductPriceInsertResult> TryCreateAsync(
        ProductPrice price,
        CancellationToken cancellationToken)
    {
        _dbContext.ProductPrices.Add(price);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return ProductPriceInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(
                ex,
                EffectiveFromIndexName))
        {
            _dbContext.Detach(price);

            return ProductPriceInsertResult.DuplicateEffectiveFrom;
        }
    }

    public async Task<IReadOnlyList<ProductPrice>> GetHistoryAsync(
        Guid productVariantId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProductPrices
            .AsNoTracking()
            .Where(x => x.ProductVariantId == productVariantId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);
    }
}