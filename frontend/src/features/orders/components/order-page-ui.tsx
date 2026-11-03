import Image from "next/image";
import type { ReactNode } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { formatPrice } from "@/lib/formatters";
import {
    AlertCircle,
    ArrowRight,
    Check,
    Package,
    ShoppingBag,
    ShieldCheck,
    Truck,
} from "lucide-react";

export type CustomerOrderStatus =
    | "Placed"
    | "Confirmed"
    | "Processing"
    | "Shipped"
    | "OutForDelivery"
    | "Delivered"
    | "Cancelled"
    | string;

export interface CustomerOrderItem {
    id: string;
    productVariantId?: string;
    productName: string;
    sku?: string;
    imageUrl?: string | null;
    packageQuantity?: number;
    packageUnit?: string;
    quantity: number;
    unitMrp?: number;
    unitSellingPrice: number;
    lineTaxAmount?: number;
    lineDiscountAmount?: number;
    lineSubtotal?: number;
    lineTotal: number;
    requiresPrescription?: boolean;
}

export interface CustomerOrderAddress {
    recipientName: string;
    recipientPhone: string;
    addressLine1: string;
    addressLine2?: string | null;
    city: string;
    state: string;
    postalCode: string;
    country: string;
}

export interface CustomerOrder {
    id: string;
    orderNumber: string;
    status: CustomerOrderStatus;
    currency: string;
    subtotal: number;
    discountTotal: number;
    taxTotal: number;
    shippingFee: number;
    grandTotal: number;
    createdAtUtc: string;
    updatedAtUtc?: string;
    shippingAddress?: CustomerOrderAddress | null;
    items?: CustomerOrderItem[];
    itemCount?: number;
}

export interface PagedOrders<T> {
    items: T[];
    page?: number;
    pageSize?: number;
    totalCount?: number;
}

export const ORDER_STATUS_LABEL: Record<string, string> = {
    Placed: "Payment pending",
    Confirmed: "Confirmed",
    Processing: "Being prepared",
    Shipped: "Shipped",
    OutForDelivery: "Out for delivery",
    Delivered: "Delivered",
    Cancelled: "Cancelled",
};

export function getOrderStatusLabel(status: CustomerOrderStatus) {
    return ORDER_STATUS_LABEL[status] ?? status;
}

export function getOrderStatusClass(status: CustomerOrderStatus) {
    switch (status) {
        case "Placed":
            return "border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-900/70 dark:bg-amber-950/40 dark:text-amber-300";
        case "Confirmed":
            return "border-green-200 bg-green-50 text-green-700 dark:border-green-900/70 dark:bg-green-950/40 dark:text-green-300";
        case "Processing":
            return "border-yellow-200 bg-yellow-50 text-yellow-700 dark:border-yellow-900/70 dark:bg-yellow-950/40 dark:text-yellow-300";
        case "Shipped":
            return "border-teal-200 bg-teal-50 text-teal-700 dark:border-teal-900/70 dark:bg-teal-950/40 dark:text-teal-300";
        case "OutForDelivery":
            return "border-blue-200 bg-blue-50 text-blue-700 dark:border-blue-900/70 dark:bg-blue-950/40 dark:text-blue-300";
        case "Delivered":
            return "border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900/70 dark:bg-emerald-950/40 dark:text-emerald-300";
        case "Cancelled":
            return "border-rose-200 bg-rose-50 text-rose-700 dark:border-rose-900/70 dark:bg-rose-950/40 dark:text-rose-300";
        default:
            return "border-border bg-muted text-muted-foreground";
    }
}

export function OrderStatusBadge({ status }: { status: CustomerOrderStatus }) {
    return (
        <Badge
            variant="outline"
            className={`gap-1.5 rounded-full px-2.5 py-1 font-medium ${getOrderStatusClass(status)}`}
        >
            <span className="size-1.5 rounded-full bg-current" aria-hidden="true" />
            {getOrderStatusLabel(status)}
        </Badge>
    );
}

export function formatOrderDate(value: string, includeTime = false) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "Date unavailable";

    return new Intl.DateTimeFormat(
        "en-IN",
        includeTime
            ? { day: "numeric", month: "short", year: "numeric", hour: "numeric", minute: "2-digit" }
            : { day: "numeric", month: "short", year: "numeric" },
    ).format(date);
}

export function isOrderInProgress(status: CustomerOrderStatus) {
    return ["Placed", "Confirmed", "Processing", "Shipped", "OutForDelivery"].includes(status);
}

export function extractOrderList(value: unknown): {
    items: CustomerOrder[];
    totalCount: number;
    page: number;
    pageSize: number;
} {
    if (Array.isArray(value)) {
        return { items: value as CustomerOrder[], totalCount: value.length, page: 1, pageSize: value.length || 10 };
    }

    if (typeof value === "object" && value !== null) {
        const record = value as Record<string, unknown>;
        const rawItems = Array.isArray(record.items)
            ? record.items
            : Array.isArray(record.data)
                ? record.data
                : [];
        const items = rawItems as CustomerOrder[];
        const page = typeof record.page === "number" ? record.page : 1;
        const pageSize = typeof record.pageSize === "number" ? record.pageSize : Math.max(items.length, 10);
        const totalCount = typeof record.totalCount === "number" ? record.totalCount : items.length;
        return { items, totalCount, page, pageSize };
    }

    return { items: [], totalCount: 0, page: 1, pageSize: 10 };
}

export function extractOrderDetails(value: unknown): CustomerOrder | null {
    if (typeof value !== "object" || value === null) return null;
    const record = value as Record<string, unknown>;
    const order = typeof record.order === "object" && record.order !== null
        ? record.order
        : value;
    return order as CustomerOrder;
}

export function OrderItemImage({ item, size = "normal" }: { item: CustomerOrderItem; size?: "small" | "normal" | "large" }) {
    const dimensions = size === "small" ? "size-12 rounded-xl" : size === "large" ? "size-20 rounded-2xl" : "size-16 rounded-2xl";
    const imageSize = size === "small" ? 48 : size === "large" ? 80 : 64;

    return (
        <div className={`relative flex shrink-0 items-center justify-center overflow-hidden border border-border/80 bg-muted/40 ${dimensions}`}>
            {item.imageUrl ? (
                <Image
                    src={item.imageUrl}
                    alt={item.productName}
                    width={imageSize}
                    height={imageSize}
                    unoptimized
                    className="size-full object-contain p-1.5"
                />
            ) : (
                <Package className="size-6 text-muted-foreground/65" aria-hidden="true" />
            )}
        </div>
    );
}

export function OrdersLoadingState() {
    return (
        <div className="space-y-5" aria-label="Loading orders" aria-busy="true">
            <div className="space-y-3">
                <div className="h-3 w-28 animate-pulse rounded bg-muted" />
                <div className="h-9 w-56 animate-pulse rounded-lg bg-muted" />
                <div className="h-4 w-80 max-w-full animate-pulse rounded bg-muted" />
            </div>
            <div className="grid gap-3 sm:grid-cols-3">
                {Array.from({ length: 3 }, (_, index) => (
                    <div key={index} className="h-28 animate-pulse rounded-2xl border border-border bg-card" />
                ))}
            </div>
            <div className="space-y-3 rounded-2xl border border-border bg-card p-4 sm:p-6">
                {Array.from({ length: 3 }, (_, index) => (
                    <div key={index} className="h-36 animate-pulse rounded-xl bg-muted/70" />
                ))}
            </div>
        </div>
    );
}

export function OrdersErrorState({ message, onRetry }: { message?: string; onRetry: () => void }) {
    return (
        <div className="flex min-h-80 flex-col items-center justify-center rounded-3xl border border-border bg-card px-6 py-12 text-center">
            <div className="flex size-14 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
                <AlertCircle className="size-6" />
            </div>
            <h2 className="mt-5 text-lg font-semibold tracking-tight">We couldn’t load your orders</h2>
            <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
                {message || "Something went wrong while loading your order history. Please try again."}
            </p>
            <Button onClick={onRetry} className="mt-6 rounded-xl">Try again</Button>
        </div>
    );
}

export function OrdersEmptyState({ onShop }: { onShop: () => void }) {
    return (
        <div className="flex min-h-80 flex-col items-center justify-center rounded-3xl border border-dashed border-border bg-card px-6 py-12 text-center">
            <div className="flex size-16 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                <ShoppingBag className="size-7" />
            </div>
            <h2 className="mt-5 text-xl font-semibold tracking-tight">No orders yet</h2>
            <p className="mt-2 max-w-sm text-sm leading-6 text-muted-foreground">
                Your orders will appear here after you place your first order. Explore our health essentials whenever you’re ready.
            </p>
            <Button onClick={onShop} className="mt-6 rounded-xl">
                Explore health essentials <ArrowRight className="ml-2 size-4" />
            </Button>
        </div>
    );
}

export function OrderProgress({ status }: { status: CustomerOrderStatus }) {
    const steps = [
        { key: "Placed", label: "Placed" },
        { key: "Confirmed", label: "Confirmed" },
        { key: "Processing", label: "Preparing" },
        { key: "Shipped", label: status === "OutForDelivery" ? "Out for delivery" : "Shipped" },
        { key: "Delivered", label: "Delivered" },
    ];
    const activeIndex = status === "OutForDelivery" ? 3 : steps.findIndex((step) => step.key === status);

    if (status === "Cancelled") {
        return (
            <div className="flex gap-3 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-rose-800 dark:border-rose-900/70 dark:bg-rose-950/30 dark:text-rose-300">
                <AlertCircle className="mt-0.5 size-5 shrink-0" />
                <div>
                    <p className="text-sm font-semibold">This order was cancelled</p>
                    <p className="mt-1 text-sm leading-6 opacity-85">The order record remains available here for your reference. Contact support if you have a question about this order.</p>
                </div>
            </div>
        );
    }

    if (status === "Placed") {
        return (
            <div className="flex gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-4 text-amber-900 dark:border-amber-900/70 dark:bg-amber-950/30 dark:text-amber-300">
                <ShieldCheck className="mt-0.5 size-5 shrink-0" />
                <div>
                    <p className="text-sm font-semibold">Payment confirmation pending</p>
                    <p className="mt-1 text-sm leading-6 opacity-85">Your order is placed, but it will move forward only after payment is verified.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="rounded-2xl border border-border bg-card px-3 py-5 sm:px-6">
            <div className="grid grid-cols-5 gap-1 sm:gap-3">
                {steps.map((step, index) => {
                    const complete = status === "Delivered" || index < activeIndex;
                    const current = status !== "Delivered" && index === activeIndex;
                    return (
                        <div key={step.key} className="relative flex min-w-0 flex-col items-center text-center">
                            {index < steps.length - 1 && (
                                <span className={`absolute left-[calc(50%+15px)] right-[calc(-50%+15px)] top-3.5 h-0.5 ${index < activeIndex ? "bg-primary" : "bg-border"}`} aria-hidden="true" />
                            )}
                            <span className={`relative z-10 flex size-7 items-center justify-center rounded-full border text-xs ${complete ? "border-primary bg-primary text-primary-foreground" : current ? "border-primary bg-background text-primary ring-4 ring-primary/10" : "border-border bg-background text-muted-foreground"}`}>
                                {complete ? <Check className="size-3.5" /> : index === 3 ? <Truck className="size-3.5" /> : index + 1}
                            </span>
                            <span className={`mt-2 text-[10px] leading-4 sm:text-xs ${complete || current ? "font-semibold text-foreground" : "text-muted-foreground"}`}>{step.label}</span>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

export function OrderTotals({ order, showCaption = true }: { order: CustomerOrder; showCaption?: boolean }) {
    return (
        <div>
            <div className="space-y-3.5">
                <TotalLine label="Subtotal" value={formatPrice(order.subtotal)} />
                {order.discountTotal > 0 && <TotalLine label="Discount" value={`−${formatPrice(order.discountTotal)}`} valueClassName="text-emerald-600 dark:text-emerald-400" />}
                <TotalLine label="Tax" value={formatPrice(order.taxTotal)} />
                <TotalLine label="Delivery" value={order.shippingFee === 0 ? "Free" : formatPrice(order.shippingFee)} />
                <div className="border-t border-border pt-4">
                    <TotalLine label="Order total" value={formatPrice(order.grandTotal)} strong />
                </div>
            </div>
            {showCaption && <p className="mt-4 text-xs leading-5 text-muted-foreground">Amounts reflect the pricing snapshot saved with this order.</p>}
        </div>
    );
}

function TotalLine({ label, value, strong = false, valueClassName = "" }: { label: string; value: string; strong?: boolean; valueClassName?: string }) {
    return (
        <div className="flex items-center justify-between gap-4">
            <span className={strong ? "text-sm font-semibold text-foreground" : "text-sm text-muted-foreground"}>{label}</span>
            <span className={`${strong ? "text-lg font-bold tracking-tight text-foreground" : "text-sm font-medium text-foreground"} ${valueClassName}`}>{value}</span>
        </div>
    );
}

export function SectionCard({ title, description, children, action }: { title: string; description?: string; children: ReactNode; action?: ReactNode }) {
    return (
        <section className="overflow-hidden rounded-2xl border border-border/80 bg-card shadow-sm shadow-black/[0.02]">
            <div className="flex items-start justify-between gap-4 border-b border-border/70 px-5 py-4 sm:px-6">
                <div>
                    <h2 className="text-sm font-semibold tracking-tight text-foreground">{title}</h2>
                    {description && <p className="mt-1 text-xs leading-5 text-muted-foreground">{description}</p>}
                </div>
                {action}
            </div>
            <div className="p-5 sm:p-6">{children}</div>
        </section>
    );
}
