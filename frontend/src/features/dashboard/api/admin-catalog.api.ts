import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

export interface ProductPriceRecord {
  id: string;
  productVariantId: string;
  sellingPrice: number;
  effectiveFrom: string;
  isCurrent: boolean;
  isFuture: boolean;
}

export interface CreatedProduct {
  id: string;
  name: string;
  slug: string;
  productType: string;
  status: string;
}

export interface CreatedReference {
  id: string;
  name: string;
  slug: string;
  status: string;
}

export interface CreatedVariant {
  id: string;
  productId: string;
  sku: string;
  barcode: string | null;
  packageQuantity: number;
  packageUnit: string;
  netQuantity: number | null;
  netQuantityUnit: string | null;
  status: string;
}

interface CreateProductRequest {
  name: string;
  productType: number;
  manufacturerId: string;
  brandId?: string | null;
  requiresPrescription: boolean;
  hsnCode: string;
  taxRate: number;
  returnable: boolean;
  returnWindowDays: number;
}

interface CreateVariantRequest {
  barcode?: string | null;
  packageQuantity: number;
  packageUnit: string;
  netQuantity?: number | null;
  netQuantityUnit?: string | null;
}

interface CreatePriceRequest {
  sellingPrice: number;
  effectiveFrom?: string | null;
}

async function request<T>(run: () => Promise<{ data: T }>): Promise<T> {
  try {
    return (await run()).data;
  } catch (error) {
    throw normalizeApiError(error);
  }
}

export const adminCatalogApi = {
  createProduct: (body: CreateProductRequest) =>
    request(() => apiClient.post<CreatedProduct>("/v1/admin/products", body)),

  createBrand: (name: string) =>
    request(() => apiClient.post<CreatedReference>("/v1/admin/brands", { name })),

  createManufacturer: (name: string) =>
    request(() => apiClient.post<CreatedReference>("/v1/admin/manufacturers", { name })),

  productPrices: (variantId: string) =>
    request(() => apiClient.get<ProductPriceRecord[]>(
      `/v1/admin/products/variants/${encodeURIComponent(variantId)}/prices`,
    )),

  createPrice: (productId: string, variantId: string, body: CreatePriceRequest) =>
    request(() => apiClient.post<{
      id: string;
      productVariantId: string;
      sellingPrice: number;
      effectiveFrom: string;
    }>(
      `/v1/admin/products/${encodeURIComponent(productId)}/variants/${encodeURIComponent(variantId)}/prices`,
      body,
    )),

  createVariant: (productId: string, body: CreateVariantRequest) =>
    request(() => apiClient.post<CreatedVariant>(
      `/v1/admin/products/${encodeURIComponent(productId)}/variants`,
      body,
    )),

  setProductStatus: (
    productId: string,
    action: "activate" | "deactivate" | "discontinue",
  ) =>
    request(() => apiClient.post<void>(
      `/v1/admin/products/${encodeURIComponent(productId)}/${action}`,
    )),

  setVariantStatus: (
    variantId: string,
    action: "activate" | "deactivate" | "discontinue",
  ) =>
    request(() => apiClient.post<void>(
      `/v1/admin/products/variants/${encodeURIComponent(variantId)}/${action}`,
    )),
};
