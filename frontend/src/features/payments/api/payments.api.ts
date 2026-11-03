import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";
import type {
    PaymentSessionResponse,
    VerifyRazorpayPaymentRequest,
    VerifyRazorpayPaymentResponse,
} from "@/types/payment.types";

async function createPaymentSession(
    orderId: string,
    idempotencyKey: string,
): Promise<PaymentSessionResponse> {
    try {
        const { data } = await apiClient.post<PaymentSessionResponse>(
            `/v1/orders/${encodeURIComponent(orderId)}/payment-session`,
            undefined,
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

async function verifyRazorpayPayment(
    request: VerifyRazorpayPaymentRequest,
): Promise<VerifyRazorpayPaymentResponse> {
    try {
        const { data } = await apiClient.post<VerifyRazorpayPaymentResponse>(
            "/v1/payments/verify",
            request,
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export const paymentsApi = {
    createPaymentSession,
    verifyRazorpayPayment,
};
