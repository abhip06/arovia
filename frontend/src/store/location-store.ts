"use client";

import { create } from "zustand";
import {
    createJSONStorage,
    persist,
} from "zustand/middleware";

export type PincodeSource = "User" | "Ip";

export interface LocationDetails {
    city?: string | null;
    state?: string | null;
    country?: string | null;
}

interface LocationState {
    pincode: string | null;
    source: PincodeSource | null;
    city: string | null;
    state: string | null;
    country: string | null;

    // True when the customer explicitly selected a location.
    isManuallySelected: boolean;

    // Identifies the auth context in which the default was resolved.
    resolvedForIdentity: string | null;

    // Runtime-only hydration state.
    hasHydrated: boolean;

    setUserLocation: (
        pincode: string,
        details?: LocationDetails,
    ) => void;

    setIpLocation: (
        pincode: string,
        details?: LocationDetails,
    ) => void;

    setResolvedUserLocation: (
        pincode: string,
        identity: string,
        details?: LocationDetails,
    ) => void;

    setResolvedIpLocation: (
        pincode: string,
        identity: string,
        details?: LocationDetails,
    ) => void;

    clearLocation: () => void;

    // Allows the backend-resolved default location to take over again.
    resumeDefaultLocationSync: () => void;

    setHasHydrated: (hydrated: boolean) => void;
}

type PersistedLocationState = Pick<
    LocationState,
    | "pincode"
    | "source"
    | "city"
    | "state"
    | "country"
    | "isManuallySelected"
    | "resolvedForIdentity"
>;

function normalizePincode(
    pincode: string,
): string | null {
    const normalized = pincode.trim();

    return /^\d{6}$/.test(normalized)
        ? normalized
        : null;
}

function normalizeText(
    value?: string | null,
): string | null {
    return value?.trim() || null;
}

function createLocationValues(
    pincode: string,
    source: PincodeSource,
    details: LocationDetails | undefined,
    isManuallySelected: boolean,
    identity: string | null,
) {
    const normalized = normalizePincode(pincode);

    return {
        pincode: normalized,
        source: normalized ? source : null,
        city: normalized ? normalizeText(details?.city) : null,
        state: normalized ? normalizeText(details?.state) : null,
        country: normalized ? normalizeText(details?.country) : null,
        isManuallySelected: normalized
            ? isManuallySelected
            : false,
        resolvedForIdentity: normalized ? identity : null,
    };
}

export const useLocationStore = create<LocationState>()(
    persist<LocationState, [], [], PersistedLocationState>(
        (set) => ({
            pincode: null,
            source: null,
            city: null,
            state: null,
            country: null,
            isManuallySelected: false,
            resolvedForIdentity: null,
            hasHydrated: false,

            setUserLocation: (pincode, details) => {
                set(
                    createLocationValues(
                        pincode,
                        "User",
                        details,
                        true,
                        null,
                    ),
                );
            },

            setIpLocation: (pincode, details) => {
                set(
                    createLocationValues(
                        pincode,
                        "Ip",
                        details,
                        false,
                        null,
                    ),
                );
            },

            setResolvedUserLocation: (
                pincode,
                identity,
                details,
            ) => {
                set((state) => {
                    const next = createLocationValues(
                        pincode,
                        "User",
                        details,
                        false,
                        identity,
                    );

                    if (
                        state.pincode === next.pincode &&
                        state.source === next.source &&
                        state.city === next.city &&
                        state.state === next.state &&
                        state.country === next.country &&
                        state.resolvedForIdentity === identity &&
                        !state.isManuallySelected
                    ) {
                        return state;
                    }

                    return next;
                });
            },

            setResolvedIpLocation: (
                pincode,
                identity,
                details,
            ) => {
                set((state) => {
                    const next = createLocationValues(
                        pincode,
                        "Ip",
                        details,
                        false,
                        identity,
                    );

                    if (
                        state.pincode === next.pincode &&
                        state.source === next.source &&
                        state.city === next.city &&
                        state.state === next.state &&
                        state.country === next.country &&
                        state.resolvedForIdentity === identity &&
                        !state.isManuallySelected
                    ) {
                        return state;
                    }

                    return next;
                });
            },

            clearLocation: () => {
                set({
                    pincode: null,
                    source: null,
                    city: null,
                    state: null,
                    country: null,
                    isManuallySelected: false,
                    resolvedForIdentity: null,
                });
            },

            resumeDefaultLocationSync: () => {
                set({
                    isManuallySelected: false,
                    resolvedForIdentity: null,
                });
            },

            setHasHydrated: (hasHydrated) => {
                set({ hasHydrated });
            },
        }),
        {
            name: "arovia-location",
            storage: createJSONStorage(() => localStorage),
            skipHydration: true,
            version: 2,

            partialize: (state) => ({
                pincode: state.pincode,
                source: state.source,
                city: state.city,
                state: state.state,
                country: state.country,
                isManuallySelected: state.isManuallySelected,
                resolvedForIdentity: state.resolvedForIdentity,
            }),

            migrate: (persistedState) => {
                const old =
                    persistedState as Partial<PersistedLocationState>;

                return {
                    pincode: old.pincode ?? null,
                    source: old.source ?? null,
                    city: old.city ?? null,
                    state: old.state ?? null,
                    country: old.country ?? null,

                    // Legacy data did not distinguish manual selections
                    // from backend-resolved saved addresses. Re-resolve
                    // the default safely on the next startup.
                    isManuallySelected: false,
                    resolvedForIdentity: null,
                };
            },

            onRehydrateStorage: () => (state) => {
                state?.setHasHydrated(true);
            },
        },
    ),
);