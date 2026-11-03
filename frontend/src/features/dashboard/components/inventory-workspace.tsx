"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Activity, Boxes, Clock3, PackageCheck } from "lucide-react";
import { useAuth } from "@/features/auth/context/auth-context";
import { catalogApi } from "@/features/catalog/api/catalog.api";
import type { CatalogProduct, PagedResult } from "@/features/catalog/types/catalog.types";
import { queryKeys } from "@/lib/query/query-keys";
import { useQuery } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useAdjustStock, useCreateInventoryBatch, useCreateWarehouse, useInventoryBatches, useInventoryMovements, useReceiveStock, useSetBatchStatus, useTransferStock, useWarehouseStock, useWarehouses } from "../hooks/use-operations";
import type { WarehouseStock } from "../api/operations.api";
import { useDashboardUiStore } from "../stores/dashboard-ui.store";
import { MetricCard, PageControls, PageHeading, QueryState, SearchField, SectionTitle, StatusPill, Surface } from "./dashboard-primitives";
import { CapabilityPage } from "./capability-page";

type View = "overview" | "stock" | "batches" | "movements" | "warehouses" | "replenishment";
const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

const emptyStockAction: StockActionValues = {
  quantity: "",
  costPrice: "",
  quantityDelta: "",
  reason: "",
  destinationId: "",
  note: "",
};

const stockActionSchema = (mode: "receive" | "adjust" | "transfer") =>
  z.object({
    quantity: z.string().optional(),
    costPrice: z.string().optional(),
    quantityDelta: z.string().optional(),
    reason: z.string().optional(),
    destinationId: z.string().optional(),
    note: z.string().optional(),
  }).superRefine((value, context) => {
    const addError = (
      path: "quantity" | "costPrice" | "quantityDelta" | "reason" | "destinationId",
      message: string,
    ) => context.addIssue({ code: "custom", path: [path], message });

    if (mode === "receive" || mode === "transfer") {
      const quantity = Number(value.quantity);
      if (!Number.isInteger(quantity) || quantity < 1) {
        addError("quantity", "Enter a whole quantity greater than zero.");
      }
    }

    if (mode === "receive") {
      const cost = Number(value.costPrice);
      if (!(value.costPrice ?? "").trim() || !Number.isFinite(cost) || cost < 0) {
        addError("costPrice", "Enter a valid non-negative cost price.");
      }
    }

    if (mode === "adjust") {
      const delta = Number(value.quantityDelta);
      if (!Number.isInteger(delta) || delta === 0) {
        addError("quantityDelta", "Enter a non-zero whole number.");
      }
      if ((value.reason ?? "").trim().length < 3) {
        addError("reason", "Add a reason of at least 3 characters.");
      }
    }

    if (mode === "transfer" && !uuidPattern.test(value.destinationId ?? "")) {
      addError("destinationId", "Enter a valid destination warehouse ID.");
    }
  });
type StockActionValues = z.infer<ReturnType<typeof stockActionSchema>>;

function StockActionDialog({
  open,
  onOpenChange,
  mode,
  stock,
  activeWarehouseId,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: "receive" | "adjust" | "transfer" | null;
  stock: WarehouseStock | null;
  activeWarehouseId: string;
}) {
  const receive = useReceiveStock();
  const adjust = useAdjustStock();
  const transfer = useTransferStock();
  const schema = stockActionSchema(mode ?? "receive");
  const form = useForm<StockActionValues>({
    resolver: zodResolver(schema),
    defaultValues: emptyStockAction,
  });
  const resetForm = form.reset;
  const busy = receive.isPending || adjust.isPending || transfer.isPending;

  useEffect(() => {
    if (open) resetForm(emptyStockAction);
  }, [mode, open, resetForm, stock?.id]);

  const titles = {
    receive: "Receive stock",
    adjust: "Adjust stock",
    transfer: "Transfer batch stock",
  };

  const apply = form.handleSubmit(async (values) => {
    if (!mode || !stock) return;

    try {
      if (mode === "receive") {
        await receive.mutateAsync({
          batchId: stock.inventoryBatchId,
          warehouseId: activeWarehouseId,
          quantity: Number(values.quantity),
          costPrice: Number(values.costPrice),
        });
      }

      if (mode === "adjust") {
        await adjust.mutateAsync({
          batchId: stock.inventoryBatchId,
          warehouseId: activeWarehouseId,
          quantityDelta: Number(values.quantityDelta),
          reason: values.reason!.trim(),
        });
      }

      if (mode === "transfer") {
        if (values.destinationId === activeWarehouseId) {
          form.setError("destinationId", {
            message: "Choose a different destination warehouse.",
          });
          return;
        }

        await transfer.mutateAsync({
          batchId: stock.inventoryBatchId,
          sourceId: activeWarehouseId,
          destinationId: values.destinationId!,
          quantity: Number(values.quantity),
          note: values.note?.trim(),
        });
      }

      const successMessage = {
        receive: "Stock received",
        adjust: "Stock adjustment recorded",
        transfer: "Stock transfer completed",
      }[mode];

      toast.success(successMessage);
      onOpenChange(false);
      form.reset(emptyStockAction);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "The inventory action failed.");
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{mode ? titles[mode] : "Stock action"}</DialogTitle>
          <DialogDescription>
            {stock
              ? `${stock.sku} · lot ${stock.batchNumber} · ${stock.availableQuantity} available. `
              : "Review the stock change before applying. "}
            This action will be recorded through the inventory API.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={apply} className="space-y-4">
          {(mode === "receive" || mode === "transfer") && (
            <Field label="Quantity" error={form.formState.errors.quantity?.message}>
              <Input type="number" min="1" step="1" inputMode="numeric" {...form.register("quantity")} />
            </Field>
          )}

          {mode === "receive" && (
            <Field label="Unit cost price (₹)" error={form.formState.errors.costPrice?.message}>
              <Input type="number" min="0" step="0.01" {...form.register("costPrice")} />
            </Field>
          )}

          {mode === "adjust" && (
            <>
              <Field label="Quantity change (+/-)" error={form.formState.errors.quantityDelta?.message}>
                <Input type="number" step="1" {...form.register("quantityDelta")} />
              </Field>
              <Field label="Reason" error={form.formState.errors.reason?.message}>
                <Textarea rows={3} placeholder="For example: damaged units removed" {...form.register("reason")} />
              </Field>
            </>
          )}

          {mode === "transfer" && (
            <>
              <Field label="Destination warehouse ID" error={form.formState.errors.destinationId?.message}>
                <Input placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" {...form.register("destinationId")} />
              </Field>
              <Field label="Transfer note">
                <Textarea rows={2} placeholder="Optional transfer context" {...form.register("note")} />
              </Field>
            </>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={busy}>
              {busy
                ? "Saving…"
                : mode === "adjust"
                  ? "Confirm adjustment"
                  : mode === "transfer"
                    ? "Confirm transfer"
                    : "Confirm receipt"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

const warehouseSchema = z.object({
  code: z.string().trim().min(2).max(50),
  name: z.string().trim().min(2).max(150),
  addressLine1: z.string().trim().min(2).max(200),
  addressLine2: z.string().optional(),
  city: z.string().trim().min(2),
  state: z.string().trim().min(2),
  postalCode: z.string().trim().min(2).max(20),
  country: z.string().trim().min(2),
});
type WarehouseValues = z.infer<typeof warehouseSchema>;

function CreateWarehouseDialog({
  open,
  onOpenChange,
  onCreated,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: (id: string) => void;
}) {
  const create = useCreateWarehouse();
  const form = useForm<WarehouseValues>({
    resolver: zodResolver(warehouseSchema),
    defaultValues: {
      code: "",
      name: "",
      addressLine1: "",
      addressLine2: "",
      city: "",
      state: "",
      postalCode: "",
      country: "India",
    },
  });

  const submit = form.handleSubmit(async (value) => {
    try {
      const result = await create.mutateAsync(value);
      if (result.id) onCreated(result.id);
      toast.success("Warehouse created");
      form.reset();
      onOpenChange(false);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Warehouse creation failed.");
    }
  });

  const fields = [
    ["code", "Warehouse code"],
    ["name", "Warehouse name"],
    ["addressLine1", "Address line 1"],
    ["addressLine2", "Address line 2 (optional)"],
    ["city", "City"],
    ["state", "State"],
    ["postalCode", "Postal code"],
    ["country", "Country"],
  ] as const;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>Add warehouse</DialogTitle>
          <DialogDescription>
            Create a warehouse through the admin API. The response includes its ID for inventory workflows.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={submit} className="grid gap-3 sm:grid-cols-2">
          {fields.map(([name, label]) => (
            <Field key={name} label={label} error={form.formState.errors[name]?.message}>
              <Input {...form.register(name)} />
            </Field>
          ))}
          <div className="sm:col-span-2">
            <DialogFooter>
              <Button type="button" variant="outline" disabled={create.isPending} onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={create.isPending}>
                {create.isPending ? "Creating…" : "Create warehouse"}
              </Button>
            </DialogFooter>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

const batchSchema = z.object({
  variantId: z.string().regex(uuidPattern, "Choose a valid product variant."),
  batchNumber: z.string().trim().min(1).max(100),
  expiryDate: z.string().optional(),
  sellableUntil: z.string().optional(),
  mrp: z.string().refine(
    (value) => Number.isFinite(Number(value)) && Number(value) >= 0,
    "Enter a valid MRP.",
  ),
});
type BatchValues = z.infer<typeof batchSchema>;

function CreateBatchDialog({
  open,
  onOpenChange,
  onCreated,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: (id: string) => void;
}) {
  const [variantPage, setVariantPage] = useState(1);
  const [variantSearch, setVariantSearch] = useState("");
  const products = useQuery<PagedResult<CatalogProduct>>({
    queryKey: queryKeys.catalog.products({
      page: variantPage,
      search: variantSearch,
      inventoryBatchCreate: true,
    }),
    queryFn: () => catalogApi.getCatalogProducts({
      page: variantPage,
      search: variantSearch.trim() || undefined,
    }),
    enabled: open,
  });
  const create = useCreateInventoryBatch();
  const form = useForm<BatchValues>({
    resolver: zodResolver(batchSchema),
    defaultValues: {
      variantId: "",
      batchNumber: "",
      expiryDate: "",
      sellableUntil: "",
      mrp: "",
    },
  });

  const submit = form.handleSubmit(async (value) => {
    try {
      const result = await create.mutateAsync({
        variantId: value.variantId,
        batchNumber: value.batchNumber.trim(),
        expiryDate: value.expiryDate || null,
        sellableUntil: value.sellableUntil || null,
        mrp: Number(value.mrp),
      });
      onCreated(result.id);
      toast.success(`Batch ${result.batchNumber} created`);
      form.reset();
      onOpenChange(false);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Batch creation failed.");
    }
  });

  const searchVariants = (value: string) => {
    setVariantSearch(value);
    setVariantPage(1);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Create inventory batch</DialogTitle>
          <DialogDescription>
            Link a lot to a published product variant. Receive quantities into a warehouse after creating the batch.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={submit} className="space-y-3">
          <div className="grid gap-1.5 text-xs font-medium">
            <label htmlFor="batch-product-search">Find a published product SKU</label>
            <Input
              id="batch-product-search"
              value={variantSearch}
              onChange={(event) => searchVariants(event.target.value)}
              placeholder="Search product or SKU"
            />
            <select
              aria-label="Product variant"
              className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus:ring-2 focus:ring-ring"
              {...form.register("variantId")}
            >
              <option value="">Choose a SKU</option>
              {products.data?.items.map((item) => (
                <option key={item.productVariantId} value={item.productVariantId}>
                  {item.productName} · {item.sku}
                </option>
              ))}
            </select>
            {form.formState.errors.variantId && (
              <span className="font-normal text-destructive">{form.formState.errors.variantId.message}</span>
            )}
            {products.isError && (
              <span className="font-normal text-destructive">Product variants could not be loaded.</span>
            )}
            {!products.isLoading && products.data?.items.length === 0 && (
              <span className="font-normal text-muted-foreground">No matching published SKUs. Try a different search.</span>
            )}
            {products.isLoading && <span className="font-normal text-muted-foreground">Loading product SKUs…</span>}
            {products.data && products.data.totalPages > 1 && (
              <PageControls page={products.data.page} totalPages={products.data.totalPages} onChange={setVariantPage} />
            )}
          </div>

          <Field label="Batch / lot number" error={form.formState.errors.batchNumber?.message}>
            <Input {...form.register("batchNumber")} />
          </Field>
          <div className="grid gap-3 sm:grid-cols-2">
            <Field label="Expiry date">
              <Input type="date" {...form.register("expiryDate")} />
            </Field>
            <Field label="Sellable until">
              <Input type="date" {...form.register("sellableUntil")} />
            </Field>
          </div>
          <Field label="Maximum retail price (₹)" error={form.formState.errors.mrp?.message}>
            <Input type="number" min="0" step="0.01" {...form.register("mrp")} />
          </Field>
          <DialogFooter>
            <Button type="button" variant="outline" disabled={create.isPending} onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={create.isPending || products.isLoading || products.isError}>
              {create.isPending ? "Creating…" : "Create batch"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <label className="grid gap-1.5 text-xs font-medium">
      {label}
      {children}
      {error && <span className="font-normal text-destructive">{error}</span>}
    </label>
  );
}

function ConnectWarehouse({
  warehouseId,
  setWarehouseId,
}: {
  warehouseId: string;
  setWarehouseId: (id: string) => void;
}) {
  const [draft, setDraft] = useState(warehouseId);

  return (
    <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row">
      <Input
        value={draft}
        onChange={(event) => setDraft(event.target.value)}
        placeholder="Warehouse ID (UUID)"
        aria-label="Warehouse ID"
        className="sm:w-72"
      />
      <Button
        variant="outline"
        onClick={() => {
          const value = draft.trim();
          if (!uuidPattern.test(value)) {
            toast.error("Enter a valid warehouse ID.");
            return;
          }
          setWarehouseId(value);
          toast.success("Warehouse connected");
        }}
      >
        Connect warehouse
      </Button>
    </div>
  );
}

function StockTable({
  page,
  setPage,
  warehouseId,
  setSelectedStockId,
}: {
  page: number;
  setPage: (page: number) => void;
  warehouseId: string;
  setSelectedStockId: (id: string) => void;
}) {
  const [search, setSearch] = useState("");
  const query = useWarehouseStock(warehouseId, page);
  const [action, setAction] = useState<{
    mode: "receive" | "adjust" | "transfer";
    stock: WarehouseStock;
  } | null>(null);
  const stocks = useMemo(() => (query.data?.items ?? [])
    .filter((row) => `${row.sku} ${row.batchNumber}`.toLowerCase().includes(search.toLowerCase()))
    .sort((left, right) => {
      const leftExpiry = left.expiryDate ? Date.parse(left.expiryDate) : Number.POSITIVE_INFINITY;
      const rightExpiry = right.expiryDate ? Date.parse(right.expiryDate) : Number.POSITIVE_INFINITY;
      return leftExpiry - rightExpiry;
    }), [query.data?.items, search]);

  const chooseAction = (mode: "receive" | "adjust" | "transfer", stock: WarehouseStock) => {
    setAction({ mode, stock });
  };

  const rowActions = (stock: WarehouseStock) => (
    <div className="flex flex-wrap gap-1.5">
      <Button size="sm" variant="outline" onClick={() => chooseAction("receive", stock)}>Receive</Button>
      <Button size="sm" variant="outline" onClick={() => chooseAction("adjust", stock)}>Adjust</Button>
      <Button size="sm" variant="outline" onClick={() => chooseAction("transfer", stock)}>Transfer</Button>
      <Button size="sm" variant="ghost" onClick={() => setSelectedStockId(stock.id)}>History</Button>
    </div>
  );

  return (
    <Surface>
      <SectionTitle
        title="Warehouse stock"
        description="On hand, reserved, and available quantities by batch. Rows follow earliest expiry (FEFO)."
        action={<SearchField value={search} onChange={setSearch} placeholder="Filter SKU or batch" />}
      />
      <QueryState
        loading={query.isLoading}
        error={query.error}
        empty={!query.isLoading && !query.error && stocks.length === 0}
        onRetry={() => void query.refetch()}
      />

      {stocks.length > 0 && (
        <>
          <div className="hidden overflow-x-auto lg:block">
            <table className="w-full min-w-[900px] text-left text-sm">
              <thead className="bg-muted/35 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                <tr>
                  <th className="px-4 py-3">Product / lot</th>
                  <th className="px-4 py-3">Expiry</th>
                  <th className="px-4 py-3">On hand</th>
                  <th className="px-4 py-3">Reserved</th>
                  <th className="px-4 py-3">Available</th>
                  <th className="px-4 py-3">Unit cost</th>
                  <th className="px-4 py-3">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {stocks.map((stock) => (
                  <tr key={stock.id} className="hover:bg-muted/20">
                    <td className="px-4 py-3">
                      <p className="font-medium">{stock.sku}</p>
                      <p className="mt-1 text-[10px] text-muted-foreground">Lot {stock.batchNumber}</p>
                    </td>
                    <td className="px-4 py-3 text-xs text-muted-foreground">
                      {stock.expiryDate ? new Date(`${stock.expiryDate}T00:00:00`).toLocaleDateString("en-IN") : "—"}
                    </td>
                    <td className="px-4 py-3 tabular-nums">{stock.quantityOnHand}</td>
                    <td className="px-4 py-3 tabular-nums">{stock.quantityReserved}</td>
                    <td className="px-4 py-3">
                      <span className={`font-semibold tabular-nums ${stock.availableQuantity <= 5 ? "text-destructive" : ""}`}>
                        {stock.availableQuantity}
                      </span>
                      {stock.availableQuantity <= 5 && (
                        <span className="ml-2 text-[9px] font-semibold uppercase text-destructive">Low</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-xs">₹{stock.costPrice.toLocaleString("en-IN")}</td>
                    <td className="px-4 py-3">{rowActions(stock)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="divide-y lg:hidden">
            {stocks.map((stock) => (
              <article key={stock.id} className="space-y-3 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-semibold">{stock.sku}</p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      Lot {stock.batchNumber} · {stock.expiryDate
                        ? `expires ${new Date(`${stock.expiryDate}T00:00:00`).toLocaleDateString("en-IN")}`
                        : "no expiry date"}
                    </p>
                  </div>
                  <StatusPill variant={stock.availableQuantity <= 5 ? "danger" : "success"}>
                    {stock.availableQuantity <= 5 ? "Low stock" : "Available"}
                  </StatusPill>
                </div>
                <div className="grid grid-cols-3 gap-2">
                  <QuantityCell label="On hand" value={stock.quantityOnHand} />
                  <QuantityCell label="Reserved" value={stock.quantityReserved} />
                  <QuantityCell label="Available" value={stock.availableQuantity} />
                </div>
                {rowActions(stock)}
              </article>
            ))}
          </div>

        </>
      )}
      {query.data && query.data.totalCount > 0 && (
        <PageControls
          page={query.data.page}
          totalPages={query.data.totalPages}
          onChange={setPage}
        />
      )}

      <StockActionDialog
        open={Boolean(action)}
        onOpenChange={(open) => !open && setAction(null)}
        mode={action?.mode ?? null}
        stock={action?.stock ?? null}
        activeWarehouseId={warehouseId}
      />
    </Surface>
  );
}

function QuantityCell({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-lg bg-muted/40 px-2.5 py-2">
      <p className="text-[10px] text-muted-foreground">{label}</p>
      <p className="mt-0.5 text-sm font-semibold tabular-nums">{value}</p>
    </div>
  );
}

function BatchTable({
  page,
  setPage,
  batchId,
  setBatchId,
}: {
  page: number;
  setPage: (page: number) => void;
  batchId: string;
  setBatchId: (id: string) => void;
}) {
  const [search, setSearch] = useState("");
  const [pendingStatus, setPendingStatus] = useState<"activate" | "quarantine" | "recall" | null>(null);
  const query = useInventoryBatches(page);
  const action = useSetBatchStatus();
  const batches = useMemo(() => (query.data?.items ?? [])
    .filter((batch) => `${batch.sku} ${batch.batchNumber} ${batch.status}`.toLowerCase().includes(search.toLowerCase()))
    .sort((left, right) => {
      const leftDate = left.sellableUntil ?? left.expiryDate ?? "9999-12-31";
      const rightDate = right.sellableUntil ?? right.expiryDate ?? "9999-12-31";
      return Date.parse(leftDate) - Date.parse(rightDate);
    }), [query.data?.items, search]);

  const daysLeft = (value: string | null) => {
    if (!value) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const date = new Date(`${value}T00:00:00`);
    return Math.ceil((date.getTime() - today.getTime()) / 86_400_000);
  };

  const confirmStatus = async () => {
    if (!pendingStatus || !uuidPattern.test(batchId)) return;

    try {
      await action.mutateAsync({ batchId, action: pendingStatus });
      const resultLabel = {
        activate: "activated",
        quarantine: "quarantined",
        recall: "recalled",
      }[pendingStatus];
      toast.success(`Batch ${resultLabel}`);
      setPendingStatus(null);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Batch status could not be changed.");
    }
  };

  const statusVariant = (value: string): "danger" | "success" | "warning" => {
    if (/quarantin|recall|expired/i.test(value)) return "danger";
    if (/active/i.test(value)) return "success";
    return "warning";
  };

  const statusLabel = pendingStatus
    ? { activate: "activated", quarantine: "quarantined", recall: "recalled" }[pendingStatus]
    : "updated";

  return (
    <>
      <Surface>
        <SectionTitle
          title="Batch register"
          description="Lot-level expiry and sellable-until dates, ordered by FEFO. Search filters the loaded page."
          action={<SearchField value={search} onChange={setSearch} placeholder="Search SKU, lot, status" />}
        />
        <QueryState
          loading={query.isLoading}
          error={query.error}
          empty={!query.isLoading && !query.error && batches.length === 0}
          onRetry={() => void query.refetch()}
        />

        {batches.length > 0 && (
          <>
            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[760px] text-left text-sm">
                <thead className="bg-muted/35 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-5 py-3">SKU</th>
                    <th className="px-5 py-3">Batch / lot</th>
                    <th className="px-5 py-3">Expiry</th>
                    <th className="px-5 py-3">Sellable until</th>
                    <th className="px-5 py-3">MRP</th>
                    <th className="px-5 py-3">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {batches.map((batch) => {
                    const days = daysLeft(batch.expiryDate);
                    const expired = days !== null && days < 0;
                    const expiringSoon = days !== null && days <= 60;
                    const expiryStyle = expired
                      ? "font-semibold text-destructive"
                      : expiringSoon
                        ? "font-medium text-amber-700 dark:text-amber-300"
                        : "text-muted-foreground";

                    return (
                      <tr key={`${batch.sku}-${batch.batchNumber}`}>
                        <td className="px-5 py-3.5 font-medium">{batch.sku}</td>
                        <td className="px-5 py-3.5 font-mono text-xs">{batch.batchNumber}</td>
                        <td className="px-5 py-3.5">
                          <p className={`text-xs ${expiryStyle}`}>
                            {batch.expiryDate
                              ? new Date(`${batch.expiryDate}T00:00:00`).toLocaleDateString("en-IN")
                              : "Not recorded"}
                          </p>
                          {days !== null && (
                            <p className="mt-1 text-[10px] text-muted-foreground">
                              {expired ? `${Math.abs(days)} days past expiry` : `${days} days remaining`}
                            </p>
                          )}
                        </td>
                        <td className="px-5 py-3.5 text-xs text-muted-foreground">
                          {batch.sellableUntil
                            ? new Date(`${batch.sellableUntil}T00:00:00`).toLocaleDateString("en-IN")
                            : "—"}
                        </td>
                        <td className="px-5 py-3.5 text-xs">₹{batch.mrp.toLocaleString("en-IN")}</td>
                        <td className="px-5 py-3.5">
                          <StatusPill variant={statusVariant(batch.status)}>{batch.status}</StatusPill>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            <div className="divide-y md:hidden">
              {batches.map((batch) => {
                const days = daysLeft(batch.expiryDate);
                const expired = days !== null && days < 0;

                return (
                  <article key={`${batch.sku}-${batch.batchNumber}`} className="flex items-start justify-between gap-3 p-4">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold">{batch.sku}</p>
                      <p className="mt-1 font-mono text-xs text-muted-foreground">Lot {batch.batchNumber}</p>
                      <p className={`mt-2 text-xs ${expired ? "text-destructive" : "text-muted-foreground"}`}>
                        Expiry {batch.expiryDate
                          ? new Date(`${batch.expiryDate}T00:00:00`).toLocaleDateString("en-IN")
                          : "not recorded"}
                        {days !== null && ` · ${expired ? `${Math.abs(days)} days overdue` : `${days} days left`}`}
                      </p>
                      <p className="mt-1 text-xs text-muted-foreground">
                        Sellable until {batch.sellableUntil
                          ? new Date(`${batch.sellableUntil}T00:00:00`).toLocaleDateString("en-IN")
                          : "—"}
                        {` · MRP ₹${batch.mrp.toLocaleString("en-IN")}`}
                      </p>
                    </div>
                    <StatusPill variant={statusVariant(batch.status)}>{batch.status}</StatusPill>
                  </article>
                );
              })}
            </div>

          </>
        )}
        {query.data && query.data.totalCount > 0 && (
          <PageControls
            page={query.data.page}
            totalPages={query.data.totalPages}
            onChange={setPage}
          />
        )}
      </Surface>

      <Surface className="mt-5">
        <SectionTitle
          title="Batch status actions"
          description="The batch list omits UUIDs. This field is filled from a newly created batch; older batches need the create response ID."
        />
        <div className="flex flex-col gap-3 p-4 sm:flex-row sm:items-end">
          <Field label="Inventory batch ID">
            <Input
              value={batchId}
              onChange={(event) => setBatchId(event.target.value)}
              placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            />
          </Field>
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" disabled={action.isPending || !uuidPattern.test(batchId)} onClick={() => setPendingStatus("activate")}>
              Activate
            </Button>
            <Button variant="outline" disabled={action.isPending || !uuidPattern.test(batchId)} onClick={() => setPendingStatus("quarantine")}>
              Quarantine
            </Button>
            <Button variant="destructive" disabled={action.isPending || !uuidPattern.test(batchId)} onClick={() => setPendingStatus("recall")}>
              Recall batch
            </Button>
          </div>
        </div>
      </Surface>

      <Dialog
        open={Boolean(pendingStatus)}
        onOpenChange={(open) => !open && !action.isPending && setPendingStatus(null)}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Confirm {pendingStatus} batch</DialogTitle>
            <DialogDescription>
              This status change is written to the inventory API and affects this lot’s handling.
            </DialogDescription>
          </DialogHeader>
          <p className="rounded-lg bg-muted p-3 font-mono text-xs">{batchId}</p>
          <DialogFooter>
            <Button variant="outline" disabled={action.isPending} onClick={() => setPendingStatus(null)}>
              Cancel
            </Button>
            <Button
              variant={pendingStatus === "recall" ? "destructive" : "default"}
              disabled={action.isPending}
              onClick={() => void confirmStatus()}
            >
              {action.isPending ? "Updating…" : `Confirm ${statusLabel}`}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

function WarehouseTable({
  page,
  setPage,
  onCreated,
}: {
  page: number;
  setPage: (page: number) => void;
  onCreated: (id: string) => void;
}) {
  const query = useWarehouses(page);
  const [createOpen, setCreateOpen] = useState(false);

  return (
    <>
      <Surface>
        <SectionTitle
          title="Warehouse directory"
          description="Live admin warehouse list. The current list DTO does not include warehouse IDs."
          action={<Button onClick={() => setCreateOpen(true)}>Add warehouse</Button>}
        />
        <QueryState
          loading={query.isLoading}
          error={query.error}
          empty={!query.isLoading && !query.error && query.data?.items.length === 0}
          onRetry={() => void query.refetch()}
        />

        {query.data?.items.length ? (
          <>
            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[680px] text-left text-sm">
                <thead className="bg-muted/35 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-5 py-3">Warehouse</th>
                    <th className="px-5 py-3">Location</th>
                    <th className="px-5 py-3">Address</th>
                    <th className="px-5 py-3">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {query.data.items.map((warehouse) => (
                    <tr key={warehouse.code}>
                      <td className="px-5 py-4">
                        <p className="font-medium">{warehouse.name}</p>
                        <p className="mt-1 font-mono text-[10px] text-muted-foreground">{warehouse.code}</p>
                      </td>
                      <td className="px-5 py-4 text-xs">{warehouse.city}, {warehouse.state}</td>
                      <td className="px-5 py-4 text-xs text-muted-foreground">
                        {warehouse.addressLine1} · {warehouse.postalCode}
                      </td>
                      <td className="px-5 py-4">
                        <StatusPill variant={/active/i.test(warehouse.status) ? "success" : "muted"}>
                          {warehouse.status}
                        </StatusPill>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="divide-y md:hidden">
              {query.data.items.map((warehouse) => (
                <article key={warehouse.code} className="flex items-start justify-between gap-3 p-4">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-semibold">{warehouse.name}</p>
                    <p className="mt-1 font-mono text-[10px] text-muted-foreground">{warehouse.code}</p>
                    <p className="mt-2 text-xs">{warehouse.city}, {warehouse.state}</p>
                    <p className="mt-1 text-xs leading-5 text-muted-foreground">
                      {warehouse.addressLine1} · {warehouse.postalCode}
                    </p>
                  </div>
                  <StatusPill variant={/active/i.test(warehouse.status) ? "success" : "muted"}>
                    {warehouse.status}
                  </StatusPill>
                </article>
              ))}
            </div>

          </>
        ) : null}
        {query.data && query.data.totalCount > 0 && (
          <PageControls
            page={query.data.page}
            totalPages={query.data.totalPages}
            onChange={setPage}
          />
        )}
      </Surface>

      <CreateWarehouseDialog open={createOpen} onOpenChange={setCreateOpen} onCreated={onCreated} />
      <p className="mt-3 text-xs leading-5 text-muted-foreground">
        Stock queries and transfers require a warehouse UUID. The list omits IDs; a newly created warehouse returns its ID, or use the UUID supplied by your platform operator.
      </p>
    </>
  );
}

function MovementsPanel({ initialStockId = "" }: { initialStockId?: string }) {
  const [stockId, setStockId] = useState(initialStockId);
  const [draft, setDraft] = useState(initialStockId);
  const [page, setPage] = useState(1);
  const query = useInventoryMovements(stockId, page);

  const movementLabels: Record<string, string> = {
    1: "Received",
    2: "Reserved",
    3: "Released",
    4: "Adjusted",
    5: "Transfer out",
    6: "Transfer in",
    7: "Damaged",
    8: "Quarantined",
  };

  const loadLedger = () => {
    const nextStockId = draft.trim();
    if (!uuidPattern.test(nextStockId)) {
      toast.error("Enter a valid warehouse stock ID.");
      return;
    }
    setPage(1);
    setStockId(nextStockId);
  };

  return (
    <Surface>
      <SectionTitle
        title="Movement ledger"
        description="Immutable inventory movement history for one warehouse stock record."
        action={(
          <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row">
            <Input
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") loadLedger();
              }}
              placeholder="Warehouse stock ID"
              aria-label="Warehouse stock ID"
              className="sm:w-64"
            />
            <Button variant="outline" disabled={!uuidPattern.test(draft.trim())} onClick={loadLedger}>
              Load ledger
            </Button>
          </div>
        )}
      />

      {!stockId ? (
        <div className="px-5 py-12 text-center text-sm text-muted-foreground">
          Choose “History” from a stock row or enter a warehouse stock ID.
        </div>
      ) : (
        <>
          <QueryState
            loading={query.isLoading}
            error={query.error}
            empty={!query.isLoading && !query.error && query.data?.items.length === 0}
            onRetry={() => void query.refetch()}
          />

          {query.data?.items.length ? (
            <>
              <div className="divide-y">
                {query.data.items.map((movement) => {
                  const type = String(movement.movementType);
                  const isDecrease = movement.quantityDelta < 0;
                  const createdAt = new Date(movement.createdAt);
                  const createdLabel = Number.isNaN(createdAt.getTime())
                    ? "Date unavailable"
                    : createdAt.toLocaleString("en-IN");

                  return (
                    <article key={movement.id} className="flex flex-wrap items-center gap-3 px-4 py-4 sm:px-5">
                      <span className={`grid size-9 shrink-0 place-items-center rounded-xl ${isDecrease
                        ? "bg-amber-500/10 text-amber-700 dark:text-amber-300"
                        : "bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"}`}>
                        <span className="text-base font-semibold">
                          {movement.quantityDelta > 0 ? "+" : ""}{movement.quantityDelta}
                        </span>
                      </span>
                      <div className="min-w-[130px] flex-1">
                        <p className="text-sm font-medium">
                          {movementLabels[type] ?? `Movement ${type}`}
                        </p>
                        <p className="mt-1 text-xs text-muted-foreground">
                          {movement.note || movement.referenceType || "No note supplied"}
                        </p>
                      </div>
                      <div className="ml-auto text-right">
                        <p className="text-xs font-medium">{createdLabel}</p>
                        <p className="mt-1 font-mono text-[9px] text-muted-foreground">
                          {movement.id.slice(0, 8)}
                        </p>
                      </div>
                    </article>
                  );
                })}
              </div>

              <div className="flex items-center justify-between border-t px-4 py-3 text-xs text-muted-foreground sm:px-5">
                <Button
                  size="sm"
                  variant="outline"
                  disabled={page <= 1}
                  onClick={() => setPage((current) => current - 1)}
                >
                  Previous page
                </Button>
                <span>Page {query.data.page} · {query.data.items.length} events</span>
                <Button
                  size="sm"
                  variant="outline"
                  disabled={!query.data.hasNextPage}
                  onClick={() => setPage((current) => current + 1)}
                >
                  Next page
                </Button>
              </div>
            </>
          ) : null}
        </>
      )}
    </Surface>
  );
}

export function InventoryWorkspace({
  view,
  initialStockId = "",
}: {
  view: View;
  initialStockId?: string;
}) {
  const router = useRouter();
  const { user } = useAuth();
  const isAdmin = user?.role === "Admin";
  const [page, setPage] = useState(1);
  const [createBatchOpen, setCreateBatchOpen] = useState(false);
  const {
    activeWarehouseId,
    activeStockId,
    setActiveWarehouseId,
    setActiveStockId,
  } = useDashboardUiStore();
  const batches = useInventoryBatches(1, view === "overview");
  const stock = useWarehouseStock(
    activeWarehouseId,
    1,
    view === "overview" || view === "stock",
  );
  const headings: Record<View, [string, string, string]> = {
    overview: [
      "Inventory overview",
      "Stock at a glance",
      "Review real stock, batch, and expiry data returned by the API.",
    ],
    stock: [
      "Stock management",
      "Warehouse stock",
      "Manage stock by warehouse, SKU, and batch with on-hand, reserved, and available quantities.",
    ],
    batches: [
      "Batch & expiry",
      "Batch register",
      "Monitor expiry and sellable-until dates and take expiry-aware action by lot.",
    ],
    movements: [
      "Audit ledger",
      "Stock movements",
      "Trace receipts, adjustments, transfers, and other movements for a stock record.",
    ],
    warehouses: [
      "Warehouse network",
      "Warehouses",
      "Review active fulfillment locations and add a warehouse.",
    ],
    replenishment: [
      "Replenishment planning",
      "Replenishment",
      "Review low-stock signals and proposed replenishment workflows.",
    ],
  };
  const [eyebrow, title, description] = headings[view];
  const [lastCreatedBatchId, setLastCreatedBatchId] = useState("");
  const [statusBatchId, setStatusBatchId] = useState("");
  const [lastCreatedWarehouseId, setLastCreatedWarehouseId] = useState("");
  const dayStart = new Date();
  dayStart.setHours(0, 0, 0, 0);
  const expiryCutoff = new Date(dayStart);
  expiryCutoff.setDate(expiryCutoff.getDate() + 60);
  const soon = (batches.data?.items ?? []).filter((batch) => {
    if (!batch.expiryDate) return false;
    const date = new Date(`${batch.expiryDate}T00:00:00`);
    return date >= dayStart && date <= expiryCutoff;
  }).length;
  const lowStock = (stock.data?.items ?? []).filter((item) => item.availableQuantity <= 5).length;
  const setCurrentStockId = (id: string) => {
    setActiveStockId(id);
    router.push(`/inventory/movements?stockId=${encodeURIComponent(id)}`);
  };

  const copyIdentifier = async (value: string, label: string) => {
    try {
      await navigator.clipboard.writeText(value);
      toast.success(`${label} copied`);
    } catch {
      toast.error(`Could not copy the ${label.toLowerCase()}.`);
    }
  };

  return (
    <>
      <PageHeading
        eyebrow={eyebrow}
        title={title}
        description={description}
        action={view === "batches" ? (
          <Button onClick={() => setCreateBatchOpen(true)}>Create batch</Button>
        ) : view === "stock" ? (
          <ConnectWarehouse
            key={activeWarehouseId}
            warehouseId={activeWarehouseId}
            setWarehouseId={setActiveWarehouseId}
          />
        ) : undefined}
      />

      {view === "overview" && (
        <>
          <div className="mb-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <MetricCard
              label="Stock records"
              value={activeWarehouseId ? stock.data?.totalCount ?? "—" : "—"}
              note={activeWarehouseId ? "Connected warehouse · live API" : "Connect with a warehouse ID"}
              icon={Boxes}
            />
            <MetricCard
              label="Low stock lines"
              value={!activeWarehouseId || stock.isLoading || stock.isError ? "—" : lowStock}
              note="Available quantity ≤ 5 · loaded page"
              icon={Activity}
              tone={lowStock > 0 ? "red" : "green"}
            />
            <MetricCard
              label="Batches nearing expiry"
              value={batches.isLoading || batches.isError ? "—" : soon}
              note="Within 60 days · loaded page"
              icon={Clock3}
              tone={soon > 0 ? "amber" : "green"}
            />
            <MetricCard
              label="Batch records"
              value={batches.isLoading || batches.isError ? "—" : batches.data?.totalCount ?? "—"}
              note="Live inventory API total"
              icon={PackageCheck}
            />
          </div>

          {!activeWarehouseId && (
            <div className="mb-5 rounded-xl border border-amber-500/25 bg-amber-500/5 px-4 py-3 text-xs leading-5 text-amber-900 dark:text-amber-200">
              Stock queries need a warehouse ID. The warehouse list does not include IDs, so connect a UUID from warehouse provisioning on the Stock page.
            </div>
          )}

          {activeWarehouseId && (
            <StockTable
              page={1}
              setPage={setPage}
              warehouseId={activeWarehouseId}
              setSelectedStockId={setCurrentStockId}
            />
          )}

          <div className="mt-5">
            <BatchTable
              page={1}
              setPage={() => router.push("/inventory/batches")}
              batchId={statusBatchId}
              setBatchId={setStatusBatchId}
            />
          </div>

          {!isAdmin && (
            <div className="mt-4 rounded-xl border bg-muted/30 px-4 py-3 text-xs text-muted-foreground">
              Warehouse staff can view batch and movement APIs. Warehouse selection is limited by the warehouse directory API policy.
            </div>
          )}
        </>
      )}

      {view === "stock" && (
        activeWarehouseId ? (
          <StockTable
            page={page}
            setPage={setPage}
            warehouseId={activeWarehouseId}
            setSelectedStockId={setCurrentStockId}
          />
        ) : (
          <Surface>
            <SectionTitle
              title="Connect a warehouse"
              description={isAdmin
                ? "Use the warehouse ID returned when the location was created. Enter it in the connection control above."
                : "Warehouse IDs are not available to this role through the current backend API."}
            />
            <p className="px-5 py-8 text-sm leading-6 text-muted-foreground">
              {isAdmin
                ? "Once connected, stock is grouped by product SKU and batch, with on-hand, reserved, and available quantities."
                : "Ask an administrator to provide the warehouse UUID assigned to your operations team."}
            </p>
          </Surface>
        )
      )}

      {view === "batches" && (
        <>
          <BatchTable
            page={page}
            setPage={setPage}
            batchId={statusBatchId}
            setBatchId={setStatusBatchId}
          />
          <CreateBatchDialog
            open={createBatchOpen}
            onOpenChange={setCreateBatchOpen}
            onCreated={(id) => {
              setLastCreatedBatchId(id);
              setStatusBatchId(id);
            }}
          />
          {lastCreatedBatchId && (
            <IdentifierNotice
              label="New batch ID"
              value={lastCreatedBatchId}
              onCopy={() => void copyIdentifier(lastCreatedBatchId, "Batch ID")}
            />
          )}
          <div className="mt-4 rounded-xl border bg-muted/30 px-4 py-3 text-xs leading-5 text-muted-foreground">
            The batch list omits UUIDs. This page keeps IDs returned for batches created during this session; use the original create response for older batches.
          </div>
        </>
      )}

      {view === "movements" && (
        <MovementsPanel initialStockId={initialStockId || activeStockId} />
      )}

      {view === "warehouses" && (
        isAdmin ? (
          <>
            <WarehouseTable
              page={page}
              setPage={setPage}
              onCreated={(id) => {
                setActiveWarehouseId(id);
                setLastCreatedWarehouseId(id);
              }}
            />
            {lastCreatedWarehouseId && (
              <IdentifierNotice
                label="New warehouse ID"
                value={lastCreatedWarehouseId}
                onCopy={() => void copyIdentifier(lastCreatedWarehouseId, "Warehouse ID")}
              />
            )}
          </>
        ) : (
          <Surface>
            <SectionTitle
              title="Warehouse directory unavailable"
              description="The current API limits the warehouse directory to administrators."
            />
            <p className="px-5 py-8 text-sm leading-6 text-muted-foreground">
              Your role can use inventory operations but cannot call the admin-only warehouse listing API. An administrator can provide the assigned warehouse ID to connect the stock workspace.
            </p>
          </Surface>
        )
      )}

      {view === "replenishment" && (
        <CapabilityPage
          eyebrow="Inventory planning"
          title="Replenishment"
          description="Plan and approve replenishment orders from live stock signals."
          apiArea="Replenishment recommendations and purchase orders"
        />
      )}
    </>
  );
}

function IdentifierNotice({
  label,
  value,
  onCopy,
}: {
  label: string;
  value: string;
  onCopy: () => void;
}) {
  return (
    <div className="mt-4 flex flex-wrap items-center gap-2 rounded-xl border border-primary/20 bg-primary/5 px-4 py-3 text-xs">
      <span className="font-medium">{label}</span>
      <code className="select-all font-mono text-muted-foreground">{value}</code>
      <Button size="xs" variant="outline" onClick={onCopy}>Copy ID</Button>
    </div>
  );
}
