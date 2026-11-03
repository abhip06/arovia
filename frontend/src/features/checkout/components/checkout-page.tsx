"use client";

import Image from "next/image";
import Script from "next/script";
import { useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { motion } from "framer-motion";

import {
    ArrowLeft,
    ArrowRight,
    BadgeCheck,
    Briefcase,
    Building2,
    CheckCircle2,
    CircleAlert,
    House,
    LucideIcon,
    MapPin,
    Minus,
    PackageCheck,
    Plus,
    ShieldCheck,
    ShoppingBag,
    Trash2,
    Truck,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

import { useAuth } from "@/features/auth/context/auth-context";

import {
    useRemoveCartItem,
    useSetCartItemQuantity,
} from "@/features/cart/hooks/use-cart";

import { useCheckout } from "@/features/checkout/hooks/use-checkout";

import { useAddresses } from "@/features/addresses/hooks/use-addresses";
import type { Address } from "@/types/address.types";

import type { CheckoutItem } from "@/types/checkout.types";
import { formatPrice } from "@/lib/formatters";

import { CheckoutSkeleton } from "./checkout-skeleton";
import { AddressFormDialog } from "@/features/addresses/components/address-form-dialog";
import { useCreateOrder } from "@/features/orders/hooks/use-orders";
import {
    useCreatePaymentSession,
    useVerifyRazorpayPayment,
} from "@/features/payments/hooks/use-payments";
import type { CreateOrderStatus } from "@/types/order.types";
import { DeliveryPincodeRequiredState } from "./delivery-pincode-state";
import { useRequestPincode } from "@/features/location/use-request-pincode";

const EMPTY_ADDRESSES: Address[] = [];

export function CheckoutPage() {
    const router = useRouter();

    const pincode = useRequestPincode();

    const {
        authStatus,
        isLoading: isAuthLoading,
        user,
    } = useAuth();

    const isAuthenticated =
        !isAuthLoading &&
        authStatus &&
        user;

    const [preferredAddressId, setPreferredAddressId] =
        useState<string | null>(null);

    const [isAddressDialogOpen, setIsAddressDialogOpen] =
        useState(false);

    const addressesQuery = useAddresses(
        !isAuthLoading && authStatus,
    );

    const addresses = addressesQuery.data ?? EMPTY_ADDRESSES;

    const selectedAddressId = addresses.some((address) => address.id === preferredAddressId)
        ? preferredAddressId
        : (addresses.find((address) => address.isPrimary) ?? addresses[0])?.id ?? null;

    const selectedAddress =
        addresses.find(
            (address) => address.id === selectedAddressId,
        ) ?? null;

    /*
     * Checkout is keyed by the selected address pincode.
     *
     * Until an address is selected, null is passed and the backend
     * may use its IP-based fallback. We still prevent final
     * continuation until a real saved address is selected.
     */
    const checkoutQuery = useCheckout(
        pincode,
        Boolean(pincode),
    );
    const setQuantity =
        useSetCartItemQuantity();

    const removeItem =
        useRemoveCartItem();

    const createOrder = useCreateOrder({ deferCheckoutInvalidation: true });
    const createPaymentSession = useCreatePaymentSession();
    const verifyPayment = useVerifyRazorpayPayment();

    const [isRazorpayLoaded, setIsRazorpayLoaded] = useState(false);
    const [pendingOrder, setPendingOrder] = useState<{
        id: string;
        orderNumber: string;
    } | null>(null);
    const [isPaymentProcessing, setIsPaymentProcessing] = useState(false);

    const orderAttemptRef = useRef<{
        idempotencyKey: string;
        shippingAddressId: string;
    } | null>(null);
    const paymentAttemptKeyRef = useRef<string | null>(null);
    const paymentCallbackReceivedRef = useRef(false);

    const [updatingItemId, setUpdatingItemId] =
        useState<string | null>(null);

    const [removingItemId, setRemovingItemId] =
        useState<string | null>(null);

    const checkout = checkoutQuery.data;

    const items = useMemo(() => checkout?.items ?? [], [checkout?.items]);

    const unavailableItems = useMemo(
        () =>
            items.filter(
                (item) =>
                    !item.isAvailable ||
                    item.unitPrice == null ||
                    item.lineTotal == null,
            ),
        [items],
    );

    const hasUnavailableItems =
        Boolean(checkout?.hasUnavailableItems) ||
        unavailableItems.length > 0;

    const handleQuantity = async (
        item: CheckoutItem,
        quantity: number,
    ) => {
        if (
            pendingOrder ||
            quantity < 1 ||
            quantity > 25 ||
            quantity === item.quantity
        ) {
            return;
        }

        setUpdatingItemId(item.id);

        try {
            await setQuantity.mutateAsync({
                cartItemId: item.id,
                quantity,
            });
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Could not update this item.",
            );
        } finally {
            setUpdatingItemId(null);
        }
    };

    const handleRemove = async (
        item: CheckoutItem,
    ) => {
        if (pendingOrder) {
            return;
        }

        setRemovingItemId(item.id);

        try {
            await removeItem.mutateAsync(item.id);

            toast.success(
                `${item.productName} removed from your cart.`,
            );
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Could not remove this item.",
            );
        } finally {
            setRemovingItemId(null);
        }
    };

    const startPayment = async (
        orderId: string,
        orderNumber: string,
    ) => {
        if (!isRazorpayLoaded || !window.Razorpay) {
            toast.error("Secure checkout is still loading. Please try again.");
            return;
        }

        if (isPaymentProcessing || createPaymentSession.isPending) {
            return;
        }

        setIsPaymentProcessing(true);
        paymentCallbackReceivedRef.current = false;

        const idempotencyKey =
            paymentAttemptKeyRef.current ?? crypto.randomUUID();
        paymentAttemptKeyRef.current = idempotencyKey;

        try {
            const session = await createPaymentSession.mutateAsync({
                orderId,
                idempotencyKey,
            });

            const RazorpayCheckout = window.Razorpay;
            if (!RazorpayCheckout) {
                throw new Error("Secure checkout could not be loaded. Please try again.");
            }

            const customerName = [user?.firstName, user?.lastName]
                .filter(Boolean)
                .join(" ");

            const razorpay = new RazorpayCheckout({
                key: session.keyId,
                amount: session.amountSubunits,
                currency: session.currency,
                name: "Arovia",
                description: `Payment for order ${orderNumber}`,
                order_id: session.providerOrderId,
                prefill: {
                    ...(customerName ? { name: customerName } : {}),
                    ...(user?.email ? { email: user.email } : {}),
                },
                theme: {
                    color: "#16a34a",
                },
                handler: async (response) => {
                    paymentCallbackReceivedRef.current = true;

                    try {
                        const verification = await verifyPayment.mutateAsync({
                            paymentAttemptId: session.paymentAttemptId,
                            providerOrderId: response.razorpay_order_id,
                            providerPaymentId: response.razorpay_payment_id,
                            signature: response.razorpay_signature,
                        });

                        switch (verification.status) {
                            case "Captured":
                            case "AlreadyCaptured":
                                paymentAttemptKeyRef.current = null;
                                setPendingOrder(null);
                                toast.success(
                                    `Payment successful. Order ${orderNumber} is confirmed.`,
                                );
                                router.replace("/orders");
                                break;

                            case "PaymentPending":
                                toast.info(
                                    "Payment is still being verified. Your order remains placed until capture is confirmed.",
                                );
                                break;

                            case "PaymentFailed":
                                paymentAttemptKeyRef.current = crypto.randomUUID();
                                toast.error("Payment failed. You can retry payment.");
                                break;

                            case "ReconciliationRequired":
                                toast.error(
                                    "Your payment requires verification. Please do not retry immediately.",
                                );
                                break;

                            case "InvalidSignature":
                            case "InvalidPayment":
                                toast.error(
                                    "We couldn't verify this payment. Your order has not been confirmed.",
                                );
                                break;

                            case "PaymentNotFound":
                            case "OrderNotFound":
                                toast.error(
                                    "We couldn't find the payment or order details. Please check your order history.",
                                );
                                break;

                            case "OrderNotPayable":
                                toast.error(
                                    "This order can no longer be paid through checkout.",
                                );
                                break;
                        }
                    } catch (error) {
                        toast.error(
                            error instanceof Error
                                ? error.message
                                : "We couldn't verify the payment yet. Retry verification before making another payment.",
                        );
                    } finally {
                        setIsPaymentProcessing(false);
                    }
                },
                modal: {
                    ondismiss: () => {
                        if (!paymentCallbackReceivedRef.current) {
                            setIsPaymentProcessing(false);
                            toast.info(
                                `Order ${orderNumber} is placed but payment is pending. You can retry payment here.`,
                            );
                        }
                    },
                },
            });

            razorpay.on("payment.failed", (failure) => {
                // Keep the current checkout open for its built-in retry flow.
                // A fresh application idempotency key is used if the customer
                // closes Checkout and starts a new payment attempt.
                paymentAttemptKeyRef.current = crypto.randomUUID();
                toast.error(
                    failure.error.description ||
                    "Payment failed. You can retry or choose another payment method.",
                );
            });

            razorpay.open();
        } catch (error) {
            console.error(
                "Failed to initialize Razorpay Checkout:",
                error,
            );

            setIsPaymentProcessing(false);

            const message =
                error instanceof Error && error.message.trim()
                    ? error.message
                    : "We couldn't start payment. Your order has been placed. Please retry payment.";

            toast.error(message);
        }
    };

    const handlePlaceOrder = async () => {
        if (pendingOrder) {
            await startPayment(pendingOrder.id, pendingOrder.orderNumber);
            return;
        }

        if (createOrder.isPending || isPaymentProcessing) {
            return;
        }

        if (!selectedAddress) {
            toast.error(
                "Select a delivery address before placing your order.",
            );
            return;
        }

        if (hasUnavailableItems) {
            toast.error(
                "Remove unavailable products before placing your order.",
            );
            return;
        }

        if (checkoutQuery.isFetching || !checkoutQuery.isSuccess) {
            toast.error(
                "Your checkout is still being updated. Please try again.",
            );
            return;
        }

        // Reuse the key after an uncertain network failure so a retry
        // cannot accidentally create a second order.
        const existingAttempt = orderAttemptRef.current;
        const attempt =
            existingAttempt?.shippingAddressId === selectedAddress.id
                ? existingAttempt
                : {
                    idempotencyKey: crypto.randomUUID(),
                    shippingAddressId: selectedAddress.id,
                };

        orderAttemptRef.current = attempt;

        try {
            const result = await createOrder.mutateAsync({
                request: {
                    shippingAddressId: selectedAddress.id,
                },
                idempotencyKey: attempt.idempotencyKey,
            });

            if (
                result.status === "Created" ||
                result.status === "Replayed"
            ) {
                if (!result.order) {
                    toast.error(
                        "Your order may have been placed, but its details were not returned. Retry with the same request or check your order history.",
                    );
                    return;
                }

                orderAttemptRef.current = null;
                paymentAttemptKeyRef.current = crypto.randomUUID();
                setPendingOrder({
                    id: result.order.id,
                    orderNumber: result.order.orderNumber,
                });

                toast.success(
                    result.status === "Replayed"
                        ? `Order ${result.order.orderNumber} was already placed. Continue to payment.`
                        : `Order ${result.order.orderNumber} has been placed. Complete payment to confirm it.`,
                );

                await startPayment(
                    result.order.id,
                    result.order.orderNumber,
                );
                return;
            }

            // These business responses confirm that this attempt did not
            // create an order, so a later attempt can use a fresh key.
            if (result.status !== "IdempotencyConflict") {
                orderAttemptRef.current = null;
            }

            if (
                result.status === "CartEmpty" ||
                result.status === "ItemsUnavailable" ||
                result.status === "InsufficientStock"
            ) {
                await checkoutQuery.refetch();
            }

            if (result.status === "ShippingAddressNotFound") {
                await addressesQuery.refetch();
            }

            toast.error(getCreateOrderStatusMessage(result.status));
        } catch (error) {
            // Keep the same idempotency key after transport/server errors:
            // the server may have committed the order before the response failed.
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Could not place your order. Retry this request to safely check its result.",
            );
        }
    };

    function isDeliveryPincodeRequired(error: unknown): boolean {
        if (!(error instanceof Error)) {
            return false;
        }

        return error.message
            .toLowerCase()
            .includes("could not determine your delivery pincode");
    }

    if (isAuthLoading) {
        return <CheckoutSkeleton />;
    }

    if (!authStatus || !user) {
        return (
            <CheckoutState
                icon={ShieldCheck}
                title="Sign in to continue"
                description="Your checkout is connected to your Arovia account. Sign in to access your cart and continue securely."
                actionLabel="Sign in"
                onAction={() =>
                    router.push("/sign-in")
                }
            />
        );
    }

    if (!pincode) {
        return <DeliveryPincodeRequiredState />;
    }

    if (addressesQuery.isPending) {
        return <CheckoutSkeleton />;
    }

    if (addressesQuery.isError) {
        return (
            <CheckoutState
                icon={CircleAlert}
                title="We couldn't load your addresses"
                description={
                    addressesQuery.error.message ||
                    "Something went wrong while loading your saved addresses."
                }
                actionLabel="Try again"
                onAction={() =>
                    void addressesQuery.refetch()
                }
            />
        );
    }

    if (checkoutQuery.isPending) {
        return <CheckoutSkeleton />;
    }

    if (checkoutQuery.isError) {
        if (isDeliveryPincodeRequired(checkoutQuery.error)) {
            return <DeliveryPincodeRequiredState />;
        }

        return (
            <CheckoutState
                icon={CircleAlert}
                title="We couldn't prepare your checkout"
                description={
                    checkoutQuery.error.message ||
                    "Something went wrong while preparing your checkout."
                }
                actionLabel="Try again"
                onAction={() =>
                    void checkoutQuery.refetch()
                }
            />
        );
    }

    if (items.length === 0) {
        return (
            <CheckoutState
                icon={ShoppingBag}
                title="Your cart is empty"
                description="There are no products to check out yet. Explore our healthcare essentials and add what you need."
                actionLabel="Start shopping"
                onAction={() =>
                    router.push("/otc/medicine")
                }
            />
        );
    }

    const firstName =
        user.firstName || "USER";

    return (
        <main className="min-h-screen bg-background">
            <Script
                src="https://checkout.razorpay.com/v1/checkout.js"
                strategy="afterInteractive"
                onLoad={() => setIsRazorpayLoaded(true)}
                onError={() => {
                    setIsRazorpayLoaded(false);
                    toast.error("Unable to load secure payment checkout.");
                }}
            />
            <div className="container mx-auto px-4 pb-16 pt-24 sm:px-6 lg:px-8">
                {/* Page heading */}
                <motion.div
                    initial={{
                        opacity: 0,
                        y: 12,
                    }}
                    animate={{
                        opacity: 1,
                        y: 0,
                    }}
                    transition={{
                        duration: 0.4,
                        ease: "easeOut",
                    }}
                    className="mx-auto max-w-7xl"
                >
                    <div className="flex flex-col gap-2">
                        <p className="text-base font-medium text-primary">
                            Almost there, {firstName}
                        </p>

                        <p className="max-w-2xl text-sm text-muted-foreground">
                            {pendingOrder
                                ? "Your order has been placed. Complete payment to confirm it."
                                : "Review your order and confirm your delivery details before placing it."}
                        </p>
                    </div>
                </motion.div>

                <div className="mx-auto mt-8 grid max-w-7xl gap-6 lg:grid-cols-[minmax(0,1fr)_380px] lg:items-start">
                    {pendingOrder && (
                        <div className="lg:col-span-2 rounded-2xl border border-primary/20 bg-primary/5 p-4" role="status">
                            <p className="text-sm font-semibold text-foreground">
                                Order {pendingOrder.orderNumber} placed — payment pending
                            </p>
                            <p className="mt-1 text-sm text-muted-foreground">
                                Your order will be confirmed only after Arovia verifies successful payment capture. You can retry payment using the button in the order summary.
                            </p>
                        </div>
                    )}
                    {/* Main content */}
                    <div className="flex min-w-0 flex-col gap-6">
                        {/* Order items */}
                        <CheckoutCard
                            title="Your items"
                            description={`${checkout?.itemCount ?? 0} ${checkout?.itemCount === 1
                                ? "item"
                                : "items"
                                } in this order`}
                            icon={ShoppingBag}
                        >
                            <div className="flex flex-col divide-y divide-border/70">
                                {items.map((item) => (
                                    <CheckoutItem
                                        key={item.id}
                                        item={item}
                                        isUpdating={
                                            updatingItemId === item.id ||
                                            createOrder.isPending ||
                                            pendingOrder !== null
                                        }
                                        isRemoving={
                                            removingItemId === item.id ||
                                            createOrder.isPending ||
                                            pendingOrder !== null
                                        }
                                        onQuantity={
                                            handleQuantity
                                        }
                                        onRemove={
                                            handleRemove
                                        }
                                    />
                                ))}
                            </div>
                        </CheckoutCard>

                        {/* Delivery address */}
                        <CheckoutCard
                            title="Delivery address"
                            description="Choose where you would like your order delivered."
                            icon={MapPin}
                        >
                            {addresses.length === 0 ? (
                                <div className="rounded-2xl border border-dashed border-border bg-muted/30 p-5">
                                    <p className="text-sm font-semibold">
                                        Add your first delivery address
                                    </p>

                                    <p className="mt-1 text-sm text-muted-foreground">
                                        Save your delivery details to continue with checkout.
                                    </p>

                                    <Button
                                        type="button"
                                        className="mt-4 rounded-xl"
                                        disabled={createOrder.isPending || pendingOrder !== null}
                                        onClick={() => setIsAddressDialogOpen(true)}
                                    >
                                        <Plus className="mr-2 size-4" />
                                        Add new address
                                    </Button>
                                </div>
                            ) : (
                                <div className="flex flex-col gap-3">
                                    <fieldset className="flex flex-col gap-3">
                                        <legend className="mb-3 text-sm text-muted-foreground">
                                            Select a delivery address
                                        </legend>

                                        {addresses.map((address) => {
                                            const isSelected = address.id === selectedAddressId;
                                            const labelStyle = getAddressLabelStyle(address.label);
                                            const LabelIcon = labelStyle.icon;

                                            return (
                                                <label
                                                    key={address.id}
                                                    className={[
                                                        "group relative flex cursor-pointer gap-3.5 rounded-2xl border p-4 sm:gap-4 sm:p-5",
                                                        "transition-all duration-200",
                                                        "focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2",
                                                        isSelected
                                                            ? "border-primary/60 bg-primary/[0.035] shadow-sm"
                                                            : "border-border/70 bg-card hover:border-primary/30 hover:bg-muted/20",
                                                    ].join(" ")}
                                                >
                                                    <input
                                                        type="radio"
                                                        name="delivery-address"
                                                        value={address.id}
                                                        checked={isSelected}
                                                        disabled={createOrder.isPending}
                                                        onChange={() => setPreferredAddressId(address.id)}
                                                        className="sr-only"
                                                    />

                                                    {/* Address label icon */}
                                                    <div
                                                        className={[
                                                            "flex size-11 shrink-0 items-center justify-center rounded-xl",
                                                            "transition-colors duration-200 sm:size-12",
                                                            labelStyle.container,
                                                        ].join(" ")}
                                                    >
                                                        <LabelIcon className="size-5 sm:size-[22px]" />
                                                    </div>

                                                    {/* Address details */}
                                                    <div className="min-w-0 flex-1">
                                                        {/* Label and status */}
                                                        <div className="flex flex-wrap items-center gap-2">
                                                            <span className="text-sm font-semibold text-foreground">
                                                                {address.label?.trim() || "Other"}
                                                            </span>

                                                            {address.isPrimary && (
                                                                <span className="inline-flex items-center rounded-full border border-primary/15 bg-primary/5 px-2 py-0.5 text-[10px] font-medium text-primary">
                                                                    Primary
                                                                </span>
                                                            )}
                                                        </div>

                                                        {/* Recipient */}
                                                        <p className="mt-1.5 text-sm font-medium text-foreground">
                                                            {address.recipientName}
                                                        </p>

                                                        {address.recipientPhone && (
                                                            <p className="mt-0.5 text-xs text-muted-foreground">
                                                                {address.recipientPhone}
                                                            </p>
                                                        )}

                                                        {/* Full address */}
                                                        <p className="mt-2.5 text-sm leading-6 text-muted-foreground">
                                                            {address.addressLine1}
                                                            {address.addressLine2
                                                                ? `, ${address.addressLine2}`
                                                                : ""}
                                                            <br />
                                                            {address.city}, {address.state}{" "}
                                                            {address.postalCode}
                                                            <br />
                                                            {address.country}
                                                        </p>
                                                    </div>

                                                    {/* Selection indicator */}
                                                    <div
                                                        className={[
                                                            "flex size-5 shrink-0 items-center justify-center rounded-full border transition-all duration-200",
                                                            isSelected
                                                                ? "border-primary bg-primary text-primary-foreground"
                                                                : "border-border bg-background group-hover:border-primary/40",
                                                        ].join(" ")}
                                                        aria-hidden="true"
                                                    >
                                                        {isSelected && (
                                                            <CheckCircle2 className="size-4" />
                                                        )}
                                                    </div>
                                                </label>
                                            );
                                        })}
                                    </fieldset>

                                    <Button
                                        type="button"
                                        variant="outline"
                                        className="h-11 w-full rounded-xl sm:w-auto"
                                        disabled={createOrder.isPending}
                                        onClick={() => setIsAddressDialogOpen(true)}
                                    >
                                        <Plus className="mr-2 size-4" />
                                        Add new address
                                    </Button>
                                </div>
                            )}
                        </CheckoutCard>

                        {/* Availability */}
                        {hasUnavailableItems && (
                            <motion.div
                                initial={{
                                    opacity: 0,
                                    y: 8,
                                }}
                                animate={{
                                    opacity: 1,
                                    y: 0,
                                }}
                                className="rounded-2xl border border-amber-500/20 bg-amber-500/5 p-4 sm:p-5"
                            >
                                <div className="flex items-start gap-3">
                                    <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-amber-500/10 text-amber-700 dark:text-amber-300">
                                        <CircleAlert className="size-4" />
                                    </div>

                                    <div className="flex min-w-0 flex-col gap-1">
                                        <p className="text-sm font-semibold text-foreground">
                                            Action needed before
                                            checkout
                                        </p>

                                        <p className="text-sm text-muted-foreground">
                                            Some products in your cart
                                            are currently unavailable.
                                            Remove them before
                                            continuing.
                                        </p>
                                    </div>
                                </div>
                            </motion.div>
                        )}

                        {/* Delivery */}
                        <CheckoutCard
                            title="Delivery"
                            description="What to expect from your Arovia order."
                            icon={Truck}
                        >
                            <div className="grid gap-3 sm:grid-cols-2">
                                <InfoTile
                                    icon={Truck}
                                    title="Delivery"
                                    description={
                                        selectedAddress
                                            ? "Delivery charges and estimated delivery time are calculated using your selected delivery address."
                                            : "Select a delivery address to calculate delivery charges and availability."
                                    }
                                />

                                <InfoTile
                                    icon={PackageCheck}
                                    title="Order tracking"
                                    description="Track your order from dispatch to delivery through your Arovia account."
                                />
                            </div>
                        </CheckoutCard>

                        <CheckoutCard
                            title="Secure payment"
                            description="Complete your payment through Razorpay Checkout."
                            icon={ShieldCheck}
                        >
                            <div className="flex items-start gap-3 rounded-2xl border border-primary/10 bg-primary/5 p-4">
                                <ShieldCheck className="mt-0.5 size-5 shrink-0 text-primary" />
                                <div className="flex flex-col gap-1">
                                    <p className="text-sm font-semibold text-foreground">
                                        Cards, net banking, and UPI
                                    </p>
                                    <p className="text-sm leading-6 text-muted-foreground">
                                        Razorpay securely handles payment details. Arovia confirms your order only after the backend verifies successful payment capture.
                                    </p>
                                </div>
                            </div>
                        </CheckoutCard>

                        {/* Trust */}
                        <div className="flex items-start gap-3 px-1">
                            <ShieldCheck className="mt-0.5 size-4 shrink-0 text-primary" />

                            <p className="text-xs text-muted-foreground">
                                Your checkout is protected by your
                                Arovia account session. We use your
                                information only to process and
                                deliver your order.
                            </p>
                        </div>
                    </div>

                    {/* Summary */}
                    <aside className="lg:sticky lg:top-24">
                        <CheckoutSummary
                            subtotal={
                                checkout?.subtotal ?? 0
                            }
                            discountTotal={
                                checkout?.discountTotal ?? 0
                            }
                            totalTax={
                                checkout?.totalTax ?? 0
                            }
                            shippingFee={
                                checkout?.shippingFee ?? 0
                            }
                            grandTotal={
                                checkout?.grandTotal ?? 0
                            }
                            itemCount={
                                checkout?.itemCount ?? 0
                            }
                            hasUnavailableItems={
                                hasUnavailableItems
                            }
                            hasSelectedAddress={
                                selectedAddress !== null
                            }
                            isUpdating={
                                checkoutQuery.isFetching
                            }
                            isPlacingOrder={
                                createOrder.isPending ||
                                createPaymentSession.isPending ||
                                isPaymentProcessing ||
                                verifyPayment.isPending
                            }
                            isPaymentGatewayReady={isRazorpayLoaded}
                            hasPendingOrder={pendingOrder !== null}
                            pendingOrderNumber={pendingOrder?.orderNumber ?? null}
                            onPlaceOrder={
                                handlePlaceOrder
                            }
                            onBack={() => {
                                if (pendingOrder) {
                                    toast.info(
                                        "Your order is placed but payment is still pending. Complete or retry payment before leaving this checkout.",
                                    );
                                    return;
                                }

                                router.push("/otc/medicine");
                            }}
                        />
                    </aside>
                </div>
            </div>
            <AddressFormDialog
                open={isAddressDialogOpen}
                onOpenChange={setIsAddressDialogOpen}
                onCreated={(address) => {
                    setPreferredAddressId(address.id);
                }}
            />
        </main>
    );
}

function CheckoutCard({
    title,
    description,
    icon: Icon,
    children,
}: {
    title: string;
    description: string;
    icon: React.ComponentType<{
        className?: string;
    }>;
    children: React.ReactNode;
}) {
    return (
        <section className="rounded-3xl border border-border/70 bg-card p-4 sm:p-6">
            <div className="mb-5 flex items-start gap-3">
                <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                    <Icon className="size-5" />
                </div>

                <div className="flex min-w-0 flex-col gap-1">
                    <h2 className="text-base font-semibold text-foreground sm:text-lg">
                        {title}
                    </h2>

                    <p className="text-sm text-muted-foreground">
                        {description}
                    </p>
                </div>
            </div>

            {children}
        </section>
    );
}

function CheckoutItem({
    item,
    isUpdating,
    isRemoving,
    onQuantity,
    onRemove,
}: {
    item: CheckoutItem;
    isUpdating: boolean;
    isRemoving: boolean;
    onQuantity: (
        item: CheckoutItem,
        quantity: number,
    ) => void;
    onRemove: (
        item: CheckoutItem,
    ) => void;
}) {
    const unavailable =
        !item.isAvailable ||
        item.unitPrice == null ||
        item.lineTotal == null;

    const originalTotal =
        item.mrp != null
            ? item.mrp * item.quantity
            : null;

    const hasSavings =
        !unavailable &&
        originalTotal != null &&
        item.lineTotal != null &&
        originalTotal > item.lineTotal;

    return (
        <motion.article
            layout
            initial={{
                opacity: 0,
                y: 8,
            }}
            animate={{
                opacity: 1,
                y: 0,
            }}
            exit={{
                opacity: 0,
                x: -16,
            }}
            transition={{
                duration: 0.22,
                ease: [0.22, 1, 0.36, 1],
            }}
            className="flex gap-4 py-5 first:pt-0 last:pb-0"
        >
            {/* Product image */}
            <div className="relative size-20 shrink-0 overflow-hidden rounded-xl border border-border/60 bg-primary/5 sm:size-24">
                {item.imageUrl ? (
                    <Image
                        src={item.imageUrl}
                        alt={item.productName}
                        fill
                        sizes="(min-width: 640px) 96px, 80px"
                        className="object-contain p-2"
                    />
                ) : (
                    <div className="flex size-full items-center justify-center text-primary">
                        <ShoppingBag className="size-7 opacity-60" />
                    </div>
                )}
            </div>

            {/* Details */}
            <div className="flex min-w-0 flex-1 flex-col gap-3">
                <div className="flex min-w-0 items-start justify-between gap-3">
                    <div className="min-w-0 flex-1">
                        <h3 className="line-clamp-2 text-sm font-semibold text-foreground">
                            {item.productName}
                        </h3>

                        <p className="mt-1 text-xs text-muted-foreground">
                            {item.packageQuantity}{" "}
                            {item.packageUnit}
                        </p>

                        {item.requiresPrescription && (
                            <Badge
                                variant="secondary"
                                className="mt-2 rounded-md text-[10px]"
                            >
                                Prescription
                            </Badge>
                        )}
                    </div>

                    <div className="flex shrink-0 flex-col items-end gap-1">
                        {unavailable ? (
                            <Badge
                                variant="outline"
                                className="border-amber-500/30 text-amber-700 dark:text-amber-300"
                            >
                                Unavailable
                            </Badge>
                        ) : (
                            <span className="text-sm font-semibold text-foreground">
                                {formatPrice(
                                    item.unitPrice!,
                                )}
                            </span>
                        )}

                        {hasSavings && (
                            <span className="text-[11px] text-muted-foreground line-through">
                                {formatPrice(
                                    item.mrp!,
                                )}
                            </span>
                        )}
                    </div>
                </div>

                <div className="flex flex-wrap items-center justify-between gap-3">
                    {unavailable ? (
                        <p className="text-xs text-amber-700 dark:text-amber-300">
                            This item cannot currently be ordered.
                        </p>
                    ) : (
                        <div className="inline-flex items-center rounded-full border border-border/70 bg-muted/40 p-1">
                            <QuantityButton
                                label={`Decrease ${item.productName} quantity`}
                                disabled={
                                    isUpdating ||
                                    item.quantity <= 1
                                }
                                onClick={() =>
                                    onQuantity(
                                        item,
                                        item.quantity - 1,
                                    )
                                }
                            >
                                <Minus className="size-3" />
                            </QuantityButton>

                            <span
                                aria-live="polite"
                                className="min-w-9 px-1 text-center text-xs font-semibold text-foreground"
                            >
                                {item.quantity}
                            </span>

                            <QuantityButton
                                label={`Increase ${item.productName} quantity`}
                                disabled={
                                    isUpdating ||
                                    item.quantity >= 25
                                }
                                onClick={() =>
                                    onQuantity(
                                        item,
                                        item.quantity + 1,
                                    )
                                }
                            >
                                <Plus className="size-3" />
                            </QuantityButton>
                        </div>
                    )}

                    <div className="flex items-center gap-2">
                        {hasSavings && (
                            <span className="hidden text-xs font-medium text-primary sm:block">
                                Saved{" "}
                                {formatPrice(
                                    originalTotal! -
                                    item.lineTotal!,
                                )}
                            </span>
                        )}

                        <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            aria-label={`Remove ${item.productName}`}
                            disabled={isRemoving}
                            onClick={() =>
                                onRemove(item)
                            }
                            className="h-8 rounded-lg px-2.5 text-xs text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                        >
                            {isRemoving ? (
                                <span className="mr-1.5 size-3.5 animate-spin rounded-full border-2 border-current border-t-transparent" />
                            ) : (
                                <Trash2 className="mr-1.5 size-3.5" />
                            )}

                            Remove
                        </Button>
                    </div>
                </div>
            </div>
        </motion.article>
    );
}

function QuantityButton({
    children,
    label,
    disabled,
    onClick,
}: {
    children: React.ReactNode;
    label: string;
    disabled: boolean;
    onClick: () => void;
}) {
    return (
        <button
            type="button"
            aria-label={label}
            disabled={disabled}
            onClick={onClick}
            className="flex size-7 items-center justify-center rounded-full bg-background text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-40"
        >
            {children}
        </button>
    );
}

function CheckoutSummary({
    subtotal,
    discountTotal,
    totalTax,
    shippingFee,
    grandTotal,
    itemCount,
    hasUnavailableItems,
    hasSelectedAddress,
    isUpdating,
    isPlacingOrder,
    isPaymentGatewayReady,
    hasPendingOrder,
    pendingOrderNumber,
    onPlaceOrder,
    onBack,
}: {
    subtotal: number;
    discountTotal: number;
    totalTax: number;
    shippingFee: number;
    grandTotal: number;
    itemCount: number;
    hasUnavailableItems: boolean;
    hasSelectedAddress: boolean;
    isUpdating: boolean;
    isPlacingOrder: boolean;
    isPaymentGatewayReady: boolean;
    hasPendingOrder: boolean;
    pendingOrderNumber: string | null;
    onPlaceOrder: () => void;
    onBack: () => void;
}) {
    const canProceed =
        isPaymentGatewayReady &&
        !isUpdating &&
        !isPlacingOrder &&
        (hasPendingOrder || (hasSelectedAddress && !hasUnavailableItems));

    return (
        <div className="rounded-3xl border border-border/70 bg-card p-5 shadow-sm sm:p-6">
            <div className="flex flex-col gap-1">
                <p className="text-sm font-medium text-primary">
                    Order summary
                </p>

                <h2 className="text-xl font-semibold tracking-tight text-foreground">
                    Review your order
                </h2>
            </div>

            <Separator className="my-5" />

            <div className="flex flex-col gap-3">
                <div className="flex items-center justify-between gap-4">
                    <span className="text-sm text-muted-foreground">
                        Items
                    </span>

                    <span className="text-sm font-medium text-foreground">
                        {itemCount}
                    </span>
                </div>

                <div className="flex items-center justify-between gap-4">
                    <span className="text-sm text-muted-foreground">
                        Subtotal
                    </span>

                    <span className="text-sm font-medium text-foreground">
                        {formatPrice(subtotal)}
                    </span>
                </div>

                <div className="flex items-center justify-between gap-4">
                    <span className="text-sm text-muted-foreground">
                        Discount
                    </span>

                    <span className="text-sm font-medium text-primary">
                        - {formatPrice(discountTotal)}
                    </span>
                </div>

                <div className="flex items-center justify-between gap-4">
                    <span className="text-sm text-muted-foreground">
                        Taxes
                    </span>

                    <span className="text-sm font-medium text-foreground">
                        {formatPrice(totalTax)}
                    </span>
                </div>

                <div className="flex items-center justify-between gap-4">
                    <span className="text-sm text-muted-foreground">
                        Delivery
                    </span>

                    {shippingFee === 0
                        ? (<span className="text-sm font-medium text-primary">
                            FREE
                        </span>)
                        : (<span className="text-sm font-medium text-foreground">
                            {formatPrice(shippingFee)}
                        </span>)}
                </div>
            </div>

            <Separator className="my-5" />

            <div className="flex items-end justify-between gap-4">
                <div className="flex min-w-0 flex-col gap-1">
                    <span className="text-sm font-semibold text-foreground">
                        Total
                    </span>

                    <span className="text-xs text-muted-foreground">
                        Includes applicable taxes and delivery
                    </span>
                </div>

                <span className="shrink-0 text-2xl font-semibold tracking-tight text-foreground">
                    {formatPrice(grandTotal)}
                </span>
            </div>

            <div className="mt-5 rounded-2xl border border-border/70 bg-muted/25 p-3.5">
                <div className="flex items-center gap-2.5">
                    <ShieldCheck className="size-4 text-primary" />
                    <span className="text-xs font-medium text-foreground">
                        Payment status
                    </span>
                </div>
                <p className="mt-2 text-sm font-semibold text-foreground">
                    {hasPendingOrder
                        ? `Payment pending for ${pendingOrderNumber}`
                        : "Payment will be collected after order placement"}
                </p>
            </div>

            <Button
                type="button"
                disabled={!canProceed}
                onClick={onPlaceOrder}
                className="group mt-5 h-12 w-full rounded-xl text-sm font-semibold cursor-pointer"
            >
                {isPlacingOrder ? (
                    <>
                        <span className="mr-2 size-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                        {hasPendingOrder ? "Opening secure checkout..." : "Placing order..."}
                    </>
                ) : (
                    <>
                        {hasPendingOrder ? "Retry payment" : "Place order"}
                        <PackageCheck className="size-4 transition-transform duration-200 group-hover:scale-105" />
                    </>
                )}
            </Button>

            <p className="mt-3 text-center text-[11px] leading-5 text-muted-foreground">
                {!isPaymentGatewayReady
                    ? "Loading secure payment checkout..."
                    : isPlacingOrder
                        ? hasPendingOrder
                            ? "Connecting to Razorpay securely..."
                            : "Placing your order securely..."
                        : isUpdating
                            ? "Updating your checkout..."
                            : hasPendingOrder
                                ? "Your order remains Placed until payment is verified."
                                : !hasSelectedAddress
                                    ? "Select a delivery address to place your order."
                                    : hasUnavailableItems
                                        ? "Remove unavailable products before placing your order."
                                        : "Your order will be placed before payment is collected."}
            </p>

            <Button
                type="button"
                variant="ghost"
                onClick={onBack}
                className="group mt-4 h-10 w-full rounded-xl text-sm font-medium cursor-pointer"
            >
                <ArrowLeft className="size-4" />
                Continue shopping
            </Button>

            <div className="mt-5 flex flex-col gap-3 border-t border-border/70 pt-5">
                <div className="flex items-center gap-2.5">
                    <ShieldCheck className="size-4 text-primary" />

                    <span className="text-xs text-muted-foreground">
                        Secure account checkout
                    </span>
                </div>

                <div className="flex items-center gap-2.5">
                    <BadgeCheck className="size-4 text-primary" />

                    <span className="text-xs text-muted-foreground">
                        Order status changes to Confirmed only after successful payment
                    </span>
                </div>
            </div>
        </div>
    );
}

function InfoTile({
    icon: Icon,
    title,
    description,
}: {
    icon: React.ComponentType<{
        className?: string;
    }>;
    title: string;
    description: string;
}) {
    return (
        <div className="rounded-2xl border border-border/70 bg-muted/20 p-4">
            <div className="flex items-center gap-2.5">
                <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                    <Icon className="size-4" />
                </div>

                <h3 className="text-sm font-semibold text-foreground">
                    {title}
                </h3>
            </div>

            <p className="mt-3 text-xs text-muted-foreground">
                {description}
            </p>
        </div>
    );
}

function CheckoutState({
    icon: Icon,
    title,
    description,
    actionLabel,
    onAction,
}: {
    icon: React.ComponentType<{
        className?: string;
    }>;
    title: string;
    description: string;
    actionLabel: string;
    onAction: () => void;
}) {
    return (
        <main className="flex min-h-screen items-center justify-center bg-background px-4 py-20">
            <motion.section
                initial={{
                    opacity: 0,
                    y: 12,
                }}
                animate={{
                    opacity: 1,
                    y: 0,
                }}
                transition={{
                    duration: 0.35,
                    ease: "easeOut",
                }}
                className="flex w-full max-w-md flex-col items-center text-center"
            >
                <div className="flex size-16 items-center justify-center rounded-2xl border border-primary/10 bg-primary/5 text-primary">
                    <Icon className="size-7" />
                </div>

                <div className="mt-6 flex flex-col gap-2">
                    <h1 className="text-2xl font-semibold tracking-tight text-foreground">
                        {title}
                    </h1>

                    <p className="text-sm text-muted-foreground">
                        {description}
                    </p>
                </div>

                <Button
                    type="button"
                    onClick={onAction}
                    className="mt-7 h-11 rounded-xl px-6"
                >
                    {actionLabel}
                    <ArrowRight className="ml-2 size-4" />
                </Button>
            </motion.section>
        </main>
    );
}



function getCreateOrderStatusMessage(status: CreateOrderStatus): string {
    switch (status) {
        case "CartEmpty":
            return "Your cart is empty. Add items before placing an order.";
        case "ShippingAddressNotFound":
            return "The selected delivery address could not be found. Select another address or add it again.";
        case "RecipientPhoneRequired":
            return "A recipient phone number is required. Add one to your address or account before placing the order.";
        case "InvalidShippingAddress":
            return "The selected delivery address is invalid. Review it or choose another address.";
        case "ItemsUnavailable":
            return "One or more items are no longer available. Review your cart and try again.";
        case "InsufficientStock":
            return "There is not enough stock for the requested quantities. Update your cart and try again.";
        case "IdempotencyConflict":
            return "This request conflicts with an earlier order attempt. Check your order history before retrying.";
        case "Created":
        case "Replayed":
            return "Your order has been placed.";
    }
}

function getAddressLabelStyle(label: string): {
    icon: LucideIcon;
    container: string;
} {
    const normalizedLabel = label.trim().toLowerCase();

    if (normalizedLabel === "home") {
        return {
            icon: House,
            container:
                "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400",
        };
    }

    if (normalizedLabel === "work") {
        return {
            icon: Briefcase,
            container:
                "bg-blue-500/10 text-blue-600 dark:text-blue-400",
        };
    }

    if (
        normalizedLabel === "hosted" ||
        normalizedLabel === "hostel"
    ) {
        return {
            icon: Building2,
            container:
                "bg-violet-500/10 text-violet-600 dark:text-violet-400",
        };
    }

    return {
        icon: MapPin,
        container:
            "bg-muted text-muted-foreground",
    };
}
