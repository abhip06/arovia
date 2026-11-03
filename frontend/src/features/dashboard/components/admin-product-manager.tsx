"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowLeft, PackagePlus, Plus, Tag } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useProductDetail } from "@/features/catalog/hooks/use-product-detail";
import { queryKeys } from "@/lib/query/query-keys";
import { cn } from "@/lib/utils";
import { adminCatalogApi, type ProductPriceRecord } from "../api/admin-catalog.api";
import { PageHeading, QueryState, SectionTitle, StatusPill, Surface } from "./dashboard-primitives";

const priceSchema = z.object({
  sellingPrice: z.string().refine(
    (value) => Number.isFinite(Number(value)) && Number(value) >= 0.01,
    "Enter a price greater than zero.",
  ),
  effectiveFrom: z.string().optional(),
});

const variantSchema = z.object({
  barcode: z.string().trim().max(80).optional(),
  packageQuantity: z.string().refine(
    (value) => Number.isFinite(Number(value)) && Number(value) > 0,
    "Enter a package quantity greater than zero.",
  ),
  packageUnit: z.string().trim().min(1, "Enter a package unit."),
  netQuantity: z.string().optional().refine(
    (value) => !value || (Number.isFinite(Number(value)) && Number(value) > 0),
    "Enter a net quantity greater than zero.",
  ),
  netQuantityUnit: z.string().trim().max(40).optional(),
});

type PriceValues = z.infer<typeof priceSchema>;
type VariantValues = z.infer<typeof variantSchema>;
type PriceTarget = { sku: string; id: string };
type CatalogAction = {
  target: "product" | "variant";
  id: string;
  action: "activate" | "deactivate" | "discontinue";
  label: string;
};

function PriceDialog({
  open,
  onOpenChange,
  productId,
  variantId,
  onCreated,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  productId: string;
  variantId: string;
  onCreated: () => void;
}) {
  const queryClient = useQueryClient();
  const form = useForm<PriceValues>({
    resolver: zodResolver(priceSchema),
    defaultValues: { sellingPrice: "", effectiveFrom: "" },
  });
  const create = useMutation({
    mutationFn: (value: PriceValues) => adminCatalogApi.createPrice(productId, variantId, {
      sellingPrice: Number(value.sellingPrice),
      effectiveFrom: value.effectiveFrom ? new Date(value.effectiveFrom).toISOString() : null,
    }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.catalog.priceHistory(variantId) });
      void queryClient.invalidateQueries({ queryKey: queryKeys.catalog.products({}) });
      toast.success("Price scheduled");
      onCreated();
    },
    onError: (error) => toast.error(error.message || "Could not add the price."),
  });

  const submit = form.handleSubmit(async (value) => {
    try {
      await create.mutateAsync(value);
      form.reset();
      onOpenChange(false);
    } catch {
      // The mutation handler reports the API error.
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add selling price</DialogTitle>
          <DialogDescription>Schedule a price for this product variant through the admin API.</DialogDescription>
        </DialogHeader>
        <form onSubmit={submit} className="space-y-3">
          <label className="grid gap-1.5 text-xs font-medium">
            Selling price (₹)
            <Input type="number" min="0.01" step="0.01" {...form.register("sellingPrice")} />
            {form.formState.errors.sellingPrice && (
              <span className="font-normal text-destructive">{form.formState.errors.sellingPrice.message}</span>
            )}
          </label>
          <label className="grid gap-1.5 text-xs font-medium">
            Effective from (optional)
            <Input type="datetime-local" {...form.register("effectiveFrom")} />
          </label>
          <DialogFooter>
            <Button type="button" variant="outline" disabled={create.isPending} onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={create.isPending}>
              {create.isPending ? "Saving…" : "Save price"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function VariantDialog({
  open,
  onOpenChange,
  productId,
  onCreated,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  productId: string;
  onCreated: (sku: string, id: string) => void;
}) {
  const queryClient = useQueryClient();
  const form = useForm<VariantValues>({
    resolver: zodResolver(variantSchema),
    defaultValues: {
      barcode: "",
      packageQuantity: "1",
      packageUnit: "piece",
      netQuantity: "",
      netQuantityUnit: "",
    },
  });
  const create = useMutation({
    mutationFn: (value: VariantValues) => adminCatalogApi.createVariant(productId, {
      barcode: value.barcode?.trim() || null,
      packageQuantity: Number(value.packageQuantity),
      packageUnit: value.packageUnit.trim(),
      netQuantity: value.netQuantity ? Number(value.netQuantity) : null,
      netQuantityUnit: value.netQuantityUnit?.trim() || null,
    }),
    onSuccess: (created) => {
      void queryClient.invalidateQueries({ queryKey: ["catalog", "products"] });
      toast.success(`Variant ${created.sku} created`);
      onCreated(created.sku, created.id);
    },
    onError: (error) => toast.error(error.message || "Could not add the variant."),
  });

  const submit = form.handleSubmit(async (value) => {
    try {
      await create.mutateAsync(value);
      form.reset();
      onOpenChange(false);
    } catch {
      // The mutation handler reports the API error.
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Add product variant</DialogTitle>
          <DialogDescription>Create a new SKU with package details.</DialogDescription>
        </DialogHeader>
        <form onSubmit={submit} className="grid gap-3 sm:grid-cols-2">
          <label className="grid gap-1.5 text-xs font-medium">
            Barcode (optional)
            <Input {...form.register("barcode")} />
          </label>
          <label className="grid gap-1.5 text-xs font-medium">
            Package quantity
            <Input type="number" min="0.01" step="0.01" {...form.register("packageQuantity")} />
            {form.formState.errors.packageQuantity && (
              <span className="font-normal text-destructive">{form.formState.errors.packageQuantity.message}</span>
            )}
          </label>
          <label className="grid gap-1.5 text-xs font-medium">
            Package unit
            <Input placeholder="bottle, tablet, box" {...form.register("packageUnit")} />
            {form.formState.errors.packageUnit && (
              <span className="font-normal text-destructive">{form.formState.errors.packageUnit.message}</span>
            )}
          </label>
          <label className="grid gap-1.5 text-xs font-medium">
            Net quantity (optional)
            <Input type="number" min="0" step="0.01" {...form.register("netQuantity")} />
            {form.formState.errors.netQuantity && (
              <span className="font-normal text-destructive">{form.formState.errors.netQuantity.message}</span>
            )}
          </label>
          <label className="grid gap-1.5 text-xs font-medium sm:col-span-2">
            Net quantity unit
            <Input placeholder="ml, g, pieces" {...form.register("netQuantityUnit")} />
          </label>
          <div className="sm:col-span-2">
            <DialogFooter>
              <Button type="button" variant="outline" disabled={create.isPending} onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={create.isPending}>
                {create.isPending ? "Creating…" : "Create variant"}
              </Button>
            </DialogFooter>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function formatPriceDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Date unavailable";
  return date.toLocaleString("en-IN");
}

function PriceHistory({ prices }: { prices: ProductPriceRecord[] | undefined }) {
  return (
    <div className="divide-y">
      {prices?.map((price) => (
        <div key={price.id} className="flex items-center justify-between gap-3 py-3 first:border-0">
          <div>
            <p className="text-sm font-semibold">₹{price.sellingPrice.toLocaleString("en-IN")}</p>
            <p className="mt-1 text-[10px] text-muted-foreground">Effective {formatPriceDate(price.effectiveFrom)}</p>
          </div>
          <StatusPill variant={price.isCurrent ? "success" : price.isFuture ? "warning" : "muted"}>
            {price.isCurrent ? "Current" : price.isFuture ? "Scheduled" : "Past"}
          </StatusPill>
        </div>
      ))}
    </div>
  );
}

export function AdminProductManager({ slug, sku }: { slug: string; sku: string }) {
  const detail = useProductDetail(slug, sku);
  const product = detail.data?.product;
  const variant = detail.data?.variant;
  const [priceOpen, setPriceOpen] = useState(false);
  const [variantOpen, setVariantOpen] = useState(false);
  const [createdVariant, setCreatedVariant] = useState<PriceTarget | null>(null);
  const [priceTarget, setPriceTarget] = useState<PriceTarget | null>(null);
  const [pendingAction, setPendingAction] = useState<CatalogAction | null>(null);
  const queryClient = useQueryClient();

  const prices = useQuery({
    queryKey: queryKeys.catalog.priceHistory(variant?.id ?? ""),
    queryFn: () => adminCatalogApi.productPrices(variant!.id),
    enabled: Boolean(variant?.id),
  });
  const createdVariantPrices = useQuery({
    queryKey: queryKeys.catalog.priceHistory(createdVariant?.id ?? ""),
    queryFn: () => adminCatalogApi.productPrices(createdVariant!.id),
    enabled: Boolean(createdVariant?.id),
  });
  const refreshCatalog = () => {
    void queryClient.invalidateQueries({ queryKey: queryKeys.catalog.productDetail(slug, sku) });
    void queryClient.invalidateQueries({ queryKey: ["catalog", "products"] });
  };

  const updateStatus = useMutation({
    mutationFn: ({ target, id, action }: CatalogAction) => target === "product"
      ? adminCatalogApi.setProductStatus(id, action)
      : adminCatalogApi.setVariantStatus(id, action),
    onSuccess: () => {
      refreshCatalog();
      toast.success("Catalog status updated");
      setPendingAction(null);
    },
    onError: (error) => toast.error(error.message || "Catalog status update failed."),
  });

  const requestStatusChange = (
    target: CatalogAction["target"],
    id: string,
    action: CatalogAction["action"],
    label: string,
  ) => setPendingAction({ target, id, action, label });

  const confirmStatusChange = async () => {
    if (!pendingAction) return;

    try {
      await updateStatus.mutateAsync(pendingAction);
    } catch {
      // The mutation handler reports the API error.
    }
  };

  return (
    <>
      <PageHeading
        eyebrow="Catalog management"
        title={product?.name ?? "Product details"}
        description={product
          ? `${product.type} · ${product.manufacturer.name}${product.brand ? ` · ${product.brand.name}` : ""}`
          : "Review product details and supported catalog actions."}
        action={(
          <Link href="/admin/catalog" className={cn(buttonVariants({ variant: "outline" }), "gap-2")}>
            <ArrowLeft className="size-4" /> Back to catalog
          </Link>
        )}
      />

      <QueryState loading={detail.isLoading} error={detail.error} onRetry={() => void detail.refetch()} />

      {product && variant && (
        <>
          <div className="grid gap-5 xl:grid-cols-[1fr_0.8fr]">
            <Surface>
              <SectionTitle
                title="Product & variant"
                description="Current detail API record"
                action={(
                  <div className="flex items-center gap-2">
                    <StatusPill variant="success">Storefront visible</StatusPill>
                    <StatusPill>{variant.sku}</StatusPill>
                  </div>
                )}
              />
              <div className="grid gap-3 p-4 sm:grid-cols-2 sm:p-5">
                {[
                  ["Product type", product.type],
                  ["Manufacturer", product.manufacturer.name],
                  ["Brand", product.brand?.name ?? "Not assigned"],
                  ["Prescription required", product.requiresPrescription ? "Yes" : "No"],
                  ["Returnable", product.returnable ? `Yes · ${product.returnWindowDays} days` : "No"],
                  ["Current SKU", variant.sku],
                  ["Package", `${variant.packageQuantity} ${variant.packageUnit}`],
                  ["Selling price", `₹${variant.sellingPrice.toLocaleString("en-IN")}`],
                ].map(([label, value]) => (
                  <div key={label} className="rounded-lg border bg-background px-3 py-2.5">
                    <p className="text-[10px] uppercase tracking-wide text-muted-foreground">{label}</p>
                    <p className="mt-1 text-sm font-medium">{value}</p>
                  </div>
                ))}
              </div>
              <div className="flex flex-wrap gap-2 border-t p-4">
                <Button size="sm" variant="outline" disabled={updateStatus.isPending} onClick={() => requestStatusChange("product", product.id, "deactivate", "product")}>
                  Deactivate product
                </Button>
                <Button size="sm" variant="outline" disabled={updateStatus.isPending} onClick={() => requestStatusChange("product", product.id, "activate", "product")}>
                  Activate product
                </Button>
                <Button size="sm" variant="destructive" disabled={updateStatus.isPending} onClick={() => requestStatusChange("product", product.id, "discontinue", "product")}>
                  Discontinue product
                </Button>
              </div>
            </Surface>

            <Surface>
              <SectionTitle
                title="Variant controls"
                description="Price history and lifecycle actions for each SKU."
                action={(
                  <Button size="sm" variant="outline" onClick={() => setVariantOpen(true)}>
                    <PackagePlus className="mr-1.5 size-4" /> Add variant
                  </Button>
                )}
              />

              <div className="border-b p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-semibold">{variant.sku}</p>
                    <p className="mt-1 text-xs text-muted-foreground">{variant.barcode || "No barcode"}</p>
                  </div>
                  <Button size="sm" onClick={() => {
                    setPriceTarget({ id: variant.id, sku: variant.sku });
                    setPriceOpen(true);
                  }}>
                    <Plus className="mr-1.5 size-4" /> Add price
                  </Button>
                </div>
                <VariantActions
                  disabled={updateStatus.isPending}
                  onAction={(action) => requestStatusChange("variant", variant.id, action, variant.sku)}
                />
              </div>

              <div className="px-4 py-4">
                <p className="mb-3 text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
                  Price history · {variant.sku}
                </p>
                <QueryState
                  loading={prices.isLoading}
                  error={prices.error}
                  empty={!prices.isLoading && !prices.error && prices.data?.length === 0}
                  onRetry={() => void prices.refetch()}
                />
                <PriceHistory prices={prices.data} />
              </div>

              {createdVariant && (
                <div className="border-t p-4">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div>
                      <p className="text-sm font-semibold">New SKU · {createdVariant.sku}</p>
                      <code className="mt-1 block select-all font-mono text-[10px] text-muted-foreground">
                        ID {createdVariant.id}
                      </code>
                    </div>
                    <Button size="sm" onClick={() => {
                      setPriceTarget(createdVariant);
                      setPriceOpen(true);
                    }}>
                      <Plus className="mr-1 size-3.5" /> Add price
                    </Button>
                  </div>
                  <VariantActions
                    disabled={updateStatus.isPending}
                    onAction={(action) => requestStatusChange("variant", createdVariant.id, action, createdVariant.sku)}
                  />
                  <div className="mt-3">
                    <QueryState
                      loading={createdVariantPrices.isLoading}
                      error={createdVariantPrices.error}
                      empty={!createdVariantPrices.isLoading && !createdVariantPrices.error && createdVariantPrices.data?.length === 0}
                      onRetry={() => void createdVariantPrices.refetch()}
                    />
                    <PriceHistory prices={createdVariantPrices.data} />
                  </div>
                </div>
              )}
            </Surface>
          </div>

          <div className="mt-5 rounded-xl border bg-muted/30 px-4 py-3 text-xs leading-5 text-muted-foreground">
            <Tag className="mr-2 inline size-3.5 text-primary" />
            Reference list APIs provide names and slugs but omit IDs. New product setup can create its manufacturer and optional brand inline; existing reference IDs can be entered if supplied by an administrator.
          </div>

          {priceTarget && (
            <PriceDialog
              open={priceOpen}
              onOpenChange={setPriceOpen}
              productId={product.id}
              variantId={priceTarget.id}
              onCreated={() => {
                void prices.refetch();
                void createdVariantPrices.refetch();
              }}
            />
          )}
          <VariantDialog
            open={variantOpen}
            onOpenChange={setVariantOpen}
            productId={product.id}
            onCreated={(newSku, id) => setCreatedVariant({ sku: newSku, id })}
          />
        </>
      )}

      <Dialog open={Boolean(pendingAction)} onOpenChange={(open) => !open && !updateStatus.isPending && setPendingAction(null)}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Confirm {pendingAction?.action}?</DialogTitle>
            <DialogDescription>
              This will {pendingAction?.action} {pendingAction?.label} through the catalog admin API.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" disabled={updateStatus.isPending} onClick={() => setPendingAction(null)}>
              Cancel
            </Button>
            <Button
              variant={pendingAction?.action === "discontinue" ? "destructive" : "default"}
              disabled={!pendingAction || updateStatus.isPending}
              onClick={() => void confirmStatusChange()}
            >
              {updateStatus.isPending ? "Updating…" : "Confirm change"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

function VariantActions({
  disabled,
  onAction,
}: {
  disabled: boolean;
  onAction: (action: CatalogAction["action"]) => void;
}) {
  return (
    <div className="mt-4 flex flex-wrap gap-2">
      <Button size="sm" variant="outline" disabled={disabled} onClick={() => onAction("deactivate")}>
        Deactivate SKU
      </Button>
      <Button size="sm" variant="outline" disabled={disabled} onClick={() => onAction("activate")}>
        Activate SKU
      </Button>
      <Button size="sm" variant="destructive" disabled={disabled} onClick={() => onAction("discontinue")}>
        Discontinue SKU
      </Button>
    </div>
  );
}
