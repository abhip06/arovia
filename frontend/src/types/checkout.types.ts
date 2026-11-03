export interface CheckoutItem {
    id: string;
    productVariantId: string;
    productId: string;
    productName: string;
    sku: string;
    packageQuantity: number;
    packageUnit: string;
    requiresPrescription: boolean;
    quantity: number;
    mrp: number | null;
    unitPrice: number | null;
    lineTotal: number | null;
    imageUrl: string | null;
    isAvailable: boolean;
}

export interface CheckoutData {
    id: string | null;
    items: CheckoutItem[];
    itemCount: number;
    subtotal: number;
    discountTotal: number;
    totalTax: number;
    shippingFee: number;
    grandTotal: number;
    hasUnavailableItems: boolean;
}