import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Warehouse {
  id?: string;
  code: string;
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  status: string;
}

export interface Batch {
  sku: string;
  batchNumber: string;
  expiryDate: string | null;
  sellableUntil: string | null;
  mrp: number;
  status: string;
}

export interface WarehouseStock {
  id: string;
  inventoryBatchId: string;
  warehouseId: string;
  sku: string;
  batchNumber: string;
  expiryDate: string | null;
  costPrice: number;
  quantityOnHand: number;
  quantityReserved: number;
  availableQuantity: number;
  receivedAt: string;
}

export interface Movement {
  id: string;
  warehouseStockId: string;
  movementType: number | string;
  quantityDelta: number;
  createdByUserId: string | null;
  referenceType: string | null;
  referenceId: string | null;
  note: string | null;
  createdAt: string;
}

export interface MovementPage {
  items: Movement[];
  page: number;
  pageSize: number;
  hasNextPage: boolean;
}

export interface CreatedBatch {
  id: string;
  productVariantId: string;
  batchNumber: string;
  expiryDate: string | null;
  sellableUntil: string | null;
  mrp: number;
  status: string;
}

interface WarehousePayload {
  code: string;
  name: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

async function request<T>(run: () => Promise<{ data: T }>): Promise<T> {
  try {
    return (await run()).data;
  } catch (error) {
    throw normalizeApiError(error);
  }
}

export const operationsApi = {
  warehouses: (page = 1) =>
    request(() => apiClient.get<Paged<Warehouse>>("/v1/admin/warehouses", { params: { page } })),

  batches: (page = 1) =>
    request(() => apiClient.get<Paged<Batch>>("/v1/inventory/batches", { params: { page } })),

  stock: (warehouseId: string, page = 1) =>
    request(() => apiClient.get<Paged<WarehouseStock>>(
      `/v1/inventory/warehouses/${encodeURIComponent(warehouseId)}/stock`,
      { params: { page } },
    )),

  movements: (stockId: string, page = 1) =>
    request(() => apiClient.get<MovementPage>(
      `/v1/inventory/stock/${encodeURIComponent(stockId)}/movements`,
      { params: { page } },
    )),

  createWarehouse: (body: WarehousePayload) =>
    request(() => apiClient.post<Warehouse>("/v1/admin/warehouses", body)),

  createBatch: (
    variantId: string,
    body: {
      batchNumber: string;
      expiryDate?: string | null;
      sellableUntil?: string | null;
      mrp: number;
    },
  ) =>
    request(() => apiClient.post<CreatedBatch>(
      `/v1/inventory/variants/${variantId}/batches`,
      body,
    )),

  setBatchStatus: (
    batchId: string,
    action: "activate" | "quarantine" | "recall",
  ) =>
    request(() => apiClient.post<void>(`/v1/inventory/batches/${batchId}/${action}`)),

  receive: (
    batchId: string,
    warehouseId: string,
    body: { quantity: number; costPrice: number },
  ) =>
    request(() => apiClient.post(
      `/v1/inventory/batches/${batchId}/warehouses/${warehouseId}/receive`,
      body,
    )),

  adjust: (
    batchId: string,
    warehouseId: string,
    body: { quantityDelta: number; reason: string },
  ) =>
    request(() => apiClient.post(
      `/v1/inventory/batches/${batchId}/warehouses/${warehouseId}/adjust`,
      body,
    )),

  transfer: (
    batchId: string,
    sourceId: string,
    destinationId: string,
    body: { quantity: number; note?: string },
  ) =>
    request(() => apiClient.post(
      `/v1/inventory/batches/${batchId}/transfer/${sourceId}/${destinationId}`,
      body,
    )),
};
