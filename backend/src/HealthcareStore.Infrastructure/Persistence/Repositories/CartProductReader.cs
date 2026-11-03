using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Carts.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class CartProductReader(
    ApplicationDbContext dbContext,
    IProductVariantMediaRepository mediaRepository)
    : ICartProductReader
{
    public async Task<IReadOnlyList<CartProductReadModel>> GetForCartAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productVariantIds);

        var ids =
            productVariantIds
                .Where(static id => id != Guid.Empty)
                .Distinct()
                .ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        var today =
            DateOnly.FromDateTime(
                now.UtcDateTime);

        var rows =
            await (
                from variant in dbContext.ProductVariants.AsNoTracking()
                join product in dbContext.Products.AsNoTracking()
                    on variant.ProductId equals product.Id
                where ids.Contains(variant.Id)
                select new CartProductDbRow
                {
                    ProductVariantId = variant.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = variant.Sku,
                    PackageQuantity = variant.PackageQuantity,
                    PackageUnit = variant.PackageUnit,
                    RequiresPrescription =
                        product.RequiresPrescription,

                    CurrentSellingPrice =
                        dbContext.ProductPrices
                            .Where(
                                price =>
                                    price.ProductVariantId ==
                                    variant.Id &&
                                    price.EffectiveFrom <= now)
                            .OrderByDescending(
                                price => price.EffectiveFrom)
                            .Select(
                                price =>
                                    (decimal?)
                                    price.SellingPrice)
                            .FirstOrDefault(),

                    Mrp =
                        dbContext.InventoryBatches
                            .Where(
                                batch =>
                                    batch.ProductVariantId ==
                                    variant.Id &&
                                    (
                                        batch.SellableUntil == null ||
                                        batch.SellableUntil >= today
                                    ) &&
                                    dbContext.WarehouseStocks.Any(
                                        stock =>
                                            stock.InventoryBatchId ==
                                            batch.Id &&
                                            stock.QuantityOnHand >
                                            stock.QuantityReserved))
                            .OrderBy(
                                batch =>
                                    batch.SellableUntil == null)
                            .ThenBy(
                                batch =>
                                    batch.SellableUntil)
                            .ThenBy(
                                batch =>
                                    batch.ExpiryDate)
                            .Select(
                                batch =>
                                    (decimal?)
                                    batch.Mrp)
                            .FirstOrDefault(),

                    HasSellableStock =
                        dbContext.InventoryBatches.Any(
                            batch =>
                                batch.ProductVariantId ==
                                variant.Id &&
                                (
                                    batch.SellableUntil == null ||
                                    batch.SellableUntil >= today
                                ) &&
                                dbContext.WarehouseStocks.Any(
                                    stock =>
                                        stock.InventoryBatchId ==
                                        batch.Id &&
                                        stock.QuantityOnHand >
                                        stock.QuantityReserved))
                })
            .ToListAsync(cancellationToken);

        var imageUrls =
            await mediaRepository.GetPrimaryUrlsAsync(
                ids,
                cancellationToken);

        return rows
            .Select(
                row =>
                    new CartProductReadModel(
                        row.ProductVariantId,
                        row.ProductId,
                        row.ProductName,
                        row.Sku,
                        row.PackageQuantity,
                        row.PackageUnit,
                        row.RequiresPrescription,
                        row.Mrp,
                        row.CurrentSellingPrice,
                        row.HasSellableStock,
                        imageUrls.GetValueOrDefault(
                            row.ProductVariantId)))
            .ToList();
    }

    private sealed class CartProductDbRow
    {
        public Guid ProductVariantId { get; init; }

        public Guid ProductId { get; init; }

        public string ProductName { get; init; } = null!;

        public string Sku { get; init; } = null!;

        public decimal PackageQuantity { get; init; }

        public string PackageUnit { get; init; } = null!;

        public bool RequiresPrescription { get; init; }

        public decimal? Mrp { get; init; }

        public decimal? CurrentSellingPrice { get; init; }

        public bool HasSellableStock { get; init; }
    }
}