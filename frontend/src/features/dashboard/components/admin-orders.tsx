"use client";

import { useDeferredValue, useMemo, useState } from "react";
import {
  AlertTriangle,
  ArrowDownRight,
  ArrowRight,
  CheckCircle2,
  CircleCheck,
  Clock3,
  PackageCheck,
  RefreshCw,
  Search,
  Truck,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  useAdminOrderDetails,
  useAdminOrders,
  useCancelAdminOrder,
  useChangeOrderStatus,
} from "@/features/orders/hooks/use-orders";
import type {
  ChangeOrderStatusTarget,
  OrderStatus,
} from "@/types/order.types";
import {
  MetricCard,
  PageControls,
  PageHeading,
  QueryState,
  SectionTitle,
  StatusPill,
  Surface,
} from "./dashboard-primitives";

const PAGE_SIZE = 20;

const statusOptions: OrderStatus[] = [
  "Placed",
  "Confirmed",
  "Processing",
  "Shipped",
  "OutForDelivery",
  "Delivered",
  "Cancelled",
];

const orderProgress: OrderStatus[] = [
  "Placed",
  "Confirmed",
  "Processing",
  "Shipped",
  "OutForDelivery",
  "Delivered",
];

const nextStatus: Partial<Record<OrderStatus, ChangeOrderStatusTarget>> = {
  Placed: "Confirmed",
  Confirmed: "Processing",
  Processing: "Shipped",
  Shipped: "OutForDelivery",
  OutForDelivery: "Delivered",
};

type Confirmation = "status" | "cancel";

function formatStatus(status: string) {
  return status.replace(/([A-Z])/g, " $1").trim();
}

function money(amount: number, currency: string) {
  try {
    return new Intl.NumberFormat("en-IN", {
      style: "currency",
      currency: currency || "INR",
      maximumFractionDigits: 2,
    }).format(amount);
  } catch {
    return `${currency || "INR"} ${amount.toLocaleString("en-IN")}`;
  }
}

function dateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";

  return new Intl.DateTimeFormat("en-IN", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function orderStatusTone(
  status: string,
): "success" | "warning" | "danger" | "muted" | "default" {
  if (status === "Delivered") return "success";
  if (["Placed", "Confirmed", "Processing"].includes(status)) return "warning";
  if (status === "Cancelled") return "danger";
  if (["Shipped", "OutForDelivery"].includes(status)) return "default";
  return "muted";
}

function OrderProgress({ status }: { status: OrderStatus }) {
  const currentStep = orderProgress.indexOf(status);

  if (status === "Cancelled") {
    return (
      <div className="flex items-center gap-2 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
        <AlertTriangle className="size-4" />
        This order was cancelled.
      </div>
    );
  }

  return (
    <ol className="grid grid-cols-3 gap-x-2 gap-y-3 sm:grid-cols-6" aria-label="Order fulfillment progress">
      {orderProgress.map((step, index) => {
        const complete = index < currentStep;
        const active = index === currentStep;

        return (
          <li key={step} className="min-w-0">
            <div className="flex items-center">
              <span
                className={`grid size-7 shrink-0 place-items-center rounded-full border ${
                  complete
                    ? "border-emerald-600/30 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
                    : active
                      ? "border-primary bg-primary/10 text-primary"
                      : "border-border bg-background text-muted-foreground"
                }`}
              >
                {complete ? <CheckCircle2 className="size-4" /> : index === 3 ? <Truck className="size-3.5" /> : <span className="text-[10px]">{index + 1}</span>}
              </span>
              {index < orderProgress.length - 1 && (
                <span className={`ml-1 h-px flex-1 ${complete ? "bg-emerald-600/40" : "bg-border"}`} />
              )}
            </div>
            <p className={`mt-1.5 truncate text-[10px] ${active ? "font-semibold text-foreground" : "text-muted-foreground"}`}>
              {formatStatus(step)}
            </p>
          </li>
        );
      })}
    </ol>
  );
}

function SummaryRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-4 text-muted-foreground">
      <span>{label}</span>
      <span className="text-foreground">{value}</span>
    </div>
  );
}

function OrderDetailsDialog({
  orderId,
  onClose,
}: {
  orderId: string | null;
  onClose: () => void;
}) {
  const details = useAdminOrderDetails(orderId ?? "", Boolean(orderId));
  const changeStatus = useChangeOrderStatus();
  const cancelOrder = useCancelAdminOrder();
  const [confirmation, setConfirmation] = useState<Confirmation | null>(null);
  const order = details.data;
  const pending = changeStatus.isPending || cancelOrder.isPending;
  const targetStatus = order ? nextStatus[order.status] : undefined;

  const applyAction = async () => {
    if (!order || !confirmation) return;

    try {
      if (confirmation === "cancel") {
        await cancelOrder.mutateAsync({ orderId: order.id });
        toast.success("Order cancelled. Inventory reservations were released where supported.");
      } else {
        if (!targetStatus) return;
        const result = await changeStatus.mutateAsync({
          orderId: order.id,
          status: targetStatus,
        });
        toast.success(`Order status updated to ${formatStatus(result.order?.status ?? targetStatus)}.`);
      }

      setConfirmation(null);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "The order could not be updated.");
    }
  };

  return (
    <Dialog
      open={Boolean(orderId)}
      onOpenChange={(open) => {
        if (!open) {
          setConfirmation(null);
          onClose();
        }
      }}
    >
      <DialogContent className="max-h-[92vh] overflow-y-auto sm:max-w-4xl">
        <DialogHeader>
          <DialogTitle className="text-xl">Order details</DialogTitle>
          <DialogDescription>
            Review the order, its delivery details, and the next valid fulfillment action.
          </DialogDescription>
        </DialogHeader>

        <QueryState
          loading={details.isLoading}
          error={details.error}
          onRetry={() => void details.refetch()}
        />

        {order && (
          <div className="space-y-5">
            <section className="rounded-xl border bg-muted/20 p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="font-mono text-sm font-semibold">{order.orderNumber}</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Placed {dateTime(order.createdAtUtc)} · customer {order.userId.slice(0, 8)}…
                  </p>
                </div>
                <StatusPill variant={orderStatusTone(order.status)}>{formatStatus(order.status)}</StatusPill>
              </div>
              <div className="mt-5 border-t pt-4">
                <div className="mb-3 flex items-center justify-between gap-3">
                  <h3 className="text-xs font-semibold">Fulfillment progress</h3>
                  <span className="text-[10px] text-muted-foreground">Updated {dateTime(order.updatedAtUtc)}</span>
                </div>
                <OrderProgress status={order.status} />
              </div>
            </section>

            <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
              <section className="overflow-hidden rounded-xl border">
                <SectionTitle
                  title={`Items · ${order.items.length}`}
                  description={`${order.items.reduce((count, item) => count + item.quantity, 0)} units`}
                />
                <div className="divide-y">
                  {order.items.map((item) => (
                    <article key={item.id} className="flex gap-3 px-4 py-3">
                      <div className="grid size-10 shrink-0 place-items-center rounded-lg bg-muted text-muted-foreground">
                        <PackageCheck className="size-4" />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-sm font-medium">{item.productName}</p>
                        <p className="mt-1 text-[11px] text-muted-foreground">
                          {item.sku} · {item.packageQuantity} {item.packageUnit} · Qty {item.quantity}
                        </p>
                        {item.requiresPrescription && (
                          <p className="mt-1 text-[10px] font-medium text-amber-700 dark:text-amber-300">
                            Prescription required
                          </p>
                        )}
                      </div>
                      <p className="shrink-0 text-sm font-semibold">{money(item.lineTotal, order.currency)}</p>
                    </article>
                  ))}
                </div>
              </section>

              <div className="space-y-4">
                <section className="rounded-xl border p-4">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Order total</h3>
                  <div className="mt-3 space-y-2 text-xs">
                    <SummaryRow label="Items" value={money(order.subtotal, order.currency)} />
                    <SummaryRow label="Discount" value={`−${money(order.discountTotal, order.currency)}`} />
                    <SummaryRow label="Tax" value={money(order.taxTotal, order.currency)} />
                    <SummaryRow label="Shipping" value={money(order.shippingFee, order.currency)} />
                    <div className="mt-2 flex justify-between border-t pt-3 text-sm font-semibold">
                      <span>Total</span><span>{money(order.grandTotal, order.currency)}</span>
                    </div>
                  </div>
                </section>

                <section className="rounded-xl border p-4">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Ship to</h3>
                  <div className="mt-2 text-xs leading-5">
                    <p className="font-medium">{order.shippingAddress.recipientName}</p>
                    <p className="text-muted-foreground">
                      {order.shippingAddress.addressLine1}
                      {order.shippingAddress.addressLine2 ? `, ${order.shippingAddress.addressLine2}` : ""}
                    </p>
                    <p className="text-muted-foreground">
                      {order.shippingAddress.city}, {order.shippingAddress.state} {order.shippingAddress.postalCode}
                    </p>
                    <p className="text-muted-foreground">
                      {order.shippingAddress.country} · {order.shippingAddress.recipientPhone}
                    </p>
                  </div>
                </section>
              </div>
            </div>

            <section className="rounded-xl border bg-muted/20 p-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h3 className="text-sm font-semibold">Order actions</h3>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Only the next supported status transition is available.
                  </p>
                </div>
                <div className="flex flex-wrap gap-2">
                  {targetStatus && (
                    <Button size="sm" disabled={pending} onClick={() => setConfirmation("status")}>
                      Move to {formatStatus(targetStatus)} <ArrowRight className="ml-1 size-3.5" />
                    </Button>
                  )}
                  {["Placed", "Confirmed", "Processing"].includes(order.status) && (
                    <Button size="sm" variant="destructive" disabled={pending} onClick={() => setConfirmation("cancel")}>
                      Cancel order
                    </Button>
                  )}
                  {!targetStatus && order.status !== "Cancelled" && order.status !== "Delivered" && (
                    <span className="inline-flex items-center gap-1.5 text-xs text-muted-foreground">
                      <CircleCheck className="size-4" /> No further action available
                    </span>
                  )}
                </div>
              </div>

              {confirmation && (
                <div className={`mt-4 rounded-lg border p-3 ${confirmation === "cancel" ? "border-destructive/20 bg-destructive/5" : "border-primary/20 bg-primary/5"}`}>
                  <p className="text-sm font-medium">
                    {confirmation === "cancel" ? "Confirm order cancellation" : `Confirm status change to ${formatStatus(targetStatus ?? "")}`}
                  </p>
                  <p className="mt-1 text-xs leading-5 text-muted-foreground">
                    {confirmation === "cancel"
                      ? "This action cancels the order and releases reserved stock where supported."
                      : "The backend validates the transition and any prescription requirements before saving."}
                  </p>
                  <div className="mt-3 flex flex-wrap justify-end gap-2">
                    <Button size="sm" variant="outline" disabled={pending} onClick={() => setConfirmation(null)}>
                      Keep current status
                    </Button>
                    <Button
                      size="sm"
                      variant={confirmation === "cancel" ? "destructive" : "default"}
                      disabled={pending}
                      onClick={() => void applyAction()}
                    >
                      {pending ? "Updating…" : confirmation === "cancel" ? "Confirm cancellation" : "Confirm update"}
                    </Button>
                  </div>
                </div>
              )}
            </section>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

export function AdminOrdersPage() {
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState<OrderStatus | "">("");
  const [search, setSearch] = useState("");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const deferredSearch = useDeferredValue(search);

  const request = useMemo(
    () => ({
      page,
      pageSize: PAGE_SIZE,
      ...(status ? { status } : {}),
      ...(deferredSearch.trim() ? { orderNumber: deferredSearch.trim() } : {}),
    }),
    [deferredSearch, page, status],
  );

  const orders = useAdminOrders(request);
  const rows = orders.data?.items ?? [];
  const actionableCount = rows.filter(
    (order) => Boolean(nextStatus[order.status]) || ["Placed", "Confirmed", "Processing"].includes(order.status),
  ).length;
  const pageValue = rows.reduce((sum, order) => sum + order.grandTotal, 0);

  const changeSearch = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  const changeFilter = (value: string) => {
    setStatus(value as OrderStatus | "");
    setPage(1);
  };

  return (
    <>
      <PageHeading
        eyebrow="Order operations · live API"
        title="Orders"
        description="Review customer orders, inspect delivery details, and advance valid backend status transitions."
        action={(
          <Button variant="outline" onClick={() => void orders.refetch()} disabled={orders.isFetching}>
            <RefreshCw className={`mr-2 size-4 ${orders.isFetching ? "animate-spin" : ""}`} />
            Refresh
          </Button>
        )}
      />

      <div className="mb-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Matching orders" value={orders.isError ? "—" : orders.data?.totalCount ?? "—"} note="Backend total for current filters" icon={PackageCheck} />
        <MetricCard label="On this page" value={orders.isError ? "—" : rows.length} note={`Page ${orders.data?.page ?? page} of ${orders.data?.totalPages ?? "—"}`} icon={Clock3} />
        <MetricCard label="Order value on page" value={orders.isError || orders.isLoading ? "—" : money(pageValue, rows[0]?.currency ?? "INR")} note="Loaded page only" icon={ArrowDownRight} tone="green" />
        <MetricCard label="Ready for action" value={orders.isError || orders.isLoading ? "—" : actionableCount} note="Progress or cancellation available" icon={AlertTriangle} tone="amber" />
      </div>

      <Surface>
        <SectionTitle
          title="Order queue"
          description="Search by order number and filter by current status."
          action={(
            <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row">
              <label className="relative">
                <Search className="pointer-events-none absolute left-2.5 top-1/2 size-3.5 -translate-y-1/2 text-muted-foreground" />
                <Input
                  className="h-9 pl-8 sm:w-56"
                  value={search}
                  onChange={(event) => changeSearch(event.target.value)}
                  placeholder="Search order number"
                  aria-label="Search orders by order number"
                />
              </label>
              <select
                aria-label="Filter orders by status"
                className="h-9 rounded-lg border border-input bg-background px-3 text-xs"
                value={status}
                onChange={(event) => changeFilter(event.target.value)}
              >
                <option value="">All statuses</option>
                {statusOptions.map((item) => <option key={item} value={item}>{formatStatus(item)}</option>)}
              </select>
            </div>
          )}
        />

        <QueryState
          loading={orders.isLoading}
          error={orders.error}
          empty={!orders.isLoading && !orders.error && rows.length === 0}
          onRetry={() => void orders.refetch()}
        />

        {!orders.isLoading && !orders.error && rows.length > 0 && (
          <>
            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[780px] text-left text-sm">
                <thead className="bg-muted/35 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-5 py-3">Order</th>
                    <th className="px-5 py-3">Placed</th>
                    <th className="px-5 py-3">Items</th>
                    <th className="px-5 py-3">Status</th>
                    <th className="px-5 py-3 text-right">Total</th>
                    <th className="px-5 py-3 text-right">Review</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {rows.map((order) => (
                    <tr key={order.id} className="transition-colors hover:bg-muted/20">
                      <td className="px-5 py-3.5">
                        <button className="font-mono text-xs font-semibold text-primary hover:underline" onClick={() => setSelectedId(order.id)}>
                          {order.orderNumber}
                        </button>
                        <p className="mt-1 text-[10px] text-muted-foreground">ID {order.id.slice(0, 8)}…</p>
                      </td>
                      <td className="px-5 py-3.5 text-xs text-muted-foreground">{dateTime(order.createdAtUtc)}</td>
                      <td className="px-5 py-3.5 text-xs">{order.itemCount} items · {order.totalQuantity} units</td>
                      <td className="px-5 py-3.5"><StatusPill variant={orderStatusTone(order.status)}>{formatStatus(order.status)}</StatusPill></td>
                      <td className="px-5 py-3.5 text-right font-semibold">{money(order.grandTotal, order.currency)}</td>
                      <td className="px-5 py-3.5 text-right">
                        <Button size="sm" variant="outline" onClick={() => setSelectedId(order.id)}>Review</Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="divide-y md:hidden">
              {rows.map((order) => (
                <button
                  key={order.id}
                  className="flex w-full items-center gap-3 p-4 text-left transition-colors hover:bg-muted/20"
                  onClick={() => setSelectedId(order.id)}
                >
                  <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                    <PackageCheck className="size-4" />
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="flex items-center justify-between gap-2">
                      <span className="truncate font-mono text-xs font-semibold">{order.orderNumber}</span>
                      <StatusPill variant={orderStatusTone(order.status)}>{formatStatus(order.status)}</StatusPill>
                    </span>
                    <span className="mt-1 block text-xs text-muted-foreground">
                      {dateTime(order.createdAtUtc)} · {order.itemCount} items
                    </span>
                  </span>
                  <span className="shrink-0 text-right text-xs font-semibold">
                    {money(order.grandTotal, order.currency)}
                    <ArrowRight className="ml-auto mt-1 size-3.5 text-muted-foreground" />
                  </span>
                </button>
              ))}
            </div>

          </>
        )}
        {orders.data && orders.data.totalCount > 0 && (
          <PageControls
            page={orders.data.page}
            totalPages={orders.data.totalPages}
            onChange={setPage}
          />
        )}
      </Surface>

      <div className="mt-4 flex items-start gap-2 rounded-xl border bg-muted/25 px-4 py-3 text-xs leading-5 text-muted-foreground">
        <CheckCircle2 className="mt-0.5 size-4 shrink-0 text-primary" />
        Order data, details, cancellation, and status changes use the live admin API. Payment and carrier details are shown on their own admin pages.
      </div>

      <OrderDetailsDialog orderId={selectedId} onClose={() => setSelectedId(null)} />
    </>
  );
}
