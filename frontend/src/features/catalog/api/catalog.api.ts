import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";
import type {
    CatalogCategory,
    CatalogOption,
    CatalogProduct,
    CatalogProductParams,
    PagedResult,
    ProductDetail,
} from "../types/catalog.types";

async function getCatalogProducts(
    params: CatalogProductParams,
): Promise<PagedResult<CatalogProduct>> {
    try {
        const { data } = await apiClient.get<PagedResult<CatalogProduct>>(
            "/v1/catalog/products",
            { params },
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getBrands(): Promise<CatalogOption[]> {
    try {
        const { data } = await apiClient.get<CatalogOption[]>(
            "/v1/catalog/brands",
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getManufacturers(): Promise<CatalogOption[]> {
    try {
        const { data } = await apiClient.get<CatalogOption[]>(
            "/v1/catalog/manufacturers",
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getCategories(): Promise<CatalogCategory[]> {
    try {
        const { data } = await apiClient.get<CatalogCategory[]>(
            "/v1/catalog/categories",
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function getProductDetail(
    productSlug: string,
    sku: string,
): Promise<ProductDetail> {
    try {
        const { data } = await apiClient.get<ProductDetail>(
            `/v1/catalog/products/${encodeURIComponent(productSlug)}/variants/${encodeURIComponent(sku)}`,
        );

        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function createBrand(name: string) {
    try {
        const { data } = await apiClient.post<{ id: string; name: string; slug: string; status: string }>("/v1/admin/brands", { name });
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function createManufacturer(name: string) {
    try {
        const { data } = await apiClient.post<{ id: string; name: string; slug: string; status: string }>("/v1/admin/manufacturers", { name });
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

async function createCategory(input: { productType: number; name: string; parentCategoryId: string | null; sortOrder: number }) {
    try {
        const { data } = await apiClient.post("/v1/admin/categories", input);
        return data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export const catalogApi = {
    getCatalogProducts,
    getBrands,
    getManufacturers,
    getCategories,
    getProductDetail,
    createBrand,
    createManufacturer,
    createCategory,
};
