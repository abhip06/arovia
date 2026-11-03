"use client";

import {
    useMutation,
    useQueryClient,
} from "@tanstack/react-query";

import { clearAccessToken } from "@/lib/auth/access-token";

import {
    requestPasswordReset,
    resetPassword,
} from "../api/auth.api";
import { useAuth } from "../context/auth-context";

export function useRequestPasswordReset() {
    return useMutation({ mutationFn: requestPasswordReset });
}

export function useResetPassword() {
    const queryClient = useQueryClient();
    const { setUnauthenticated } = useAuth();

    return useMutation({
        mutationFn: resetPassword,
        onSuccess: () => {
            clearAccessToken();
            queryClient.clear();
            setUnauthenticated();
        },
    });
}
