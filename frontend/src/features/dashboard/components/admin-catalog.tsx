"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowUpRight, PackageSearch } from "lucide-react";
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
import { catalogApi } from "@/features/catalog/api/catalog.api";
import {
  productTypes,
  type CatalogCategory,
  type CatalogProduct,
  type PagedResult,
} from "@/features/catalog/types/catalog.types";
import { queryKeys } from "@/lib/query/query-keys";
import { cn } from "@/lib/utils";
import {
  PageControls,
  PageHeading,
  QueryState,
  SearchField,
  SectionTitle,
  StatusPill,
  Surface,
} from "./dashboard-primitives";
import { AdminProductWizard } from "./admin-product-wizard";

type CatalogReferenceKind = "brand" | "manufacturer" | "category";

const buildReferenceSchema = (isCategory: boolean) => z.object({
  name: z.string().trim().min(2, "Enter at least 2 characters.").max(150),
  productType: z.string().optional(),
  sortOrder: z.string().optional(),
}).superRefine((value, context) => {
  if (isCategory && !value.productType) {
    context.addIssue({
      code: "custom",
      path: ["productType"],
      message: "Choose a product type.",
    });
  }

  if (isCategory && !/^(0|[1-9]\d*)$/.test(value.sortOrder ?? "")) {
    context.addIssue({
      code: "custom",
      path: ["sortOrder"],
      message: "Enter a whole number of 0 or more.",
    });
  }
});

type ReferenceValues = z.infer<ReturnType<typeof buildReferenceSchema>>;

function useAdminCatalog(page: number, search: string) {
  const params = { page, ...(search ? { search } : {}) };

  return useQuery<PagedResult<CatalogProduct>>({
    queryKey: queryKeys.catalog.products({ ...params, admin: true }),
    queryFn: () => catalogApi.getCatalogProducts(params),
  });
}

function CreateReferenceDialog({
  kind,
  open,
  onOpenChange,
  onCreate,
  busy,
}: {
  kind: CatalogReferenceKind;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreate: (value: ReferenceValues) => Promise<void>;
  busy: boolean;
}) {
  const form = useForm<ReferenceValues>({
    resolver: zodResolver(buildReferenceSchema(kind === "category")),
    defaultValues: { name: "", productType: "", sortOrder: "0" },
  });
  const label = kind;

  const submit = form.handleSubmit(async (value) => {
    try {
      await onCreate(value);
      form.reset();
      onOpenChange(false);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : `Could not create ${label}.`);
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add {label}</DialogTitle>
          <DialogDescription>This creates a live catalog record through the admin API.</DialogDescription>
        </DialogHeader>

        <form onSubmit={submit} className="space-y-3">
          <label className="grid gap-1.5 text-xs font-medium">
            Name
            <Input {...form.register("name")} />
            {form.formState.errors.name && (
              <span className="font-normal text-destructive">{form.formState.errors.name.message}</span>
            )}
          </label>

          {kind === "category" && (
            <>
              <label className="grid gap-1.5 text-xs font-medium">
                Product type
                <select
                  className="h-9 rounded-lg border border-input bg-background px-3 text-sm outline-none focus:ring-2 focus:ring-ring"
                  {...form.register("productType")}
                >
                  <option value="">Choose a type</option>
                  {productTypes.map((item, index) => (
                    <option key={item.apiType} value={String(index + 1)}>{item.label}</option>
                  ))}
                </select>
                {form.formState.errors.productType && (
                  <span className="font-normal text-destructive">{form.formState.errors.productType.message}</span>
                )}
              </label>

              <label className="grid gap-1.5 text-xs font-medium">
                Display order
                <Input type="number" min="0" step="1" {...form.register("sortOrder")} />
                {form.formState.errors.sortOrder && (
                  <span className="font-normal text-destructive">{form.formState.errors.sortOrder.message}</span>
                )}
              </label>
            </>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" disabled={busy} onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={busy}>
              {busy ? "Creating…" : `Create ${label}`}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function flattenCategories(items: CatalogCategory[]): CatalogCategory[] {
  return items.flatMap((item) => [item, ...flattenCategories(item.children ?? [])]);
}

export function AdminCatalog() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [createKind, setCreateKind] = useState<CatalogReferenceKind | null>(null);
  const [productWizardOpen, setProductWizardOpen] = useState(false);
  const query = useAdminCatalog(page, search.trim());
  const queryClient = useQueryClient();
  const brands = useQuery({ queryKey: queryKeys.catalog.brands, queryFn: catalogApi.getBrands });
  const manufacturers = useQuery({ queryKey: queryKeys.catalog.manufacturers, queryFn: catalogApi.getManufacturers });
  const categories = useQuery({ queryKey: queryKeys.catalog.categories, queryFn: catalogApi.getCategories });

  const createReference = useMutation({
    mutationFn: async ({ kind, value }: { kind: CatalogReferenceKind; value: ReferenceValues }) => {
      if (kind === "brand") return catalogApi.createBrand(value.name);
      if (kind === "manufacturer") return catalogApi.createManufacturer(value.name);

      return catalogApi.createCategory({
        productType: Number(value.productType),
        name: value.name,
        parentCategoryId: null,
        sortOrder: Number(value.sortOrder),
      });
    },
    onSuccess: (_result, variables) => {
      const queryKey = variables.kind === "brand"
        ? queryKeys.catalog.brands
        : variables.kind === "manufacturer"
          ? queryKeys.catalog.manufacturers
          : queryKeys.catalog.categories;

      void queryClient.invalidateQueries({ queryKey });
      toast.success(`${variables.kind[0].toUpperCase()}${variables.kind.slice(1)} created`);
    },
  });

  const onSearch = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  const references = [
    { kind: "brand" as const, label: "Brands", count: brands.data?.length, loading: brands.isLoading },
    { kind: "manufacturer" as const, label: "Manufacturers", count: manufacturers.data?.length, loading: manufacturers.isLoading },
    {
      kind: "category" as const,
      label: "Categories",
      count: categories.data ? flattenCategories(categories.data).length : undefined,
      loading: categories.isLoading,
    },
  ];

  return (
    <>
      <PageHeading
        eyebrow="Catalog management"
        title="Product catalog"
        description="Create products with variants and pricing, maintain catalog references, and review items exposed through the storefront catalog."
        action={(
          <div className="flex flex-wrap gap-2">
            <Button onClick={() => setProductWizardOpen(true)}>
              <PackageSearch className="mr-2 size-4" /> Add product
            </Button>
            <Link href="/otc/medicine" className={cn(buttonVariants({ variant: "outline" }), "gap-2")}>
              View storefront <ArrowUpRight className="size-4" />
            </Link>
          </div>
        )}
      />

      <Surface className="mb-5">
        <SectionTitle title="Catalog reference data" description="Manage the supported top-level catalog records." />
        <div className="grid gap-3 p-4 sm:p-5 md:grid-cols-3">
          {references.map((item) => (
            <article key={item.kind} className="rounded-xl border bg-background p-4">
              <div className="flex items-start justify-between gap-2">
                <div>
                  <p className="text-sm font-medium">{item.label}</p>
                  <p className="mt-1 text-2xl font-semibold">{item.loading ? "—" : item.count ?? "—"}</p>
                </div>
                <StatusPill variant="success">Live API</StatusPill>
              </div>
              <Button className="mt-4 w-full" size="sm" variant="outline" onClick={() => setCreateKind(item.kind)}>
                Add {item.kind}
              </Button>
            </article>
          ))}
        </div>
        {(brands.error || manufacturers.error || categories.error) && (
          <p className="px-5 pb-4 text-xs text-destructive">Some catalog references could not be loaded.</p>
        )}
        <CreateReferenceDialog
          key={createKind ?? "reference-dialog"}
          kind={createKind ?? "brand"}
          open={Boolean(createKind)}
          onOpenChange={(open) => !open && setCreateKind(null)}
          onCreate={(value) => createReference.mutateAsync({ kind: createKind ?? "brand", value }).then(() => undefined)}
          busy={createReference.isPending}
        />
      </Surface>

      <Surface>
        <SectionTitle
          title="Published catalog"
          description={query.data
            ? `${query.data.totalCount} results · page ${query.data.page} of ${query.data.totalPages}`
            : "Live data from /v1/catalog/products"}
          action={<SearchField value={search} onChange={onSearch} placeholder="Search product or brand" />}
        />
        <QueryState
          loading={query.isLoading}
          error={query.error}
          empty={query.data?.items.length === 0}
          onRetry={() => void query.refetch()}
        />

        {query.data?.items.length ? (
          <>
            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[780px] text-left text-sm">
                <thead className="bg-muted/40 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-5 py-3 font-semibold">Product</th>
                    <th className="px-5 py-3 font-semibold">SKU</th>
                    <th className="px-5 py-3 font-semibold">Brand</th>
                    <th className="px-5 py-3 font-semibold">Price</th>
                    <th className="px-5 py-3 font-semibold">Package</th>
                    <th className="px-5 py-3" />
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {query.data.items.map((product) => (
                    <tr key={product.productVariantId} className="transition-colors hover:bg-muted/20">
                      <td className="px-5 py-3.5">
                        <p className="max-w-[290px] truncate font-medium">{product.productName}</p>
                        <p className="mt-1 text-[11px] text-muted-foreground">/{product.productSlug}</p>
                      </td>
                      <td className="px-5 py-3.5 font-mono text-xs text-muted-foreground">{product.sku}</td>
                      <td className="px-5 py-3.5">{product.brandName || <span className="text-muted-foreground">—</span>}</td>
                      <td className="px-5 py-3.5 font-medium">₹{product.sellingPrice.toLocaleString("en-IN")}</td>
                      <td className="px-5 py-3.5 text-xs text-muted-foreground">{product.packageQuantity} {product.packageUnit}</td>
                      <td className="px-5 py-3.5">
                        <CatalogProductLinks product={product} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="divide-y md:hidden">
              {query.data.items.map((product) => (
                <article key={product.productVariantId} className="space-y-3 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <h3 className="truncate text-sm font-semibold">{product.productName}</h3>
                      <p className="mt-1 font-mono text-[10px] text-muted-foreground">{product.sku}</p>
                    </div>
                    <p className="shrink-0 text-sm font-semibold">₹{product.sellingPrice.toLocaleString("en-IN")}</p>
                  </div>
                  <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
                    <span>{product.brandName || "No brand"}</span>
                    <span>{product.packageQuantity} {product.packageUnit}</span>
                  </div>
                  <div className="border-t pt-3"><CatalogProductLinks product={product} /></div>
                </article>
              ))}
            </div>

            <PageControls page={query.data.page} totalPages={query.data.totalPages} onChange={setPage} />
          </>
        ) : null}
      </Surface>

      <div className="mt-4 flex items-start gap-3 rounded-xl border bg-muted/30 px-4 py-3 text-xs leading-5 text-muted-foreground">
        <PackageSearch className="mt-0.5 size-4 shrink-0 text-primary" />
        <p>The current catalog endpoint returns storefront-visible variants. Unpublished products are not returned by this API.</p>
      </div>

      <AdminProductWizard open={productWizardOpen} onOpenChange={setProductWizardOpen} />
    </>
  );
}

function CatalogProductLinks({ product }: { product: CatalogProduct }) {
  const manageHref = `/admin/catalog/${encodeURIComponent(product.productSlug)}/${encodeURIComponent(product.sku)}`;
  const storefrontHref = `/products/${encodeURIComponent(product.productSlug)}/${encodeURIComponent(product.sku)}`;

  return (
    <div className="flex items-center gap-3">
      <Link href={manageHref} className="inline-flex items-center gap-1 text-xs font-medium text-primary">
        Manage <ArrowUpRight className="size-3.5" />
      </Link>
      <Link href={storefrontHref} className="text-xs text-muted-foreground hover:text-foreground">
        Storefront
      </Link>
    </div>
  );
}
