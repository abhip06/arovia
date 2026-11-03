"use client";

import Link from "next/link";
import {
  Activity,
  AlertTriangle,
  ArrowRight,
  Boxes,
  CalendarClock,
  ChartNoAxesCombined,
  CircleDollarSign,
  ClipboardList,
  CreditCard,
  PackageCheck,
  Users,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { useCatalogProducts } from "@/features/catalog/hooks/use-catalog";
import { useAdminOrders } from "@/features/orders/hooks/use-orders";
import { cn } from "@/lib/utils";
import { useAdminPayments } from "../hooks/use-admin-payments";
import { useAdminUsers } from "../hooks/use-admin-users";
import { useInventoryBatches, useWarehouseStock } from "../hooks/use-operations";
import { useDashboardUiStore } from "../stores/dashboard-ui.store";
import {
  MetricCard,
  PageHeading,
  QueryState,
  SectionTitle,
  StatusPill,
  Surface,
} from "./dashboard-primitives";

interface WorkspaceLink {
  title: string;
  source: string;
  note: string;
  icon: LucideIcon;
  href: string;
}

function dateLabel(value: string | null) {
  if (!value) return "No expiry date recorded";

  const date = new Date(`${value}T00:00:00`);
  if (Number.isNaN(date.getTime())) return "Expiry date unavailable";

  return `Expires ${date.toLocaleDateString("en-IN", { dateStyle: "medium" })}`;
}

export function AdminOverview() {
  const products = useCatalogProducts({ page: 1 });
  const batches = useInventoryBatches(1);
  const activeWarehouseId = useDashboardUiStore((state) => state.activeWarehouseId);
  const stock = useWarehouseStock(activeWarehouseId, 1, Boolean(activeWarehouseId));
  const placedOrders = useAdminOrders({ page: 1, pageSize: 1, status: "Placed" });
  const confirmedOrders = useAdminOrders({ page: 1, pageSize: 1, status: "Confirmed" });
  const payments = useAdminPayments(1);
  const users = useAdminUsers();

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const expiryCutoff = new Date(today);
  expiryCutoff.setDate(expiryCutoff.getDate() + 60);
  const batchItems = batches.data?.items ?? [];
  const expiringBatches = batchItems.filter((batch) => {
    if (!batch.expiryDate) return false;
    const expiry = new Date(`${batch.expiryDate}T00:00:00`);
    return expiry >= today && expiry <= expiryCutoff;
  });
  const flaggedBatches = batchItems.filter((batch) => {
    if (/hold|expired|quarantin|recall/i.test(batch.status)) return true;
    if (!batch.expiryDate) return false;

    return new Date(`${batch.expiryDate}T00:00:00`) < today;
  });
  const lowStockLines = (stock.data?.items ?? []).filter((item) => item.availableQuantity <= 5).length;
  const orderError = placedOrders.error ?? confirmedOrders.error;
  const orderCount = placedOrders.isError || confirmedOrders.isError
    ? null
    : (placedOrders.data?.totalCount ?? 0) + (confirmedOrders.data?.totalCount ?? 0);
  const alerts = [
    ...flaggedBatches.slice(0, 3),
    ...expiringBatches.filter((batch) => !flaggedBatches.includes(batch)).slice(0, 3),
  ];

  const workspaces: WorkspaceLink[] = [
    {
      title: "Orders",
      source: "Live admin API",
      note: `${orderCount ?? "—"} placed or confirmed`,
      icon: ClipboardList,
      href: "/admin/orders",
    },
    {
      title: "Catalog",
      source: "Live catalog API",
      note: products.data ? `${products.data.totalCount} published products` : "Products, variants & prices",
      icon: PackageCheck,
      href: "/admin/catalog",
    },
    {
      title: "Inventory",
      source: "Live stock APIs",
      note: "Batches, warehouses & movements",
      icon: Activity,
      href: "/inventory",
    },
    {
      title: "Payments",
      source: "Live admin payment ledger",
      note: payments.data ? `${payments.data.totalCount} payment records` : "Paged transaction history",
      icon: CreditCard,
      href: "/admin/payments",
    },
    {
      title: "Users & staff",
      source: "Live admin directory",
      note: users.data ? `${users.data.totalCount} accounts` : "Roles and verification status",
      icon: Users,
      href: "/admin/users",
    },
  ];

  return (
    <>
      <PageHeading
        eyebrow="Platform overview"
        title="Operations overview"
        description="A live snapshot of orders, catalog, payments, accounts, and inventory from connected Arovia APIs."
        action={<span className="rounded-full border bg-card px-3 py-1.5 text-xs text-muted-foreground">Live backend data</span>}
      />

      <div className="mb-6 grid gap-3 sm:grid-cols-2 xl:grid-cols-4 2xl:grid-cols-7">
        <MetricCard
          label="Orders needing action"
          value={placedOrders.isLoading || confirmedOrders.isLoading ? "—" : orderCount ?? "—"}
          note={orderError ? "Orders API unavailable" : "Placed + confirmed orders"}
          icon={ClipboardList}
          tone={orderCount === null || (orderCount ?? 0) > 0 ? "amber" : "green"}
        />
        <MetricCard
          label="Inventory alerts"
          value={batches.isLoading || batches.isError ? "—" : flaggedBatches.length}
          note={batches.isError ? "Inventory API unavailable" : "Flagged batches · loaded page"}
          icon={Boxes}
          tone={batches.isError || flaggedBatches.length ? "amber" : "green"}
        />
        <MetricCard
          label="Low stock lines"
          value={!activeWarehouseId || stock.isLoading || stock.isError ? "—" : lowStockLines}
          note={activeWarehouseId ? "Available quantity ≤ 5 · loaded page" : "Connect a warehouse in Inventory"}
          icon={Boxes}
          tone={lowStockLines > 0 ? "red" : "green"}
        />
        <MetricCard
          label="Batches nearing expiry"
          value={batches.isLoading || batches.isError ? "—" : expiringBatches.length}
          note={batches.isError ? "Inventory API unavailable" : "Within 60 days · loaded page"}
          icon={CalendarClock}
          tone={batches.isError || expiringBatches.length ? "amber" : "green"}
        />
        <MetricCard
          label="Published products"
          value={products.isLoading || products.isError ? "—" : products.data?.totalCount ?? "—"}
          note="Storefront-visible catalog records"
          icon={PackageCheck}
        />
        <MetricCard
          label="Payment records"
          value={payments.isLoading || payments.isError ? "—" : payments.data?.totalCount ?? "—"}
          note="Total from paged admin ledger"
          icon={CircleDollarSign}
          tone="green"
        />
        <MetricCard
          label="Accounts"
          value={users.isLoading || users.isError ? "—" : users.data?.totalCount ?? "—"}
          note="Users and operational staff"
          icon={Users}
        />
      </div>

      {orderError && (
        <div className="mb-5 rounded-xl border border-destructive/20 bg-destructive/5 px-4 py-3 text-xs leading-5 text-destructive">
          The order summary could not be loaded: {orderError instanceof Error ? orderError.message : "The orders API request failed."}
        </div>
      )}

      <div className="grid gap-5 xl:grid-cols-[1.1fr_0.9fr]">
        <Surface>
          <SectionTitle
            title="Inventory attention"
            description="Live batch status and expiry data from the current API page."
            action={<Link href="/inventory/batches" className="inline-flex items-center gap-1 text-xs font-medium text-primary">Open batches <ArrowRight className="size-3.5" /></Link>}
          />
          <QueryState loading={batches.isLoading} error={batches.error} onRetry={() => void batches.refetch()} />
          {!batches.isLoading && !batches.error && (
            <div className="divide-y">
              {alerts.length === 0 ? (
                <div className="px-5 py-9 text-center text-sm text-muted-foreground">
                  No expiry or batch status alerts in the loaded page.
                </div>
              ) : alerts.map((batch) => {
                const urgent = /expired|hold|quarantin|recall/i.test(batch.status);

                return (
                  <div key={`${batch.sku}-${batch.batchNumber}`} className="flex flex-wrap items-center justify-between gap-3 px-4 py-3.5 sm:px-5">
                    <div className="flex min-w-0 items-center gap-3">
                      <span className={`grid size-9 shrink-0 place-items-center rounded-xl ${urgent ? "bg-destructive/10 text-destructive" : "bg-amber-500/10 text-amber-700 dark:text-amber-300"}`}>
                        <AlertTriangle className="size-4" />
                      </span>
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium">
                          {batch.sku} <span className="font-normal text-muted-foreground">· lot {batch.batchNumber}</span>
                        </p>
                        <p className="mt-0.5 text-xs text-muted-foreground">{dateLabel(batch.expiryDate)}</p>
                      </div>
                    </div>
                    <StatusPill variant={urgent ? "danger" : "warning"}>{batch.status}</StatusPill>
                  </div>
                );
              })}
            </div>
          )}
        </Surface>

        <Surface>
          <SectionTitle title="Workspaces" description="Open an operational area." />
          <div className="grid gap-2 p-4 sm:grid-cols-2 xl:grid-cols-1">
            {workspaces.map(({ title, source, note, icon: Icon, href }) => (
              <Link key={title} href={href} className="group flex items-center gap-3 rounded-xl border bg-background p-3 transition-colors hover:border-primary/30 hover:bg-muted/40">
                <span className="grid size-9 shrink-0 place-items-center rounded-lg bg-primary/5 text-primary">
                  <Icon className="size-4" />
                </span>
                <span className="min-w-0 flex-1">
                  <span className="block text-sm font-medium">{title}</span>
                  <span className="mt-0.5 block truncate text-[11px] text-muted-foreground">{source} · {note}</span>
                </span>
                <ArrowRight className="size-4 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
              </Link>
            ))}
            <Link href="/admin/reports" className="group flex items-center gap-3 rounded-xl border border-dashed bg-background p-3 text-muted-foreground transition-colors hover:border-primary/30 hover:bg-muted/40 hover:text-foreground">
              <span className="grid size-9 shrink-0 place-items-center rounded-lg bg-muted"><ChartNoAxesCombined className="size-4" /></span>
              <span className="min-w-0 flex-1">
                <span className="block text-sm font-medium">Reports</span>
                <span className="mt-0.5 block text-[11px]">Reporting API is not yet available</span>
              </span>
              <ArrowRight className="size-4" />
            </Link>
          </div>
          <div className="mx-4 mb-4 flex items-start gap-2 rounded-lg bg-muted/60 p-3 text-xs leading-5 text-muted-foreground">
            <CalendarClock className="mt-0.5 size-4 shrink-0" />
            Expiry awareness supports FEFO handling. Validate lot expiry and sellable-until dates before receiving or allocating stock.
          </div>
        </Surface>
      </div>

      <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-primary/15 bg-primary/[0.035] px-4 py-3 text-xs text-muted-foreground">
        <span>Inventory alert counts use the current batch page. Order counts query all matching backend records.</span>
        <Link href="/admin/orders" className={cn(buttonVariants({ size: "sm", variant: "outline" }), "gap-1.5")}>
          <ClipboardList className="size-3.5" /> Review orders
        </Link>
      </div>
    </>
  );
}
