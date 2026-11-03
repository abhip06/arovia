"use client";

import { useEffect } from "react";

import { useLocationStore } from "@/store/location-store";

export function LocationStoreHydration() {
    const hasHydrated = useLocationStore(
        (state) => state.hasHydrated,
    );

    const setHasHydrated = useLocationStore(
        (state) => state.setHasHydrated,
    );

    useEffect(() => {
        if (useLocationStore.persist.hasHydrated()) {
            setHasHydrated(true);
            return;
        }

        let cancelled = false;

        void Promise.resolve(
            useLocationStore.persist.rehydrate(),
        )
            .catch(() => {
                // Continue with the default in-memory state if
                // persisted storage is unavailable or malformed.
            })
            .finally(() => {
                if (!cancelled) {
                    setHasHydrated(true);
                }
            });

        return () => {
            cancelled = true;
        };
    }, [setHasHydrated]);

    return null;
}