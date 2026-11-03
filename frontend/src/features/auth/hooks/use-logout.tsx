"use client";

import {
    useMutation,
    useQueryClient,
} from "@tanstack/react-query";

import { clearAccessToken } from "@/lib/auth/access-token";
import { revokeSession } from "@/lib/auth/session";

import { useAuth } from "../context/auth-context";

export function useLogout() {
    const queryClient = useQueryClient();
    const { setUnauthenticated } = useAuth();

    return useMutation({
        mutationFn: revokeSession,

        onSettled: () => {
            clearAccessToken();

            queryClient.clear();

            setUnauthenticated();
        },
    });
}