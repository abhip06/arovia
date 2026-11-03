"use client";

import { useLocationStore } from "@/store/location-store";

export function useRequestPincode(): string | null {
    return useLocationStore((state) => {
        const { pincode, source } = state;

        if (source !== "User" || !pincode) {
            return null;
        }

        return /^\d{6}$/.test(pincode)
            ? pincode
            : null;
    });
}