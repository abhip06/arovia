export type OrderStatus =
    | "Placed"
    | "Confirmed"
    | "Processing"
    | "Shipped"
    | "OutForDelivery"
    | "Delivered"
    | "Cancelled";

export type CreateOrderStatus =
    | "Created"
    | "Replayed"
    | "CartEmpty"
    | "ShippingAddressNotFound"
    | "RecipientPhoneRequired"
    | "InvalidShippingAddress"
    | "ItemsUnavailable"
    | "InsufficientStock"
    | "IdempotencyConflict";

export type OrderMutationStatus =
    | "Succeeded"
    | "AlreadyCancelled"
    | "NotFound"
    | "InvalidTransition"
    | "InvalidStatus"
    | "PrescriptionVerificationRequired";

export type ChangeOrderStatusTarget = Exclude<
    OrderStatus,
    "Placed" | "Cancelled"
>;

// Query parameters

export interface OrderListRequest {
    page?: number;
    pageSize?: number;
    status?: OrderStatus | null;
    createdFromUtc?: string | null;
    createdToUtc?: string | null;
    orderNumber?: string | null;
}

export type GetAdminOrdersRequest = OrderListRequest;

export type GetCustomerOrdersRequest = OrderListRequest;

// Route parameters

export interface GetOrderDetailsRequest {
    orderId: string;
}

export interface CancelOrderRequest {
    orderId: string;
}

export interface ChangeOrderStatusRequest {
    orderId: string;
    status: ChangeOrderStatusTarget;
}

// Create order

export interface CreateOrderRequest {
    shippingAddressId: string;
}

// Address snapshot saved with an order

export interface OrderAddressResponse {
    recipientName: string;
    recipientPhone: string;
    addressLine1: string;
    addressLine2: string | null;
    city: string;
    state: string;
    postalCode: string;
    country: string;
}

// Order summary

export interface OrderSummaryResponse {
    id: string;
    orderNumber: string;
    userId: string;
    status: OrderStatus;
    currency: string;
    grandTotal: number;
    createdAtUtc: string;
    itemCount: number;
    totalQuantity: number;
}

// Individual order item

export interface OrderItemDetailsResponse {
    id: string;
    productVariantId: string;
    productName: string;
    sku: string;
    imageUrl: string | null;
    packageQuantity: number;
    packageUnit: string;
    quantity: number;
    unitMrp: number;
    unitSellingPrice: number;
    lineTaxAmount: number;
    lineDiscountAmount: number;
    lineSubtotal: number;
    lineTotal: number;
    requiresPrescription: boolean;
}

// Complete order details

export interface OrderDetailsResponse {
    id: string;
    orderNumber: string;
    userId: string;
    status: OrderStatus;
    currency: string;
    subtotal: number;
    discountTotal: number;
    taxTotal: number;
    shippingFee: number;
    grandTotal: number;
    shippingAddress: OrderAddressResponse;
    createdAtUtc: string;
    updatedAtUtc: string;
    items: OrderItemDetailsResponse[];
}

// Paginated order list

export interface PagedResultResponse<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

// Create order result

export interface CreateOrderResponse {
    status: CreateOrderStatus;
    order?: OrderDetailsResponse | null;
}

// Cancel/status mutation result

export interface OrderMutationResponse {
    status: OrderMutationStatus;
    order?: OrderDetailsResponse | null;
}

export interface DownloadCustomerOrderReceiptVariables {
    orderId: string;
    orderNumber: string;
}