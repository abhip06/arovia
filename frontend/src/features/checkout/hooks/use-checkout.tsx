import { useQuery } from "@tanstack/react-query";

import type { CheckoutData } from "@/types/checkout.types";
import { queryKeys } from "@/lib/query/query-keys";
import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

async function getCheckout(
    pincode: string | null,
    signal?: AbortSignal,
): Promise<CheckoutData> {
    try {
        const { data } = await apiClient.get<CheckoutData>(
            "/v1/me/checkout",
            {
                params: pincode ? { pincode } : undefined,
                signal,
            },
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export function useCheckout(
    pincode: string | null,
    enabled = true,
) {
    return useQuery({
        queryKey: queryKeys.checkout.current(pincode),
        queryFn: ({ signal }) =>
            getCheckout(pincode, signal),
        enabled,
        staleTime: 0,
    });
}