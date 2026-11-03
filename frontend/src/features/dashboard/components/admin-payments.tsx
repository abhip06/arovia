"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import { ArrowUpRight, CheckCircle2, CircleDollarSign, Clock3, Copy, CreditCard, RefreshCw, Search, ShieldAlert } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { MetricCard, PageControls, PageHeading, QueryState, SectionTitle, StatusPill, Surface } from "./dashboard-primitives";
import { useAdminPayments } from "../hooks/use-admin-payments";

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";

  return new Intl.DateTimeFormat("en-IN", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function statusVariant(status: string): "success" | "warning" | "danger" | "muted" {
  const value = status.toLowerCase();
  if (value.includes("captur") || value.includes("success")) return "success";
  if (value.includes("pending") || value.includes("process")) return "warning";
  if (value.includes("fail") || value.includes("refund") || value.includes("cancel")) return "danger";
  return "muted";
}

function shortId(value: string) {
  return `${value.slice(0, 8)}…${value.slice(-4)}`;
}

async function copyId(value: string, label: string) {
  try {
    await navigator.clipboard.writeText(value);
    toast.success(`${label} copied`);
  } catch {
    toast.error(`Could not copy the ${label.toLowerCase()}.`);
  }
}

export function AdminPaymentsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const paymentsQuery = useAdminPayments(page);
  const payments = useMemo(() => paymentsQuery.data?.items ?? [], [paymentsQuery.data?.items]);
  const searchTerm = search.trim().toLowerCase();

  const visiblePayments = useMemo(() => {
    if (!searchTerm) return payments;
    return payments.filter((payment) =>
      `${payment.id} ${payment.orderId} ${payment.status}`.toLowerCase().includes(searchTerm),
    );
  }, [payments, searchTerm]);

  const statusCounts = payments.reduce<Record<string, number>>((counts, payment) => {
    const key = payment.status.toLowerCase();
    counts[key] = (counts[key] ?? 0) + 1;
    return counts;
  }, {});

  return (
    <>
      <PageHeading
        eyebrow="Payments · live API"
        title="Payment activity"
        description="Review payment records and their linked orders. The ledger is read-only; this API does not expose refunds or reconciliation actions."
        action={(
          <Button
            variant="outline"
            onClick={() => void paymentsQuery.refetch()}
            disabled={paymentsQuery.isFetching}
          >
            <RefreshCw className={`mr-2 size-4 ${paymentsQuery.isFetching ? "animate-spin" : ""}`} />
            Refresh
          </Button>
        )}
      />

      <div className="mb-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Payment records"
          value={paymentsQuery.isError ? "—" : paymentsQuery.data?.totalCount ?? "—"}
          note="Total count from the paginated admin API"
          icon={CreditCard}
        />
        <MetricCard
          label="On this page"
          value={paymentsQuery.isError ? "—" : payments.length}
          note={`Page ${paymentsQuery.data?.page ?? page} of ${paymentsQuery.data?.totalPages ?? "—"}`}
          icon={CircleDollarSign}
          tone="primary"
        />
        <MetricCard
          label="Captured on page"
          value={paymentsQuery.isLoading || paymentsQuery.isError ? "—" : Object.entries(statusCounts).filter(([status]) => status.includes("captur")).reduce((sum, [, count]) => sum + count, 0)}
          note="Status counts use the loaded page"
          icon={CheckCircle2}
          tone="green"
        />
        <MetricCard
          label="Pending on page"
          value={paymentsQuery.isLoading || paymentsQuery.isError ? "—" : Object.entries(statusCounts).filter(([status]) => status.includes("pending")).reduce((sum, [, count]) => sum + count, 0)}
          note="Review the linked order for context"
          icon={Clock3}
          tone="amber"
        />
      </div>

      <Surface>
        <SectionTitle
          title="Payment records"
          description="Payment ID, order relationship, status, and timestamps returned by the backend."
          action={(
            <label className="relative block w-full sm:w-72">
              <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Search this page by ID or status"
                className="pl-9"
                aria-label="Search payments on this page"
              />
            </label>
          )}
        />

        <QueryState
          loading={paymentsQuery.isLoading}
          error={paymentsQuery.error}
          empty={!paymentsQuery.isLoading && !paymentsQuery.error && visiblePayments.length === 0}
          onRetry={() => void paymentsQuery.refetch()}
        />

        {!paymentsQuery.isLoading && !paymentsQuery.error && visiblePayments.length > 0 && (
          <>
            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[860px] text-left text-sm">
                <thead className="bg-muted/35 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-5 py-3">Payment</th>
                    <th className="px-5 py-3">Order</th>
                    <th className="px-5 py-3">Status</th>
                    <th className="px-5 py-3">Created</th>
                    <th className="px-5 py-3">Last updated</th>
                    <th className="px-5 py-3 text-right">Action</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {visiblePayments.map((payment) => (
                    <tr key={payment.id} className="transition-colors hover:bg-muted/20">
                      <td className="px-5 py-3.5">
                        <button className="inline-flex items-center gap-2 font-mono text-xs font-medium hover:text-primary" onClick={() => void copyId(payment.id, "Payment ID")}>
                          {shortId(payment.id)} <Copy className="size-3 text-muted-foreground" />
                        </button>
                      </td>
                      <td className="px-5 py-3.5">
                        <button className="inline-flex items-center gap-2 font-mono text-xs hover:text-primary" onClick={() => void copyId(payment.orderId, "Order ID")}>
                          {shortId(payment.orderId)} <Copy className="size-3 text-muted-foreground" />
                        </button>
                      </td>
                      <td className="px-5 py-3.5"><StatusPill variant={statusVariant(payment.status)}>{payment.status}</StatusPill></td>
                      <td className="px-5 py-3.5 text-xs text-muted-foreground">{formatDate(payment.createdAtUtc)}</td>
                      <td className="px-5 py-3.5 text-xs text-muted-foreground">{formatDate(payment.updatedAtUtc)}</td>
                      <td className="px-5 py-3.5 text-right">
                        <Link href="/admin/orders" className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline">
                          Orders <ArrowUpRight className="size-3.5" />
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="divide-y md:hidden">
              {visiblePayments.map((payment) => (
                <article key={payment.id} className="space-y-3 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <p className="text-[10px] font-medium uppercase tracking-wide text-muted-foreground">Payment</p>
                      <button className="mt-1 inline-flex items-center gap-2 font-mono text-xs" onClick={() => void copyId(payment.id, "Payment ID")}>
                        {shortId(payment.id)} <Copy className="size-3 text-muted-foreground" />
                      </button>
                    </div>
                    <StatusPill variant={statusVariant(payment.status)}>{payment.status}</StatusPill>
                  </div>
                  <div className="grid grid-cols-2 gap-3 text-xs">
                    <div className="min-w-0">
                      <p className="text-muted-foreground">Order ID</p>
                      <button className="mt-1 inline-flex items-center gap-1 font-mono" onClick={() => void copyId(payment.orderId, "Order ID")}>
                        {shortId(payment.orderId)} <Copy className="size-3 text-muted-foreground" />
                      </button>
                    </div>
                    <div><p className="text-muted-foreground">Created</p><p className="mt-1">{formatDate(payment.createdAtUtc)}</p></div>
                  </div>
                  <div className="flex items-center justify-between border-t pt-3 text-xs">
                    <span className="text-muted-foreground">Updated {formatDate(payment.updatedAtUtc)}</span>
                    <Link href="/admin/orders" className="inline-flex items-center gap-1 font-medium text-primary">Orders <ArrowUpRight className="size-3.5" /></Link>
                  </div>
                </article>
              ))}
            </div>

            {searchTerm && (
              <p className="border-t px-4 py-2 text-xs text-muted-foreground sm:px-5">
                Search filters the current page only. Use pagination to browse other records.
              </p>
            )}
          </>
        )}
        {paymentsQuery.data && paymentsQuery.data.totalCount > 0 && (
          <PageControls
            page={paymentsQuery.data.page}
            totalPages={paymentsQuery.data.totalPages}
            onChange={setPage}
          />
        )}
      </Surface>

      <div className="mt-4 flex items-start gap-2 rounded-xl border bg-muted/25 px-4 py-3 text-xs leading-5 text-muted-foreground">
        <ShieldAlert className="mt-0.5 size-4 shrink-0 text-primary" />
        The payment API does not return transaction amounts or provider references. This view shows only the fields available from the live paged endpoint; it does not infer missing financial data.
      </div>
    </>
  );
}
