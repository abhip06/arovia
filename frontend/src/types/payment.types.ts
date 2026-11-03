import type { OrderDetailsResponse } from "@/types/order.types";

export interface PaymentSessionResponse {
    paymentId: string;
    paymentAttemptId: string;
    keyId: string;
    providerOrderId: string;
    amountSubunits: number;
    currency: string;
    expiresAtUtc: string;
}

export interface VerifyRazorpayPaymentRequest {
    paymentAttemptId: string;
    providerOrderId: string;
    providerPaymentId: string;
    signature: string;
}

export type VerifyRazorpayPaymentStatus =
    | "Captured"
    | "AlreadyCaptured"
    | "PaymentPending"
    | "InvalidSignature"
    | "PaymentNotFound"
    | "OrderNotFound"
    | "InvalidPayment"
    | "OrderNotPayable"
    | "ReconciliationRequired"
    | "PaymentFailed";

export interface VerifyRazorpayPaymentResponse {
    status: VerifyRazorpayPaymentStatus;
    order: OrderDetailsResponse | null;
}
