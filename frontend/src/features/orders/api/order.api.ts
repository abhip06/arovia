import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";
import type {
    CancelOrderRequest,
    ChangeOrderStatusRequest,
    CreateOrderRequest,
    CreateOrderResponse,
    GetAdminOrdersRequest,
    GetCustomerOrdersRequest,
    GetOrderDetailsRequest,
    OrderDetailsResponse,
    OrderMutationResponse,
    OrderSummaryResponse,
    PagedResultResponse,
} from "@/types/order.types";

function toMutationResponse(
    data: OrderMutationResponse | OrderDetailsResponse,
): OrderMutationResponse {
    // The order mutation endpoints return the updated order itself with HTTP 200.
    // Adapt that transport contract here so hooks have one stable response shape.
    if ("orderNumber" in data) {
        return { status: "Succeeded", order: data };
    }

    return data;
}

async function getAdminOrders(
    request: GetAdminOrdersRequest = {},
): Promise<PagedResultResponse<OrderSummaryResponse>> {
    try {
        const { data } = await apiClient.get<
            PagedResultResponse<OrderSummaryResponse>
        >("/v1/admin/orders", {
            params: request,
        });

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getCustomerOrders(
    request: GetCustomerOrdersRequest = {},
): Promise<PagedResultResponse<OrderSummaryResponse>> {
    try {
        const { data } = await apiClient.get<
            PagedResultResponse<OrderSummaryResponse>
        >("/v1/orders", {
            params: request,
        });

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getAdminOrderById(
    request: GetOrderDetailsRequest,
): Promise<OrderDetailsResponse> {
    try {
        const { data } = await apiClient.get<OrderDetailsResponse>(
            `/v1/admin/orders/${encodeURIComponent(request.orderId)}`,
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getCustomerOrderById(
    request: GetOrderDetailsRequest,
): Promise<OrderDetailsResponse> {
    try {
        const { data } = await apiClient.get<OrderDetailsResponse>(
            `/v1/orders/${encodeURIComponent(request.orderId)}`,
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function createOrder(
    request: CreateOrderRequest,
    idempotencyKey: string,
): Promise<CreateOrderResponse> {
    try {
        const { data } = await apiClient.post<CreateOrderResponse>(
            "/v1/orders",
            request,
            {
                headers: {
                    "Idempotency-Key": idempotencyKey,
                },
            },
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function cancelCustomerOrder(
    request: CancelOrderRequest,
): Promise<OrderMutationResponse> {
    try {
        const { data } = await apiClient.post<
            OrderMutationResponse | OrderDetailsResponse
        >(
            `/v1/orders/${encodeURIComponent(request.orderId)}/cancel`,
        );

        return toMutationResponse(data);
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function cancelAdminOrder(
    request: CancelOrderRequest,
): Promise<OrderMutationResponse> {
    try {
        const { data } = await apiClient.post<
            OrderMutationResponse | OrderDetailsResponse
        >(
            `/v1/admin/orders/${encodeURIComponent(request.orderId)}/cancel`,
        );

        return toMutationResponse(data);
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function changeOrderStatus(
    request: ChangeOrderStatusRequest,
): Promise<OrderMutationResponse> {
    try {
        const { orderId, status } = request;

        const { data } = await apiClient.post<
            OrderMutationResponse | OrderDetailsResponse
        >(
            `/v1/admin/orders/${encodeURIComponent(orderId)}/status`,
            { status },
        );

        return toMutationResponse(data);
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function downloadCustomerOrderReceipt(
    request: GetOrderDetailsRequest,
): Promise<Blob> {
    try {
        const { data } = await apiClient.get<Blob>(
            `/v1/orders/${encodeURIComponent(request.orderId)}/receipt`,
            {
                responseType: "blob",
            },
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export const ordersApi = {
    getAdminOrders,
    getCustomerOrders,
    getAdminOrderById,
    getCustomerOrderById,
    createOrder,
    cancelCustomerOrder,
    cancelAdminOrder,
    changeOrderStatus,
    downloadCustomerOrderReceipt,
};
