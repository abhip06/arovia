"use client";

import {
    useMutation,
    useQueryClient,
} from "@tanstack/react-query";
import { useRef } from "react";

import { refreshAccessToken } from "@/lib/auth/session";
import { queryKeys } from "@/lib/query/query-keys";

import { getGoogleLoginUrl } from "../api/auth.api";
import { getCurrentUser } from "../api/user.api";
import { useAuth } from "../context/auth-context";

const sleep = (duration: number) =>
    new Promise<void>((resolve) => window.setTimeout(resolve, duration));

export function useGoogleLogin() {
    const queryClient = useQueryClient();
    const { setAuthenticatedUser } = useAuth();
    const popupRef = useRef<Window | null>(null);

    const mutation = useMutation({
        mutationFn: async () => {
            const popup = popupRef.current;

            if (!popup) {
                throw new Error(
                    "Allow pop-ups to continue with Google.",
                );
            }

            const expiresAt = Date.now() + 90_000;

            try {
                while (Date.now() < expiresAt) {
                    if (popup.closed) {
                        throw new Error(
                            "Google sign-in was cancelled.",
                        );
                    }

                    try {
                        await refreshAccessToken();
                        return await getCurrentUser();
                    } catch {
                        await sleep(1_000);
                    }
                }

                throw new Error(
                    "Google sign-in timed out. Please try again.",
                );
            } finally {
                if (!popup.closed) {
                    popup.close();
                }
                popupRef.current = null;
            }
        },
        onSuccess: (user) => {
            queryClient.setQueryData(queryKeys.auth.me, user);
            setAuthenticatedUser(user);
        },
    });

    const startGoogleLogin = (
        options?: Parameters<typeof mutation.mutate>[1],
    ) => {
        if (mutation.isPending) {
            return;
        }

        const popup = window.open(
            getGoogleLoginUrl(),
            "arovia-google-sign-in",
            "popup=yes,width=520,height=680,resizable=yes,scrollbars=yes",
        );

        if (popup) {
            popup.opener = null;
        }

        popupRef.current = popup;
        mutation.mutate(undefined, options);
    };

    return { ...mutation, startGoogleLogin };
}
