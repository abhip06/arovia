"use client";

import { useEffect } from "react";

import { useAuth } from "@/features/auth/context/auth-context";
import { useCurrentLocation } from "./use-current-location";
import { useLocationStore } from "@/store/location-store";

export function CurrentLocationSync() {
    const {
        authStatus,
        isLoading,
        user,
    } = useAuth();

    const hasHydrated = useLocationStore(
        (state) => state.hasHydrated,
    );

    const pincode = useLocationStore(
        (state) => state.pincode,
    );

    const source = useLocationStore(
        (state) => state.source,
    );

    const city = useLocationStore(
        (state) => state.city,
    );

    const state = useLocationStore(
        (state) => state.state,
    );

    const country = useLocationStore(
        (state) => state.country,
    );

    const isManuallySelected = useLocationStore(
        (state) => state.isManuallySelected,
    );

    const resolvedForIdentity = useLocationStore(
        (state) => state.resolvedForIdentity,
    );

    const clearLocation = useLocationStore(
        (state) => state.clearLocation,
    );

    const setResolvedUserLocation = useLocationStore(
        (state) => state.setResolvedUserLocation,
    );

    const setResolvedIpLocation = useLocationStore(
        (state) => state.setResolvedIpLocation,
    );

    const isAuthenticated =
        !isLoading && Boolean(authStatus && user);

    const userId = isAuthenticated
        ? user!.id
        : null;

    const identity = userId ?? "anonymous";

    const locationQuery = useCurrentLocation(
        identity,
        hasHydrated && !isLoading,
    );

    // Never display a backend-resolved location belonging to a
    // previous authenticated identity.
    useEffect(() => {
        if (
            !hasHydrated ||
            isLoading ||
            isManuallySelected ||
            resolvedForIdentity === identity
        ) {
            return;
        }

        if (pincode !== null || source !== null) {
            clearLocation();
        }
    }, [
        hasHydrated,
        isLoading,
        isManuallySelected,
        resolvedForIdentity,
        identity,
        pincode,
        source,
        clearLocation,
    ]);

    useEffect(() => {
        if (
            !hasHydrated ||
            isLoading ||
            !locationQuery.isSuccess ||
            isManuallySelected
        ) {
            return;
        }

        const location = locationQuery.data;

        if (location === null) {
            if (pincode !== null || source !== null) {
                clearLocation();
            }

            return;
        }

        const details = {
            city: location.city,
            state: location.state,
            country: location.country,
        };

        if (location.source === "user") {
            setResolvedUserLocation(
                location.pincode,
                identity,
                details,
            );
        } else {
            setResolvedIpLocation(
                location.pincode,
                identity,
                details,
            );
        }
    }, [
        hasHydrated,
        isLoading,
        locationQuery.isSuccess,
        locationQuery.data,
        isManuallySelected,
        identity,
        pincode,
        source,
        city,
        state,
        country,
        clearLocation,
        setResolvedUserLocation,
        setResolvedIpLocation,
    ]);

    return null;
}