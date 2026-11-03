"use client";

import { useState } from "react";

import {
    QueryClientProvider,
} from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";

import { createQueryClient } from "@/lib/query/query-client";
import { AuthProvider } from "@/features/auth/context/auth-context";
import { AuthBootstrap } from "@/features/auth/components/auth.bootstrap";
import { CurrentLocationSync } from "@/features/location/current-location-sync";
import { LocationStoreHydration } from "@/features/location/location-store-hydration";

interface ProvidersProps {
    children: React.ReactNode;
}

export function Providers({
    children,
}: ProvidersProps) {
    const [queryClient] = useState(
        createQueryClient,
    );

    return (
        <QueryClientProvider client={queryClient}>
            <LocationStoreHydration />

            <AuthProvider>
                <CurrentLocationSync />

                <AuthBootstrap>
                    {children}
                </AuthBootstrap>
            </AuthProvider>

            <ReactQueryDevtools initialIsOpen={false} />
        </QueryClientProvider>
    );
}
