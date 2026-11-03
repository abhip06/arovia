"use client";

import { useMemo, useState, type ElementType } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import {
    ArrowRight,
    CheckCircle2,
    ChevronLeft,
    ChevronRight,
    Clock3,
    PackageCheck,
    Search,
    ShieldCheck,
    ShoppingBag,
    Truck,
    X,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuth } from "@/features/auth/context/auth-context";
import { useCustomerOrders } from "@/features/orders/hooks/use-orders";
import {
    extractOrderList,
    formatOrderDate,
    isOrderInProgress,
    OrderItemImage,
    OrderStatusBadge,
    OrdersEmptyState,
    OrdersErrorState,
    OrdersLoadingState,
} from "./order-page-ui";

const PAGE_SIZE = 8;
type FilterKey = "all" | "active" | "delivered" | "cancelled";

const filters: { value: FilterKey; label: string }[] = [
    { value: "all", label: "All orders" },
    { value: "active", label: "In progress" },
    { value: "delivered", label: "Delivered" },
    { value: "cancelled", label: "Cancelled" },
];

export function OrdersPage() {
    const router = useRouter();
    const { authStatus, isLoading: isAuthLoading, user } = useAuth();
    const isAuthenticated = !isAuthLoading && Boolean(authStatus && user);
    const [page, setPage] = useState(1);
    const [activeFilter, setActiveFilter] = useState<FilterKey>("all");
    const [searchValue, setSearchValue] = useState("");

    // If your hook uses a different argument shape, adapt this call only.
    const ordersQuery = useCustomerOrders({ page, pageSize: PAGE_SIZE }, isAuthenticated);
    const result = extractOrderList(ordersQuery.data);
    const orders = result.items;
    const totalCount = result.totalCount;
    const pageCount = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

    const visibleOrders = useMemo(() => {
        const normalizedSearch = searchValue.trim().toLocaleLowerCase();
        return orders.filter((order) => {
            const statusMatches =
                activeFilter === "all" ||
                (activeFilter === "active" && isOrderInProgress(order.status)) ||
                (activeFilter === "delivered" && order.status === "Delivered") ||
                (activeFilter === "cancelled" && order.status === "Cancelled");
            const searchable = [
                order.orderNumber,
                order.status,
                ...(order.items ?? []).map((item) => item.productName),
            ].join(" ").toLocaleLowerCase();
            return statusMatches && (!normalizedSearch || searchable.includes(normalizedSearch));
        });
    }, [orders, activeFilter, searchValue]);

    const inProgressCount = orders.filter((order) => isOrderInProgress(order.status)).length;
    const deliveredCount = orders.filter((order) => order.status === "Delivered").length;

    if (isAuthLoading) return <main className="mx-auto w-full max-w-6xl px-4 py-10 sm:px-6 lg:px-8"><OrdersLoadingState /></main>;

    if (!authStatus || !user) {
        return (
            <main className="mx-auto flex min-h-[65vh] w-full max-w-6xl items-center justify-center px-4 py-12 sm:px-6">
                <div className="flex max-w-md flex-col items-center text-center">
                    <div className="flex size-16 items-center justify-center rounded-2xl border border-primary/10 bg-primary/5 text-primary"><ShieldCheck className="size-7" /></div>
                    <h1 className="mt-5 text-2xl font-semibold tracking-tight">Sign in to view your orders</h1>
                    <p className="mt-2 text-sm leading-6 text-muted-foreground">Your order history is connected to your Arovia account. Sign in to see order updates and delivery details.</p>
                    <Button className="mt-6 rounded-xl" onClick={() => router.push(`/sign-in?returnUrl=${encodeURIComponent("/account/orders")}`)}>Sign in <ArrowRight className="ml-2 size-4" /></Button>
                </div>
            </main>
        );
    }

    if (ordersQuery.isPending) return <main className="mx-auto w-full max-w-6xl px-4 py-10 sm:px-6 lg:px-8"><OrdersLoadingState /></main>;

    if (ordersQuery.isError) {
        return <main className="mx-auto w-full max-w-6xl px-4 py-10 sm:px-6 lg:px-8"><OrdersErrorState message={ordersQuery.error instanceof Error ? ordersQuery.error.message : undefined} onRetry={() => void ordersQuery.refetch()} /></main>;
    }

    const clearFilters = () => {
        setActiveFilter("all");
        setSearchValue("");
    };

    return (
        <main className="min-h-[65vh] bg-background mt-16">
            <div className="mx-auto w-full max-w-6xl px-4 py-7 sm:px-6 sm:py-10 lg:px-8">
                <div className="mb-7 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
                    <button type="button" onClick={() => router.push("/")} className="transition-colors hover:text-foreground cursor-pointer">Home</button>
                    <span aria-hidden="true">/</span>
                    <button type="button" onClick={() => router.push("/my-account")} className="transition-colors hover:text-foreground cursor-pointer">My Account</button>
                    <span aria-hidden="true">/</span>
                    <span className="font-medium text-foreground">Orders</span>
                </div>

                <motion.header
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.3, ease: "easeOut" }}
                    className="mb-7 flex flex-col justify-between gap-5 sm:flex-row sm:items-end"
                >
                    <div>
                        {/* <div className="mb-3 inline-flex items-center gap-2 rounded-full border border-primary/15 bg-primary/5 px-3 py-1.5 text-[11px] font-semibold uppercase tracking-[0.14em] text-primary">
                            <span className="size-1.5 rounded-full bg-primary" /> Your Orders
                        </div> */}
                        {/* <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Your orders</h1> */}
                        <p className="mt-2 max-w-xl text-sm leading-6 text-muted-foreground">Hi {user.firstName || "there"}, keep track of your health essentials and delivery updates in one place.</p>
                    </div>
                    <Button variant="outline" onClick={() => router.push("/otc/medicine")} className="w-fit rounded-xl border-border bg-card">
                        Continue shopping <ArrowRight className="ml-2 size-4" />
                    </Button>
                </motion.header>

                {/* <div className="mb-8 grid gap-3 sm:grid-cols-3">
                    <SummaryCard label="All orders" value={totalCount} caption="Your order history" icon={ShoppingBag} tone="green" />
                    <SummaryCard label="In progress" value={inProgressCount} caption="Orders moving forward" icon={Truck} tone="blue" />
                    <SummaryCard label="Delivered" value={deliveredCount} caption="Delivered with care" icon={CheckCircle2} tone="violet" />
                </div> */}

                {totalCount === 0 ? (
                    <OrdersEmptyState onShop={() => router.push("/otc/medicine")} />
                ) : (
                    <section>
                        <div className="mb-4 flex flex-col gap-4 rounded-2xl border border-border/80 bg-card p-4 shadow-sm shadow-black/[0.02] sm:flex-row sm:items-center sm:justify-between sm:p-5">
                            <div>
                                <h2 className="text-base font-semibold tracking-tight">Order history</h2>
                                <p className="mt-1 text-xs text-muted-foreground">Find an order or check its latest status.</p>
                            </div>
                            <div className="relative w-full sm:max-w-xs">
                                <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                                <Input value={searchValue} onChange={(event) => setSearchValue(event.target.value)} placeholder="Search order or product" aria-label="Search order or product" className="h-10 rounded-xl border-border bg-background pl-9 pr-9 text-sm" />
                                {searchValue && <button type="button" aria-label="Clear search" onClick={() => setSearchValue("")} className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"><X className="size-4" /></button>}
                            </div>
                        </div>

                        <div className="mb-4 flex gap-2 overflow-x-auto pb-1" role="group" aria-label="Filter orders">
                            {filters.map((filter) => (
                                <Button key={filter.value} variant={activeFilter === filter.value ? "default" : "outline"} size="sm" onClick={() => setActiveFilter(filter.value)} className={`shrink-0 rounded-xl ${activeFilter === filter.value ? "shadow-none" : "border-border bg-card"}`}>
                                    {filter.label}
                                </Button>
                            ))}
                        </div>

                        {visibleOrders.length === 0 ? (
                            <div className="rounded-2xl border border-dashed border-border bg-card px-6 py-14 text-center">
                                <Search className="mx-auto size-6 text-muted-foreground" />
                                <h3 className="mt-4 font-semibold">No matching orders</h3>
                                <p className="mt-1 text-sm text-muted-foreground">Try another search term or filter.</p>
                                <Button variant="outline" onClick={clearFilters} className="mt-4 rounded-xl">Clear filters</Button>
                            </div>
                        ) : (
                            <div className="space-y-4">
                                {visibleOrders.map((order, index) => (
                                    <motion.article
                                        key={order.id}
                                        initial={{ opacity: 0, y: 10 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        transition={{ duration: 0.25, delay: Math.min(index * 0.04, 0.2) }}
                                        className="overflow-hidden rounded-2xl border border-border/80 bg-card shadow-sm shadow-black/[0.02] transition-all duration-200 hover:border-primary/25 hover:shadow-md hover:shadow-primary/[0.04]"
                                    >
                                        <div className="flex flex-col gap-3 border-b border-border/70 px-4 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6">
                                            <div>
                                                <div className="flex flex-wrap items-center gap-2.5">
                                                    <p className="text-sm font-semibold tracking-tight">Order #{order.orderNumber}</p>
                                                    <OrderStatusBadge status={order.status} />
                                                </div>
                                                <p className="mt-1.5 text-xs text-muted-foreground">Placed on {formatOrderDate(order.createdAtUtc)}</p>
                                            </div>
                                            <div className="flex items-center gap-2 self-start sm:self-auto">
                                                <span className="text-xs text-muted-foreground">Order total</span>
                                                <span className="text-base font-semibold tracking-tight">{formatCurrency(order.grandTotal)}</span>
                                            </div>
                                        </div>

                                        <div className="flex flex-col gap-4 px-4 py-4 sm:flex-row sm:items-center sm:px-6 sm:py-5">
                                            <div className="flex min-w-0 flex-1 items-center">
                                                {(order.items ?? []).slice(0, 3).map((item) => (
                                                    <div key={item.id} className="-ml-2 first:ml-0">
                                                        <OrderItemImage item={item} size="normal" />
                                                    </div>
                                                ))}
                                                {(!order.items || order.items.length === 0) && (
                                                    <div className="flex size-16 items-center justify-center rounded-2xl border border-border bg-muted/40"><PackageCheck className="size-6 text-muted-foreground" /></div>
                                                )}
                                                {(order.items?.length ?? 0) > 3 && <div className="-ml-2 flex size-16 shrink-0 items-center justify-center rounded-2xl border border-border bg-muted text-xs font-semibold text-muted-foreground">+{(order.items?.length ?? 0) - 3}</div>}
                                                <div className="ml-4 min-w-0">
                                                    <p className="truncate text-sm font-medium">{order.items?.[0]?.productName ?? "Order items"}{(order.items?.length ?? 0) > 1 ? ` + ${(order.items?.length ?? 0) - 1} more` : ""}</p>
                                                    <p className="mt-1 text-xs text-muted-foreground">{order.itemCount ?? order.items?.reduce((sum, item) => sum + item.quantity, 0) ?? order.items?.length ?? 0} item(s) in this order</p>
                                                    <p className="mt-2 flex items-center gap-1.5 text-xs text-muted-foreground">{order.status === "Placed" ? <Clock3 className="size-3.5 text-amber-600" /> : order.status === "Delivered" ? <CheckCircle2 className="size-3.5 text-emerald-600" /> : <Truck className="size-3.5 text-primary" />}{order.status === "Placed" ? "Payment confirmation is pending" : order.status === "Delivered" ? "Delivered with care" : order.status === "Cancelled" ? "View the order record" : "We’ll keep you updated"}</p>
                                                </div>
                                            </div>
                                            <Button onClick={() => router.push(`/orders/${order.id}`)} className="w-full rounded-xl sm:w-auto">
                                                View details <ArrowRight className="ml-2 size-4" />
                                            </Button>
                                        </div>
                                    </motion.article>
                                ))}
                            </div>
                        )}

                        <div className="mt-5 flex flex-col gap-3 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
                            <span>Showing {visibleOrders.length} order{visibleOrders.length === 1 ? "" : "s"} on this page</span>
                            {pageCount > 1 && (
                                <div className="flex items-center gap-2 self-end sm:self-auto">
                                    <Button variant="outline" size="sm" disabled={page <= 1 || ordersQuery.isFetching} onClick={() => setPage((current) => Math.max(1, current - 1))} className="rounded-lg border-border bg-card"><ChevronLeft className="mr-1 size-4" /> Previous</Button>
                                    <span className="min-w-16 text-center">Page {page} of {pageCount}</span>
                                    <Button variant="outline" size="sm" disabled={page >= pageCount || ordersQuery.isFetching} onClick={() => setPage((current) => Math.min(pageCount, current + 1))} className="rounded-lg border-border bg-card">Next <ChevronRight className="ml-1 size-4" /></Button>
                                </div>
                            )}
                        </div>
                    </section>
                )}

                <div className="mt-8 flex items-start gap-3 rounded-2xl border border-primary/10 bg-primary/[0.035] p-4 sm:p-5">
                    <ShieldCheck className="mt-0.5 size-5 shrink-0 text-primary" />
                    <div>
                        <p className="text-sm font-medium">Your health information matters</p>
                        <p className="mt-1 text-xs leading-5 text-muted-foreground">Your order details and delivery address are shown only within your signed-in account.</p>
                    </div>
                </div>
            </div>
        </main>
    );
}

function SummaryCard({ label, value, caption, icon: Icon, tone }: { label: string; value: number; caption: string; icon: ElementType; tone: "green" | "blue" | "violet" }) {
    const iconTone = tone === "green" ? "bg-primary/10 text-primary" : tone === "blue" ? "bg-blue-500/10 text-blue-600 dark:text-blue-400" : "bg-violet-500/10 text-violet-600 dark:text-violet-400";
    return (
        <div className="flex min-h-28 items-center justify-between gap-4 rounded-2xl border border-border/80 bg-card p-4 shadow-sm shadow-black/[0.02] sm:p-5">
            <div>
                <p className="text-xs font-medium text-muted-foreground">{label}</p>
                <p className="mt-2 text-2xl font-semibold tracking-tight">{value}</p>
                <p className="mt-1 text-[11px] text-muted-foreground">{caption}</p>
            </div>
            <span className={`flex size-11 shrink-0 items-center justify-center rounded-2xl ${iconTone}`}><Icon className="size-5" /></span>
        </div>
    );
}

function formatCurrency(value: number) {
    return new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR", maximumFractionDigits: 2 }).format(value);
}
