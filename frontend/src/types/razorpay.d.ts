export {};

declare global {
    interface RazorpayCheckoutResponse {
        razorpay_payment_id: string;
        razorpay_order_id: string;
        razorpay_signature: string;
    }

    interface RazorpayPaymentFailedResponse {
        error: {
            code?: string;
            description?: string;
            source?: string;
            step?: string;
            reason?: string;
            metadata?: {
                order_id?: string;
                payment_id?: string;
            };
        };
    }

    interface RazorpayCheckoutOptions {
        key: string;
        amount: number;
        currency: string;
        name: string;
        description?: string;
        order_id: string;
        handler: (response: RazorpayCheckoutResponse) => void | Promise<void>;
        prefill?: {
            name?: string;
            email?: string;
            contact?: string;
        };
        theme?: {
            color?: string;
        };
        modal?: {
            ondismiss?: () => void;
        };
    }

    interface RazorpayCheckoutInstance {
        open(): void;
        on(
            event: "payment.failed",
            handler: (response: RazorpayPaymentFailedResponse) => void,
        ): void;
    }

    interface Window {
        Razorpay?: new (
            options: RazorpayCheckoutOptions,
        ) => RazorpayCheckoutInstance;
    }
}
