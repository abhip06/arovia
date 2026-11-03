import {
    useMutation,
    useQuery,
    useQueryClient,
} from "@tanstack/react-query";
import type { QueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query/query-keys";
import { currentLocationQueryKeys } from "@/features/location/use-current-location";

import {
    createAddress as createAddressRequest,
    deleteAddress as deleteAddressRequest,
    getAddresses,
    setPrimaryAddress as setPrimaryAddressRequest,
    updateAddress as updateAddressRequest,
} from "@/features/addresses/api/addresses.api";

import type {
    CreateAddressRequest,
    UpdateAddressRequest,
} from "@/types/address.types";
import { useLocationStore } from "@/store/location-store";

async function invalidateAddressData(
    queryClient: QueryClient,
) {
    await Promise.all([
        queryClient.invalidateQueries({
            queryKey: queryKeys.addresses.all,
        }),
        queryClient.invalidateQueries({
            queryKey: queryKeys.checkout.all,
        }),
        queryClient.invalidateQueries({
            queryKey: currentLocationQueryKeys.all,
        }),
    ]);

    useLocationStore
        .getState()
        .resumeDefaultLocationSync();
}

export function useAddresses(enabled = true) {
    return useQuery({
        queryKey: queryKeys.addresses.list,
        queryFn: ({ signal }) => getAddresses(signal),
        enabled,
        staleTime: 60_000,
    });
}

export function useCreateAddress() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (request: CreateAddressRequest) =>
            createAddressRequest(request),

        onSuccess: () =>
            invalidateAddressData(queryClient),
    });
}

export function useUpdateAddress() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            addressId,
            request,
        }: {
            addressId: string;
            request: UpdateAddressRequest;
        }) => updateAddressRequest(addressId, request),

        onSuccess: () =>
            invalidateAddressData(queryClient),
    });
}

export function useDeleteAddress() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (addressId: string) =>
            deleteAddressRequest(addressId),

        onSuccess: () =>
            invalidateAddressData(queryClient),
    });
}

export function useSetPrimaryAddress() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (addressId: string) =>
            setPrimaryAddressRequest(addressId),

        onSuccess: () =>
            invalidateAddressData(queryClient),
    });
}