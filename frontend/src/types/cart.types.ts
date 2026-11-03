export interface CartLine {
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

export interface CartResponse {
    id: string | null;
    items: CartLine[];
    itemCount: number;
    subtotal: number;
    hasUnavailableItems: boolean;
}

export interface AddCartItemRequest {
    productVariantId: string;
    quantity: number;
}

export interface SetCartItemQuantityRequest {
    quantity: number;
}
