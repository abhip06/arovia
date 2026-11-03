"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "@/lib/query/query-keys";
import { adminUsersApi, type AdminUserRole } from "../api/admin-users.api";

export function useAdminUsers() {
  return useQuery({
    queryKey: queryKeys.adminUsers.all,
    queryFn: adminUsersApi.listUsers,
    staleTime: 60_000,
  });
}

export function useChangeAdminUserRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: AdminUserRole }) =>
      adminUsersApi.changeUserRole(userId, role),
    onSuccess: (_result, variables) => {
      queryClient.setQueryData(queryKeys.adminUsers.all, (current: unknown) => {
        if (!current || typeof current !== "object" || !("users" in current)) {
          return current;
        }

        const response = current as {
          users: { id: string; role: AdminUserRole }[];
          totalCount: number;
        };

        return {
          ...response,
          users: response.users.map((user) =>
            user.id === variables.userId
              ? { ...user, role: variables.role }
              : user,
          ),
        };
      });

      void queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.all });
    },
  });
}
