import { GetAdminOrdersRequest, GetCustomerOrdersRequest } from "@/types/order.types";

export const queryKeys = {
    auth: {
        me: ["auth", "me"] as const,
    },

    cart: {
        all: ["cart"] as const,
        current: ["cart", "current"] as const,
    },

    checkout: {
        all: ["checkout"] as const,
        current: (pincode: string | null) =>
            ["checkout", "current", pincode] as const,
    },

    medicines: {
        all: ["medicines"] as const,
        list: (params: unknown) =>
            ["medicines", "list", params] as const,
        detail: (id: string) =>
            ["medicines", "detail", id] as const,
    },

    catalog: {
        all: ["catalog"] as const,
        products: (params: unknown) =>
            ["catalog", "products", params] as const,
        productDetail: (slug: string, sku: string) =>
            ["catalog", "product-detail", slug, sku] as const,
        brands: ["catalog", "brands"] as const,
        manufacturers: ["catalog", "manufacturers"] as const,
        categories: ["catalog", "categories"] as const,
        priceHistory: (variantId: string) => ["catalog", "price-history", variantId] as const,
    },
    orders: {
        all: ["orders"] as const,

        adminLists: ["orders", "admin", "list"] as const,
        adminList: (request: GetAdminOrdersRequest) =>
            ["orders", "admin", "list", request] as const,

        customerLists: ["orders", "customer", "list"] as const,
        customerList: (request: GetCustomerOrdersRequest) =>
            ["orders", "customer", "list", request] as const,

        adminDetails: ["orders", "admin", "detail"] as const,
        adminDetail: (orderId: string) =>
            ["orders", "admin", "detail", orderId] as const,

        customerDetails: ["orders", "customer", "detail"] as const,
        customerDetail: (orderId: string) =>
            ["orders", "customer", "detail", orderId] as const,
    },

    adminUsers: {
        all: ["admin", "users"] as const,
    },

    adminPayments: {
        all: ["admin", "payments"] as const,
        page: (page: number) => ["admin", "payments", page] as const,
    },

    addresses: {
        all: ["addresses"] as const,
        list: ["addresses", "list"] as const,
    },

    operations: {
        warehouses: (page: number) => ["operations", "warehouses", page] as const,
        batches: (page: number) => ["operations", "batches", page] as const,
        stock: (warehouseId: string, page: number) => ["operations", "stock", warehouseId, page] as const,
        movements: (stockId: string, page: number) => ["operations", "movements", stockId, page] as const,
    },
} as const;
