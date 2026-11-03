import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";
import type {
    AddCartItemRequest,
    CartResponse,
    SetCartItemQuantityRequest,
} from "@/types/cart.types";

async function getCart(): Promise<CartResponse> {
    try {
        const { data } = await apiClient.get<CartResponse>("/v1/me/cart");
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function addItem(
    request: AddCartItemRequest,
    idempotencyKey: string,
): Promise<CartResponse> {
    try {
        const { data } = await apiClient.post<CartResponse>(
            "/v1/me/cart/items",
            request,
            { headers: { "Idempotency-Key": idempotencyKey } },
        );
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function setItemQuantity(
    cartItemId: string,
    request: SetCartItemQuantityRequest,
    idempotencyKey: string,
): Promise<CartResponse> {
    try {
        const { data } = await apiClient.put<CartResponse>(
            `/v1/me/cart/items/${encodeURIComponent(cartItemId)}`,
            request,
            { headers: { "Idempotency-Key": idempotencyKey } },
        );
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function removeItem(
    cartItemId: string,
    idempotencyKey: string,
): Promise<CartResponse> {
    try {
        const { data } = await apiClient.delete<CartResponse>(
            `/v1/me/cart/items/${encodeURIComponent(cartItemId)}`,
            { headers: { "Idempotency-Key": idempotencyKey } },
        );
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export const cartApi = { getCart, addItem, setItemQuantity, removeItem };
