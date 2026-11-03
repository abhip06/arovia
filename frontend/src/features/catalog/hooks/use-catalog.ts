"use client";

import { useQuery } from "@tanstack/react-query";

import { catalogApi } from "../api/catalog.api";
import { queryKeys } from "@/lib/query/query-keys";
import { useRequestPincode } from "@/features/location/use-request-pincode";
import type { CatalogProductParams } from "../types/catalog.types";

export function useCatalogProducts(params: CatalogProductParams) {
    const pincode = useRequestPincode();
    const requestParams = {
        ...params,
        ...(pincode ? { pincode } : {}),
    };

    return useQuery({
        queryKey: queryKeys.catalog.products(requestParams),
        queryFn: () => catalogApi.getCatalogProducts(requestParams),
    });
}

export function useCatalogBrands() {
    return useQuery({
        queryKey: queryKeys.catalog.brands,
        queryFn: catalogApi.getBrands,
        staleTime: 10 * 60 * 1000,
    });
}

export function useCatalogManufacturers() {
    return useQuery({
        queryKey: queryKeys.catalog.manufacturers,
        queryFn: catalogApi.getManufacturers,
        staleTime: 10 * 60 * 1000,
    });
}

export function useCatalogCategories() {
    return useQuery({
        queryKey: queryKeys.catalog.categories,
        queryFn: catalogApi.getCategories,
        staleTime: 10 * 60 * 1000,
    });
}
