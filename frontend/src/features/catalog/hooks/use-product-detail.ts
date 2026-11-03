"use client";

import { useQuery } from "@tanstack/react-query";

import { catalogApi } from "../api/catalog.api";
import { queryKeys } from "@/lib/query/query-keys";

export function useProductDetail(productSlug: string, sku: string) {
    return useQuery({
        queryKey: queryKeys.catalog.productDetail(productSlug, sku),
        queryFn: () => catalogApi.getProductDetail(productSlug, sku),
        staleTime: 60_000,
    });
}
