"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query/query-keys";
import { paymentsApi } from "../api/payments.api";
import type {
    PaymentSessionResponse,
    VerifyRazorpayPaymentRequest,
    VerifyRazorpayPaymentResponse,
} from "@/types/payment.types";

interface CreatePaymentSessionVariables {
    orderId: string;
    idempotencyKey: string;
}

export function useCreatePaymentSession() {
    return useMutation<PaymentSessionResponse, Error, CreatePaymentSessionVariables>({
        mutationFn: ({ orderId, idempotencyKey }) =>
            paymentsApi.createPaymentSession(orderId, idempotencyKey),
    });
}

export function useVerifyRazorpayPayment() {
    const queryClient = useQueryClient();

    return useMutation<
        VerifyRazorpayPaymentResponse,
        Error,
        VerifyRazorpayPaymentRequest
    >({
        mutationFn: paymentsApi.verifyRazorpayPayment,
        onSuccess: (result) => {
            if (
                result.status !== "Captured" &&
                result.status !== "AlreadyCaptured"
            ) {
                return;
            }

            // Do not make the Checkout handler wait for background refetches;
            // it can navigate immediately after the server confirms capture.
            void Promise.all([
                queryClient.invalidateQueries({
                    queryKey: queryKeys.orders.all,
                }),
                queryClient.invalidateQueries({
                    queryKey: queryKeys.cart.all,
                }),
                queryClient.invalidateQueries({
                    queryKey: queryKeys.checkout.all,
                }),
            ]);

            if (result.order) {
                queryClient.setQueryData(
                    queryKeys.orders.customerDetail(result.order.id),
                    result.order,
                );
            }
        },
    });
}
