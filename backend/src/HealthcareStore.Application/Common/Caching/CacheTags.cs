namespace HealthcareStore.Application.Common.Caching;

public static class CacheTags
{
    public const string Users = "users";
    public const string Brands = "brands";
    public const string Manufacturers = "manufacturers";
    public const string Categories = "categories";
    public const string ActiveIngredients = "active-ingredients";

    public const string Products = "products";
    public const string Variants = "variants";
    public const string Warehouses = "warehouses";
    public const string Batches = "batches";
    public const string WarehouseStocks = "warehouse-stocks";
    public const string WarehouseServiceability = "warehouse-serviceability";
    public const string ProductCatalog = "product-catalog";
    public const string Cart = "cart";
    public const string AdminOrders = "admin-orders";
    public const string Payments = "payments";

    public static string Product(Guid productId)
        => $"product:{productId:N}";

    public static string ProductVariant(Guid variantId)
        => $"product-variant:{variantId:N}";

    public static string Warehouse(Guid warehouseId)
        => $"warehouse:{warehouseId:N}";

    public static string WarehouseStocksForWarehouse(
        Guid warehouseId)
        => $"warehouse-stocks:{warehouseId:N}";

    public static string InventoryBatch(Guid batchId)
        => $"inventory-batch:{batchId:N}";

    public static string InventoryMovementHistory(
        Guid warehouseStockId)
        => $"inventory-movement-history:{warehouseStockId:N}";

    public static string WarehouseServiceabilityForWarehouse(
        Guid warehouseId)
        => $"warehouse-serviceability:{warehouseId:N}";

    public static string UserOrders(Guid userId)
        => $"user-orders:{userId:N}";
}