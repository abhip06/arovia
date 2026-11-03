"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query/query-keys";

import { getCurrentUser, updateCurrentUser } from "../api/user.api";
import { CurrentUser, UpdateCurrentUserRequest } from "@/types/user.types";

export function useCurrentUser() {
    return useQuery({
        queryKey: queryKeys.auth.me,
        queryFn: getCurrentUser,
        staleTime: 5 * 60 * 1000,
    });
}

export function useUpdateCurrentUser() {
    const queryClient = useQueryClient();

    return useMutation<CurrentUser, Error, UpdateCurrentUserRequest>({
        mutationFn: updateCurrentUser,

        onSuccess: (updatedUser) => {
            queryClient.setQueryData(
                queryKeys.auth.me,
                updatedUser,
            );
        },
    });
}