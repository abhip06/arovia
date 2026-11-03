"use client";

import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "@/lib/query/query-keys";
import { adminPaymentsApi } from "../api/admin-payments.api";

export function useAdminPayments(page: number) {
  return useQuery({
    queryKey: queryKeys.adminPayments.page(page),
    queryFn: () => adminPaymentsApi.getPage(page),
    staleTime: 30_000,
    placeholderData: (previousPage) => previousPage,
  });
}
