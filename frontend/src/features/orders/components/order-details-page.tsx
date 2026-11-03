"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { motion } from "framer-motion";
import {
    ArrowLeft,
    ArrowRight,
    CalendarDays,
    ChevronRight,
    FileDown,
    MapPin,
    PackageCheck,
    ShieldCheck,
    ShoppingBag,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";

import { useAuth } from "@/features/auth/context/auth-context";
import {
    useCancelCustomerOrder,
    useCustomerOrderDetails,
    useDownloadCustomerOrderReceipt,
} from "@/features/orders/hooks/use-orders";

import {
    extractOrderDetails,
    formatOrderDate,
    OrderItemImage,
    OrderProgress,
    OrderStatusBadge,
    OrderTotals,
    OrdersErrorState,
    OrdersLoadingState,
    SectionCard,
} from "./order-page-ui";

import type { CustomerOrderItem } from "./order-page-ui";
import { formatPrice } from "@/lib/formatters";
import { toast } from "sonner";

const CANCELLABLE_STATUSES = new Set([
    "Placed",
    "Confirmed",
    "Processing",
]);

export function OrderDetailsPage() {
    const { orderId } = useParams<{ orderId: string }>();
    const router = useRouter();

    const {
        authStatus,
        isLoading: isAuthLoading,
        user,
    } = useAuth();

    const isAuthenticated =
        !isAuthLoading && Boolean(authStatus && user);

    const orderQuery = useCustomerOrderDetails(
        orderId,
        isAuthenticated,
    );

    const order = extractOrderDetails(orderQuery.data);

    const cancelOrder = useCancelCustomerOrder();
    const receiptDownload = useDownloadCustomerOrderReceipt();

    const [isCancelDialogOpen, setIsCancelDialogOpen] =
        useState(false);

    const canCancelOrder =
        Boolean(order) &&
        CANCELLABLE_STATUSES.has(order?.status ?? "");

    const handleCancelOrder = async () => {
        if (
            !order ||
            !canCancelOrder ||
            cancelOrder.isPending
        ) {
            return;
        }

        try {
            await cancelOrder.mutateAsync({
                orderId: order.id,
            });

            setIsCancelDialogOpen(false);

            toast.success(
                `Order ${order.orderNumber} was cancelled successfully.`,
            );

            // Refresh the detail query so the updated status and
            // delivery progress are displayed.
            await orderQuery.refetch();
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Unable to cancel this order. Please try again.",
            );
        }
    };

    const handleDownloadReceipt = async () => {
        if (!order) {
            return;
        }

        try {
            await receiptDownload.mutateAsync({
                orderId,
                orderNumber: order.orderNumber,
            });

            toast.success(
                "Your receipt download has started.",
            );
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Unable to download the order receipt.",
            );
        }
    };

    if (
        isAuthLoading ||
        (isAuthenticated && orderQuery.isPending)
    ) {
        return (
            <main className="mx-auto w-full max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
                <OrdersLoadingState />
            </main>
        );
    }

    if (!authStatus || !user) {
        return (
            <main className="mx-auto flex min-h-[65vh] w-full max-w-6xl items-center justify-center px-4 py-12 sm:px-6">
                <div className="flex max-w-md flex-col items-center text-center">
                    <div className="flex size-16 items-center justify-center rounded-2xl border border-primary/10 bg-primary/5 text-primary">
                        <ShieldCheck className="size-7" />
                    </div>

                    <h1 className="mt-5 text-2xl font-semibold tracking-tight">
                        Sign in to view this order
                    </h1>

                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                        Sign in to your Arovia account to view the
                        order details and delivery address.
                    </p>

                    <Button
                        className="mt-6 rounded-xl"
                        onClick={() =>
                            router.push(
                                `/sign-in?returnUrl=${encodeURIComponent(
                                    `/account/orders/${orderId}`,
                                )}`,
                            )
                        }
                    >
                        Sign in
                        <ArrowRight className="ml-2 size-4" />
                    </Button>
                </div>
            </main>
        );
    }

    if (orderQuery.isError) {
        return (
            <main className="mx-auto w-full max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
                <OrdersErrorState
                    message={
                        orderQuery.error instanceof Error
                            ? orderQuery.error.message
                            : undefined
                    }
                    onRetry={() => void orderQuery.refetch()}
                />
            </main>
        );
    }

    if (!order) {
        return (
            <main className="mx-auto flex min-h-[65vh] w-full max-w-6xl items-center justify-center px-4 py-12 sm:px-6">
                <div className="flex max-w-md flex-col items-center text-center">
                    <div className="flex size-16 items-center justify-center rounded-2xl bg-muted text-muted-foreground">
                        <PackageCheck className="size-7" />
                    </div>

                    <h1 className="mt-5 text-2xl font-semibold tracking-tight">
                        We couldn’t find this order
                    </h1>

                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                        The order may not exist or may no longer
                        be available to this account.
                    </p>

                    <Button
                        variant="outline"
                        className="mt-6 rounded-xl"
                        onClick={() =>
                            router.push("/account/orders")
                        }
                    >
                        <ArrowLeft className="mr-2 size-4" />
                        Back to orders
                    </Button>
                </div>
            </main>
        );
    }

    const items = order.items ?? [];
    const address = order.shippingAddress;

    const itemQuantity = items.reduce(
        (total, item) => total + item.quantity,
        0,
    );

    const orderTitle = `Order #${order.orderNumber}`;

    return (
        <main className="min-h-[65vh] bg-background">
            <div className="mx-auto w-full max-w-6xl px-4 py-7 sm:px-6 sm:py-10 lg:px-8">
                <div className="mb-7 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
                    <button
                        type="button"
                        onClick={() => router.push("/")}
                        className="transition-colors hover:text-foreground"
                    >
                        Home
                    </button>

                    <ChevronRight className="size-3" />

                    <button
                        type="button"
                        onClick={() => router.push("/orders")}
                        className="transition-colors hover:text-foreground"
                    >
                        My orders
                    </button>

                    <ChevronRight className="size-3" />

                    <span className="font-medium text-foreground">
                        {order.orderNumber}
                    </span>
                </div>

                <motion.header
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{
                        duration: 0.3,
                        ease: "easeOut",
                    }}
                    className="mb-7"
                >
                    <Button
                        variant="ghost"
                        onClick={() => router.push("/orders")}
                        className="-ml-3 mb-5 rounded-xl px-3 text-muted-foreground hover:text-foreground"
                    >
                        <ArrowLeft className="mr-2 size-4" />
                        Back to all orders
                    </Button>

                    <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
                        <div>
                            <div className="mb-3 flex flex-wrap items-center gap-2.5">
                                <Badge
                                    variant="outline"
                                    className="rounded-full border-primary/20 bg-primary/5 px-2.5 py-1 text-primary"
                                >
                                    Order details
                                </Badge>

                                <OrderStatusBadge
                                    status={order.status}
                                />
                            </div>

                            <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">
                                {orderTitle}
                            </h1>

                            <p className="mt-2 flex flex-wrap items-center gap-x-2 gap-y-1 text-sm text-muted-foreground">
                                <CalendarDays className="size-4" />

                                Placed on{" "}
                                {formatOrderDate(
                                    order.createdAtUtc,
                                    true,
                                )}

                                <span aria-hidden="true">·</span>

                                {itemQuantity} item
                                {itemQuantity === 1 ? "" : "s"}
                            </p>
                        </div>

                        <div className="flex flex-wrap gap-2">
                            <Button
                                variant="outline"
                                onClick={handleDownloadReceipt}
                                disabled={receiptDownload.isPending}
                                className="w-fit cursor-pointer rounded-xl border-border bg-card"
                            >
                                <FileDown className="mr-2 size-4" />

                                {receiptDownload.isPending
                                    ? "Preparing receipt..."
                                    : "Download receipt"}
                            </Button>

                            <Button
                                variant="outline"
                                onClick={() =>
                                    router.push("/otc/medicine")
                                }
                                className="w-fit cursor-pointer rounded-xl border-border bg-card"
                            >
                                Shop health essentials
                                <ArrowRight className="ml-2 size-4" />
                            </Button>

                            {canCancelOrder && (
                                <Button
                                    type="button"
                                    variant="outline"
                                    disabled={cancelOrder.isPending}
                                    onClick={() =>
                                        setIsCancelDialogOpen(true)
                                    }
                                    className="w-fit cursor-pointer rounded-xl border-destructive/40 text-destructive hover:bg-destructive/10 hover:text-destructive"
                                >
                                    Cancel order
                                </Button>
                            )}
                        </div>
                    </div>
                </motion.header>

                <div className="grid items-start gap-5 lg:grid-cols-[minmax(0,1.45fr)_minmax(290px,0.8fr)]">
                    <div className="min-w-0 space-y-5">
                        <SectionCard
                            title="Delivery updates"
                            description="Follow your order as it moves through fulfilment."
                        >
                            <div className="mb-5 flex items-start gap-3 rounded-2xl bg-gradient-to-br from-primary/[0.08] to-blue-500/[0.06] p-4 sm:p-5">
                                <span className="flex size-11 shrink-0 items-center justify-center rounded-2xl border border-primary/15 bg-background text-primary">
                                    <PackageCheck className="size-5" />
                                </span>

                                <div>
                                    <h3 className="text-sm font-semibold">
                                        {getDeliveryTitle(order.status)}
                                    </h3>

                                    <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                        {getDeliveryDescription(
                                            order.status,
                                        )}
                                    </p>
                                </div>
                            </div>

                            <OrderProgress status={order.status} />
                        </SectionCard>

                        <SectionCard
                            title="Items in this order"
                            description={`${itemQuantity} item${itemQuantity === 1 ? "" : "s"} saved in your order snapshot`}
                            action={
                                <Badge
                                    variant="secondary"
                                    className="rounded-full"
                                >
                                    Arovia care
                                </Badge>
                            }
                        >
                            {items.length === 0 ? (
                                <div className="py-6 text-center text-sm text-muted-foreground">
                                    No item details were returned for
                                    this order.
                                </div>
                            ) : (
                                <div className="divide-y divide-border/70">
                                    {items.map((item) => (
                                        <OrderLineItem
                                            key={item.id}
                                            item={item}
                                        />
                                    ))}
                                </div>
                            )}
                        </SectionCard>

                        <div className="flex items-start gap-3 rounded-2xl border border-primary/10 bg-primary/[0.035] p-4 sm:p-5">
                            <ShieldCheck className="mt-0.5 size-5 shrink-0 text-primary" />

                            <div>
                                <p className="text-sm font-medium">
                                    Order details, kept clear
                                </p>

                                <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                    Prices and delivery details are
                                    shown from the snapshot saved when
                                    the order was placed.
                                </p>
                            </div>
                        </div>
                    </div>

                    <aside className="min-w-0 space-y-5">
                        <SectionCard
                            title="Order summary"
                            description="A clear breakdown of your total"
                        >
                            <OrderTotals order={order} />

                            <Separator className="my-5" />

                            <p className="text-xs leading-5 text-muted-foreground">
                                Currency: {order.currency || "INR"}.
                                All order amounts are provided by
                                Arovia’s order service.
                            </p>
                        </SectionCard>

                        <SectionCard
                            title="Delivery address"
                            description="The address saved for this order"
                        >
                            {address ? (
                                <div className="flex gap-3">
                                    <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-blue-500/10 text-blue-600 dark:text-blue-400">
                                        <MapPin className="size-5" />
                                    </span>

                                    <div className="min-w-0 text-sm leading-6">
                                        <p className="font-semibold text-foreground">
                                            {address.recipientName}
                                        </p>

                                        <p className="mt-1 text-muted-foreground">
                                            {address.addressLine1}
                                        </p>

                                        {address.addressLine2 && (
                                            <p className="text-muted-foreground">
                                                {address.addressLine2}
                                            </p>
                                        )}

                                        <p className="text-muted-foreground">
                                            {address.city},{" "}
                                            {address.state}{" "}
                                            {address.postalCode}
                                        </p>

                                        <p className="text-muted-foreground">
                                            {address.country}
                                        </p>

                                        <p className="mt-3 text-xs font-medium text-foreground">
                                            {address.recipientPhone}
                                        </p>
                                    </div>
                                </div>
                            ) : (
                                <p className="text-sm text-muted-foreground">
                                    No delivery address was returned
                                    for this order.
                                </p>
                            )}
                        </SectionCard>

                        <section className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm shadow-black/[0.02] sm:p-6">
                            <div className="flex items-center gap-3">
                                <span className="flex size-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                    <ShoppingBag className="size-5" />
                                </span>

                                <div>
                                    <h2 className="text-sm font-semibold">
                                        Need help with this order?
                                    </h2>

                                    <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                        Keep your order number handy
                                        if you contact support.
                                    </p>
                                </div>
                            </div>

                            <div className="mt-4 rounded-xl bg-muted/50 px-3 py-2.5 text-xs font-medium text-muted-foreground">
                                Reference: {order.orderNumber}
                            </div>

                            <Button
                                variant="outline"
                                onClick={() =>
                                    router.push("/account/orders")
                                }
                                className="mt-4 w-full rounded-xl border-border"
                            >
                                Back to order history
                            </Button>
                        </section>
                    </aside>
                </div>

                <Dialog
                    open={isCancelDialogOpen}
                    onOpenChange={(open) => {
                        if (!cancelOrder.isPending) {
                            setIsCancelDialogOpen(open);
                        }
                    }}
                >
                    <DialogContent className="sm:max-w-md">
                        <DialogHeader>
                            <DialogTitle>
                                Cancel order {order.orderNumber}?
                            </DialogTitle>

                            <DialogDescription>
                                Are you sure you want to cancel this
                                order? This action cannot be undone.
                                If payment has already been collected,
                                any refund will depend on the payment
                                status and applicable refund process.
                            </DialogDescription>
                        </DialogHeader>

                        <div className="rounded-xl border border-destructive/20 bg-destructive/5 p-3 text-sm leading-5 text-muted-foreground">
                            Your order will remain available in your
                            order history for reference.
                        </div>

                        <DialogFooter className="gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                disabled={cancelOrder.isPending}
                                onClick={() =>
                                    setIsCancelDialogOpen(false)
                                }
                            >
                                Keep order
                            </Button>

                            <Button
                                type="button"
                                variant="destructive"
                                disabled={cancelOrder.isPending}
                                onClick={() =>
                                    void handleCancelOrder()
                                }
                            >
                                {cancelOrder.isPending
                                    ? "Cancelling..."
                                    : "Confirm cancellation"}
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>
        </main>
    );
}

function OrderLineItem({
    item,
}: {
    item: CustomerOrderItem;
}) {
    return (
        <div className="flex gap-3 py-4 first:pt-0 last:pb-0 sm:gap-4">
            <OrderItemImage item={item} size="large" />

            <div className="min-w-0 flex-1 py-0.5">
                <h3 className="text-sm font-semibold leading-5">
                    {item.productName}
                </h3>

                <p className="mt-1 text-xs text-muted-foreground">
                    {item.sku ? `SKU: ${item.sku} · ` : ""}
                    {item.packageQuantity != null
                        ? `${item.packageQuantity} ${item.packageUnit ?? ""}`.trim()
                        : "Healthcare essential"}
                </p>

                <p className="mt-2 text-xs text-muted-foreground">
                    Qty {item.quantity} ·{" "}
                    {formatPrice(item.unitSellingPrice)} each
                </p>

                {item.requiresPrescription && (
                    <Badge
                        variant="outline"
                        className="mt-2 rounded-full border-amber-200 bg-amber-50 text-[10px] text-amber-800 dark:border-amber-900/70 dark:bg-amber-950/30 dark:text-amber-300"
                    >
                        Prescription required
                    </Badge>
                )}
            </div>

            <div className="shrink-0 pt-0.5 text-right text-sm font-semibold">
                {formatPrice(item.lineTotal)}
            </div>
        </div>
    );
}

function getDeliveryTitle(status: string) {
    switch (status) {
        case "Placed":
            return "Your order has been placed";
        case "Confirmed":
            return "Your order is confirmed";
        case "Processing":
            return "We’re preparing your order";
        case "Shipped":
            return "Your order is on its way";
        case "OutForDelivery":
            return "Your order is out for delivery";
        case "Delivered":
            return "Your order has arrived";
        case "Cancelled":
            return "This order was cancelled";
        default:
            return "Your order status";
    }
}

function getDeliveryDescription(status: string) {
    switch (status) {
        case "Placed":
            return "Payment confirmation is still pending. The order will move forward once payment is verified.";
        case "Confirmed":
            return "Payment is confirmed and your order is ready for the next step.";
        case "Processing":
            return "Your items are being prepared with care for dispatch.";
        case "Shipped":
            return "Your package has left the fulfilment centre. We’ll show updates here as they become available.";
        case "OutForDelivery":
            return "Your package is with the delivery partner. Please keep your phone available for delivery updates.";
        case "Delivered":
            return "Your order is marked as delivered. We hope these essentials serve you well.";
        case "Cancelled":
            return "The order record remains available here for your reference.";
        default:
            return "Check this page for the latest information returned for your order.";
    }
}