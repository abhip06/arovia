using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Checkout;
using HealthcareStore.Domain.Inventory;
using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class CheckoutProductReader(
    ApplicationDbContext dbContext,
    IProductVariantMediaRepository variantMediaRepository)
    : ICheckoutProductReader
{
    public async Task<CheckoutData> GetAsync(
        Guid userId,
        string pincode,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var today =
            DateOnly.FromDateTime(
                now.UtcDateTime);

        var rows =
            await (
                from cartItem in
                    dbContext.CartItems
                        .AsNoTracking()

                join cart in
                    dbContext.Carts
                        .AsNoTracking()
                    on cartItem.CartId
                    equals cart.Id

                join variant in
                    dbContext.ProductVariants
                        .AsNoTracking()
                    on cartItem.ProductVariantId
                    equals variant.Id

                join product in
                    dbContext.Products
                        .AsNoTracking()
                    on variant.ProductId
                    equals product.Id

                where
                    cart.UserId == userId

                orderby
                    cartItem.CreatedAt,
                    cartItem.Id

                select new CheckoutSqlRow
                {
                    CartId =
                        cart.Id,

                    CartItemId =
                        cartItem.Id,

                    ProductVariantId =
                        variant.Id,

                    ProductId =
                        product.Id,

                    ProductName =
                        product.Name,

                    Sku =
                        variant.Sku,

                    PackageQuantity =
                        variant.PackageQuantity,

                    PackageUnit =
                        variant.PackageUnit,

                    RequiresPrescription =
                        product.RequiresPrescription,

                    Quantity =
                        cartItem.Quantity,

                    ProductStatus =
                        product.Status,

                    VariantStatus =
                        variant.Status,

                    TaxRate =
                        product.TaxRate,

                    SellingPrice =
                        (
                            from price in
                                dbContext.ProductPrices
                                    .AsNoTracking()

                            where
                                price.ProductVariantId ==
                                variant.Id &&

                                price.EffectiveFrom <=
                                now

                            orderby
                                price.EffectiveFrom descending

                            select
                                (decimal?)price.SellingPrice
                        ).FirstOrDefault(),

                    Mrp =
                        (
                            from batch in
                                dbContext.InventoryBatches
                                    .AsNoTracking()

                            where
                                batch.ProductVariantId ==
                                variant.Id &&

                                batch.Status ==
                                InventoryBatchStatus.Active &&

                                (
                                    batch.SellableUntil ==
                                        null ||
                                    batch.SellableUntil >=
                                        today
                                ) &&

                                (
                                    batch.ExpiryDate ==
                                        null ||
                                    batch.ExpiryDate >=
                                        today
                                ) &&

                                dbContext.WarehouseStocks
                                    .AsNoTracking()
                                    .Any(
                                        stock =>
                                            stock.InventoryBatchId ==
                                            batch.Id &&

                                            stock.QuantityOnHand >
                                            stock.QuantityReserved &&

                                            dbContext.WarehouseServiceablePincodes
                                                .AsNoTracking()
                                                .Any(
                                                    serviceable =>
                                                        serviceable.WarehouseId ==
                                                        stock.WarehouseId &&

                                                        serviceable.Pincode ==
                                                        pincode))

                            orderby
                                batch.SellableUntil == null,
                                batch.SellableUntil,
                                batch.ExpiryDate

                            select
                                (decimal?)batch.Mrp
                        ).FirstOrDefault(),

                    AvailableQuantity =
                        (
                            from stock in
                                dbContext.WarehouseStocks
                                    .AsNoTracking()

                            join batch in
                                dbContext.InventoryBatches
                                    .AsNoTracking()
                                on stock.InventoryBatchId
                                equals batch.Id

                            where
                                batch.ProductVariantId ==
                                variant.Id &&

                                batch.Status ==
                                InventoryBatchStatus.Active &&

                                (
                                    batch.SellableUntil ==
                                        null ||
                                    batch.SellableUntil >=
                                        today
                                ) &&

                                (
                                    batch.ExpiryDate ==
                                        null ||
                                    batch.ExpiryDate >=
                                        today
                                ) &&

                                stock.QuantityOnHand >
                                stock.QuantityReserved &&

                                dbContext.WarehouseServiceablePincodes
                                    .AsNoTracking()
                                    .Any(
                                        serviceable =>
                                            serviceable.WarehouseId ==
                                            stock.WarehouseId &&

                                            serviceable.Pincode ==
                                            pincode)

                            select
                                (int?)(
                                    stock.QuantityOnHand -
                                    stock.QuantityReserved)
                        ).Sum() ?? 0
                }
            )
            .ToListAsync(
                cancellationToken);

        if (rows.Count == 0)
        {
            return new CheckoutData(
                CartId: null,
                Items: []);
        }

        var variantIds =
            rows
                .Select(
                    static row =>
                        row.ProductVariantId)
                .Distinct()
                .ToArray();

        var imageUrls =
            await variantMediaRepository
                .GetPrimaryUrlsAsync(
                    variantIds,
                    cancellationToken);

        var items =
            new List<CheckoutItemData>(
                rows.Count);

        foreach (var row in rows)
        {
            imageUrls.TryGetValue(
                row.ProductVariantId,
                out var imageUrl);

            items.Add(
                new CheckoutItemData(
                    CartItemId:
                        row.CartItemId,

                    ProductVariantId:
                        row.ProductVariantId,

                    ProductId:
                        row.ProductId,

                    ProductName:
                        row.ProductName,

                    Sku:
                        row.Sku,

                    PackageQuantity:
                        row.PackageQuantity,

                    PackageUnit:
                        row.PackageUnit,

                    RequiresPrescription:
                        row.RequiresPrescription,

                    Quantity:
                        row.Quantity,

                    Mrp:
                        row.Mrp ?? 0m,

                    SellingPrice:
                        row.SellingPrice,

                    TaxRate:
                        row.TaxRate,

                    AvailableQuantity:
                        row.AvailableQuantity,

                    ImageUrl:
                        imageUrl,

                    IsProductActive:
                        row.ProductStatus ==
                        ProductStatus.Active,

                    IsVariantActive:
                        row.VariantStatus ==
                        ProductVariantStatus.Active));
        }

        return new CheckoutData(
            CartId:
                rows[0].CartId,
            Items:
                items);
    }

    private sealed class CheckoutSqlRow
    {
        public Guid CartId { get; init; }

        public Guid CartItemId { get; init; }

        public Guid ProductVariantId { get; init; }

        public Guid ProductId { get; init; }

        public string ProductName { get; init; } = null!;

        public string Sku { get; init; } = null!;

        public decimal PackageQuantity { get; init; }

        public string PackageUnit { get; init; } = null!;

        public bool RequiresPrescription { get; init; }

        public int Quantity { get; init; }

        public ProductStatus ProductStatus { get; init; }

        public ProductVariantStatus VariantStatus { get; init; }

        public decimal TaxRate { get; init; }

        public decimal? SellingPrice { get; init; }

        public decimal? Mrp { get; init; }

        public int AvailableQuantity { get; init; }
    }
}