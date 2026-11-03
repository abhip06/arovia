"use client";

import {
    useMutation,
    useQuery,
    useQueryClient,
} from "@tanstack/react-query";

import { queryKeys } from "@/lib/query/query-keys";
import { ordersApi } from "../api/order.api";
import type {
    CancelOrderRequest,
    ChangeOrderStatusRequest,
    CreateOrderRequest,
    CreateOrderResponse,
    DownloadCustomerOrderReceiptVariables,
    GetAdminOrdersRequest,
    GetCustomerOrdersRequest,
    OrderDetailsResponse,
    OrderMutationResponse,
    OrderSummaryResponse,
    PagedResultResponse,
} from "@/types/order.types";

function updateOrderListCaches(
    queryClient: ReturnType<typeof useQueryClient>,
    order: OrderDetailsResponse,
) {
    const listKeys = [
        queryKeys.orders.adminLists,
        queryKeys.orders.customerLists,
    ];

    for (const queryKey of listKeys) {
        queryClient.setQueriesData<PagedResultResponse<OrderSummaryResponse>>(
            { queryKey },
            (current) => {
                if (!current) return current;

                return {
                    ...current,
                    items: current.items.map((item) =>
                        item.id === order.id
                            ? { ...item, status: order.status }
                            : item,
                    ),
                };
            },
        );
    }
}

function refreshOrderQueries(queryClient: ReturnType<typeof useQueryClient>) {
    // Keep the confirmed server response visible immediately. A failed background
    // refresh must not turn a successful write into a mutation error for the user.
    void queryClient.invalidateQueries({ queryKey: queryKeys.orders.all });
}

export function useAdminOrders(
    request: GetAdminOrdersRequest = {},
    enabled = true,
) {
    return useQuery<PagedResultResponse<OrderSummaryResponse>>({
        queryKey: queryKeys.orders.adminList(request),
        queryFn: () => ordersApi.getAdminOrders(request),
        enabled,
        staleTime: 60_000,
    });
}

export function useCustomerOrders(
    request: GetCustomerOrdersRequest = {},
    enabled = true,
) {
    return useQuery<PagedResultResponse<OrderSummaryResponse>>({
        queryKey: queryKeys.orders.customerList(request),
        queryFn: () => ordersApi.getCustomerOrders(request),
        enabled,
        staleTime: 60_000,
    });
}

export function useAdminOrderDetails(
    orderId: string,
    enabled = true,
) {
    return useQuery<OrderDetailsResponse>({
        queryKey: queryKeys.orders.adminDetail(orderId),
        queryFn: () => ordersApi.getAdminOrderById({ orderId }),
        enabled: enabled && Boolean(orderId),
        staleTime: 30_000,
    });
}

export function useCustomerOrderDetails(
    orderId: string,
    enabled = true,
) {
    return useQuery<OrderDetailsResponse>({
        queryKey: queryKeys.orders.customerDetail(orderId),
        queryFn: () => ordersApi.getCustomerOrderById({ orderId }),
        enabled: enabled && Boolean(orderId),
        staleTime: 30_000,
    });
}

interface CreateOrderVariables {
    request: CreateOrderRequest;
    idempotencyKey: string;
}

interface CreateOrderHookOptions {
    /** Keep cart/checkout query data available while transitioning directly to payment. */
    deferCheckoutInvalidation?: boolean;
}

export function useCreateOrder(
    options: CreateOrderHookOptions = {},
) {
    const queryClient = useQueryClient();

    return useMutation<CreateOrderResponse, Error, CreateOrderVariables>({
        mutationFn: ({ request, idempotencyKey }) =>
            ordersApi.createOrder(request, idempotencyKey),
        onSuccess: (result) => {
            if (result.status !== "Created" && result.status !== "Replayed") {
                return;
            }

            void queryClient.invalidateQueries({
                queryKey: queryKeys.orders.all,
            });

            if (!options.deferCheckoutInvalidation) {
                void queryClient.invalidateQueries({
                    queryKey: queryKeys.cart.all,
                });
                void queryClient.invalidateQueries({
                    queryKey: queryKeys.checkout.all,
                });
            }

            if (result.order) {
                queryClient.setQueryData(
                    queryKeys.orders.customerDetail(result.order.id),
                    result.order,
                );
            }
        },
    });
}

export function useCancelCustomerOrder() {
    const queryClient = useQueryClient();

    return useMutation<OrderMutationResponse, Error, CancelOrderRequest>({
        mutationFn: (request) => ordersApi.cancelCustomerOrder(request),
        onSuccess: (result) => {
            if (
                result.status !== "Succeeded" &&
                result.status !== "AlreadyCancelled"
            ) {
                return;
            }

            if (result.order) {
                queryClient.setQueryData(
                    queryKeys.orders.customerDetail(result.order.id),
                    result.order,
                );
                updateOrderListCaches(queryClient, result.order);
            }

            refreshOrderQueries(queryClient);
        },
    });
}

export function useCancelAdminOrder() {
    const queryClient = useQueryClient();

    return useMutation<OrderMutationResponse, Error, CancelOrderRequest>({
        mutationFn: (request) => ordersApi.cancelAdminOrder(request),
        onSuccess: (result) => {
            if (
                result.status !== "Succeeded" &&
                result.status !== "AlreadyCancelled"
            ) {
                return;
            }

            if (result.order) {
                queryClient.setQueryData(
                    queryKeys.orders.adminDetail(result.order.id),
                    result.order,
                );
                updateOrderListCaches(queryClient, result.order);
            }

            refreshOrderQueries(queryClient);
        },
    });
}

export function useChangeOrderStatus() {
    const queryClient = useQueryClient();

    return useMutation<OrderMutationResponse, Error, ChangeOrderStatusRequest>({
        mutationFn: (request) => ordersApi.changeOrderStatus(request),
        onSuccess: (result) => {
            if (result.status !== "Succeeded") {
                return;
            }

            if (result.order) {
                queryClient.setQueryData(
                    queryKeys.orders.adminDetail(result.order.id),
                    result.order,
                );
                updateOrderListCaches(queryClient, result.order);
            }

            refreshOrderQueries(queryClient);
        },
    });
}

export function useDownloadCustomerOrderReceipt() {
    return useMutation<
        void,
        Error,
        DownloadCustomerOrderReceiptVariables
    >({
        mutationFn: async ({ orderId, orderNumber }) => {
            const blob =
                await ordersApi.downloadCustomerOrderReceipt({ orderId });

            const objectUrl = URL.createObjectURL(blob);
            const link = document.createElement("a");

            const safeOrderNumber = orderNumber.replace(
                /[^a-zA-Z0-9_-]/g,
                "-",
            );

            link.href = objectUrl;
            link.download = `Arovia-Receipt-${safeOrderNumber}.pdf`;

            document.body.appendChild(link);

            try {
                link.click();
            } finally {
                link.remove();
                window.setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);
            }
        },
    });
}
