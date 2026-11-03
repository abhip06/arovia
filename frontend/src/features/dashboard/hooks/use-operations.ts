"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "@/lib/query/query-keys";
import { operationsApi } from "../api/operations.api";

interface ReceiveStockVariables {
  batchId: string;
  warehouseId: string;
  quantity: number;
  costPrice: number;
}

interface AdjustStockVariables {
  batchId: string;
  warehouseId: string;
  quantityDelta: number;
  reason: string;
}

interface TransferStockVariables {
  batchId: string;
  sourceId: string;
  destinationId: string;
  quantity: number;
  note?: string;
}

interface CreateBatchVariables {
  variantId: string;
  batchNumber: string;
  expiryDate?: string | null;
  sellableUntil?: string | null;
  mrp: number;
}

interface BatchStatusVariables {
  batchId: string;
  action: "activate" | "quarantine" | "recall";
}

export function useWarehouses(page = 1, enabled = true) {
  return useQuery({
    queryKey: queryKeys.operations.warehouses(page),
    queryFn: () => operationsApi.warehouses(page),
    enabled,
  });
}

export function useInventoryBatches(page = 1, enabled = true) {
  return useQuery({
    queryKey: queryKeys.operations.batches(page),
    queryFn: () => operationsApi.batches(page),
    enabled,
  });
}

export function useWarehouseStock(
  warehouseId: string,
  page = 1,
  enabled = true,
) {
  return useQuery({
    queryKey: queryKeys.operations.stock(warehouseId, page),
    queryFn: () => operationsApi.stock(warehouseId, page),
    enabled: Boolean(warehouseId) && enabled,
  });
}

export function useInventoryMovements(stockId: string, page = 1) {
  return useQuery({
    queryKey: queryKeys.operations.movements(stockId, page),
    queryFn: () => operationsApi.movements(stockId, page),
    enabled: Boolean(stockId),
  });
}

function useInventoryMutation<TVariables>(
  mutationFn: (variables: TVariables) => Promise<unknown>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn,
    onSuccess: () => {
      // Refreshes run in the background so a later GET failure cannot make a
      // completed stock write look unsuccessful or invite a duplicate retry.
      void queryClient.invalidateQueries({ queryKey: ["operations", "stock"] });
      void queryClient.invalidateQueries({ queryKey: ["operations", "batches"] });
      void queryClient.invalidateQueries({ queryKey: ["operations", "movements"] });
    },
  });
}

export function useReceiveStock() {
  return useInventoryMutation(({ batchId, warehouseId, quantity, costPrice }: ReceiveStockVariables) =>
    operationsApi.receive(batchId, warehouseId, { quantity, costPrice }),
  );
}

export function useAdjustStock() {
  return useInventoryMutation(({ batchId, warehouseId, quantityDelta, reason }: AdjustStockVariables) =>
    operationsApi.adjust(batchId, warehouseId, { quantityDelta, reason }),
  );
}

export function useTransferStock() {
  return useInventoryMutation(({ batchId, sourceId, destinationId, quantity, note }: TransferStockVariables) =>
    operationsApi.transfer(batchId, sourceId, destinationId, { quantity, note }),
  );
}

export function useCreateWarehouse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: operationsApi.createWarehouse,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["operations", "warehouses"] });
    },
  });
}

export function useCreateInventoryBatch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ variantId, ...body }: CreateBatchVariables) =>
      operationsApi.createBatch(variantId, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["operations", "batches"] });
    },
  });
}

export function useSetBatchStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ batchId, action }: BatchStatusVariables) =>
      operationsApi.setBatchStatus(batchId, action),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["operations", "batches"] });
    },
  });
}
