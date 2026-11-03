import type {
    Address,
    CreateAddressRequest,
    UpdateAddressRequest,
} from "@/types/address.types";

import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

const ADDRESSES_URL = "/v1/me/addresses";

export async function getAddresses(
    signal?: AbortSignal,
): Promise<Address[]> {
    try {
        const { data } = await apiClient.get<Address[]>(
            ADDRESSES_URL,
            { signal },
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function createAddress(
    request: CreateAddressRequest,
): Promise<Address> {
    try {
        const { data } = await apiClient.post<Address>(
            ADDRESSES_URL,
            request,
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function updateAddress(
    addressId: string,
    request: UpdateAddressRequest,
): Promise<Address> {
    try {
        const { data } = await apiClient.put<Address>(
            `${ADDRESSES_URL}/${encodeURIComponent(addressId)}`,
            request,
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function deleteAddress(
    addressId: string,
): Promise<void> {
    try {
        await apiClient.delete<void>(
            `${ADDRESSES_URL}/${encodeURIComponent(addressId)}`,
        );
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function setPrimaryAddress(
    addressId: string,
): Promise<void> {
    try {
        await apiClient.put<void>(
            `${ADDRESSES_URL}/${encodeURIComponent(addressId)}/primary`,
        );
    } catch (error) {
        throw normalizeApiError(error);
    }
}