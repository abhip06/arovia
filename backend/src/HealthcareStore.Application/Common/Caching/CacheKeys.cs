namespace HealthcareStore.Application.Common.Caching;

public static class CacheKeys
{
    private const string Version = "v1";

    public static string Users
        => $"{Version}:users";

    public static string Brand(Guid brandId)
        => $"{Version}:brand:{brandId:N}";

    public static string Brands
        => $"{Version}:brands";

    public static string Manufacturer(Guid manufacturerId)
        => $"{Version}:manufacturer:{manufacturerId:N}";

    public static string Manufacturers
        => $"{Version}:manufacturers";

    public static string Category(Guid categoryId)
        => $"{Version}:category:{categoryId:N}";

    public static string Categories
        => $"{Version}:categories";

    public static string ActiveIngredient(Guid activeIngredientId)
        => $"{Version}:active-ingredient:{activeIngredientId:N}";

    public static string ActiveIngredients
        => $"{Version}:active-ingredients";

    public static string Product(Guid productId)
        => $"{Version}:product:{productId:N}";

    public static string Products(string fingerprint)
        => $"{Version}:products:{fingerprint}";

    public static string ProductVariant(Guid variantId)
        => $"{Version}:product-variant:{variantId:N}";

    public static string ProductVariants(
        Guid productId,
        string fingerprint)
        => $"{Version}:product:{productId:N}:variants:{fingerprint}";

    public static string ProductPriceHistory(Guid variantId)
        => $"{Version}:product-variant:{variantId:N}:prices";

    public static string Warehouse(Guid warehouseId)
        => $"{Version}:warehouse:{warehouseId:N}";

    public static string Warehouses(string fingerprint)
        => $"{Version}:warehouses:{fingerprint}";

    public static string WarehouseStocks(
        Guid warehouseId,
        string fingerprint)
        => $"{Version}:warehouse:{warehouseId:N}:stocks:{fingerprint}";

    public static string InventoryBatch(Guid batchId)
        => $"{Version}:inventory-batch:{batchId:N}";

    public static string InventoryBatches(string fingerprint)
        => $"{Version}:inventory-batches:{fingerprint}";

    public static string InventoryBatches(
        string sku,
        string fingerprint)
        => $"{Version}:product-variant:{sku}:batches:{fingerprint}";

    public static string InventoryMovementHistory(
        Guid warehouseStockId,
        string fingerprint)
        => $"{Version}:warehouse-stock:{warehouseStockId:N}:movements:{fingerprint}";

    public static string WarehouseServiceability(
        string pincode)
        => $"{Version}:warehouse-serviceability:{pincode}";

    public static string Cart(Guid userId)
        => $"{Version}:cart:{userId:N}";

    public static string AdminOrders(string fingerprint)
        => $"{Version}:admin:orders:{fingerprint}";

    public static string UserOrders(
        Guid userId,
        string fingerprint)
        => $"{Version}:user:{userId:N}:orders:{fingerprint}";

    public static string Payments(string fingerprint)
        => $"{Version}:payments:{fingerprint}";
}