"use client";

import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";

import { clearAccessToken } from "@/lib/auth/access-token";
import { refreshAccessToken } from "@/lib/auth/session";
import { queryKeys } from "@/lib/query/query-keys";

import { getCurrentUser } from "../api/user.api";
import { useAuth } from "../context/auth-context";

interface AuthBootstrapProps {
    children: React.ReactNode;
}

export function AuthBootstrap({
    children,
}: AuthBootstrapProps) {
    const queryClient = useQueryClient();

    const {
        setAuthenticatedUser,
        setUnauthenticated,
    } = useAuth();

    useEffect(() => {
        let cancelled = false;

        const handleSessionExpired = () => {
            clearAccessToken();
            queryClient.clear();
            setUnauthenticated();
        };

        window.addEventListener(
            "auth:session-expired",
            handleSessionExpired,
        );

        async function initializeAuth() {
            try {
                await refreshAccessToken();

                const user = await getCurrentUser();

                if (cancelled) {
                    return;
                }

                queryClient.setQueryData(
                    queryKeys.auth.me,
                    user,
                );

                setAuthenticatedUser(user);
            } catch {
                if (cancelled) {
                    return;
                }

                clearAccessToken();

                queryClient.removeQueries({
                    queryKey: queryKeys.auth.me,
                });

                setUnauthenticated();
            }
        }

        void initializeAuth();

        return () => {
            cancelled = true;
            window.removeEventListener(
                "auth:session-expired",
                handleSessionExpired,
            );
        };
    }, [
        queryClient,
        setAuthenticatedUser,
        setUnauthenticated,
    ]);

    return children;
}
