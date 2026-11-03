"use client";

import {
    useMutation,
    useQueryClient,
} from "@tanstack/react-query";

import { setAccessToken } from "@/lib/auth/access-token";
import { queryKeys } from "@/lib/query/query-keys";

import { signIn } from "../api/auth.api";
import { useAuth } from "../context/auth-context";

export function useLogin() {
    const queryClient = useQueryClient();
    const { setAuthenticatedUser } = useAuth();

    return useMutation({
        mutationFn: signIn,

        onSuccess: (data) => {
            setAccessToken(data.accessToken);

            const user = {
                id: data.userId,
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                role: data.role,
            };

            queryClient.setQueryData(
                queryKeys.auth.me,
                user,
            );

            setAuthenticatedUser(user);
        },
    });
}