"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query/query-keys";
import type { AddCartItemRequest, CartResponse, SetCartItemQuantityRequest } from "@/types/cart.types";
import { cartApi } from "../api/cart.api";

export function useCart(enabled: boolean) {
    return useQuery({
        queryKey: queryKeys.cart.current,
        queryFn: cartApi.getCart,
        enabled,
        staleTime: 15_000,
    });
}

function cacheCart(queryClient: ReturnType<typeof useQueryClient>, cart: CartResponse) {
    queryClient.setQueryData(queryKeys.cart.current, cart);
}

export function useAddProductToCart() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ itemId, quantity }: { itemId: string; quantity: number }) => {
            const request: AddCartItemRequest = {
                productVariantId: itemId,
                quantity,
            };

            return cartApi.addItem(request, crypto.randomUUID());
        },
        onSuccess: (cart) => cacheCart(queryClient, cart),
    });
}

export function useSetCartItemQuantity() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            cartItemId,
            quantity,
        }: SetCartItemQuantityRequest & { cartItemId: string }) =>
            cartApi.setItemQuantity(
                cartItemId,
                { quantity },
                crypto.randomUUID(),
            ),
        onSuccess: async () => {
            await Promise.all([
                queryClient.invalidateQueries({
                    queryKey: queryKeys.cart.all,
                }),
                queryClient.invalidateQueries({
                    queryKey: queryKeys.checkout.all,
                }),
            ]);
        },
    });
}

export function useRemoveCartItem() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (cartItemId: string) =>
            cartApi.removeItem(cartItemId, crypto.randomUUID()),
        onSuccess: async () => {
            await Promise.all([
                queryClient.invalidateQueries({
                    queryKey: queryKeys.cart.all,
                }),
                queryClient.invalidateQueries({
                    queryKey: queryKeys.checkout.all,
                }),
            ]);
        },
    });
}
