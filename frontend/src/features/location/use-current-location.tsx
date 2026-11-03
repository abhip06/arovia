"use client";

import { useQuery } from "@tanstack/react-query";

import { locationApi } from "./location.api";

export const currentLocationQueryKeys = {
    all: ["location", "current"] as const,

    byIdentity: (identity: string) =>
        [...currentLocationQueryKeys.all, identity] as const,
};

export function useCurrentLocation(
    identity: string,
    enabled = true,
) {
    return useQuery({
        queryKey: currentLocationQueryKeys.byIdentity(identity),
        queryFn: locationApi.getCurrentLocation,
        enabled,
        staleTime: 5 * 60 * 1000,
        gcTime: 30 * 60 * 1000,
        refetchOnWindowFocus: false,
        retry: 1,
    });
}