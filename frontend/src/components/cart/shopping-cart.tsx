"use client";

import Image from "next/image";
import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { AnimatePresence, motion } from "framer-motion";
import {
    ArrowRight,
    LoaderCircle,
    Minus,
    PackageCheck,
    Plus,
    ShoppingBag,
    Trash2,
    RotateCcw,
} from "lucide-react";
import { FiShoppingCart } from "react-icons/fi";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
    Sheet,
    SheetContent,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet";

import { useAuth } from "@/features/auth/context/auth-context";
import {
    useCart,
    useRemoveCartItem,
    useSetCartItemQuantity,
} from "@/features/cart/hooks/use-cart";

import { formatPrice } from "@/lib/formatters";
import type { CartLine } from "@/types/cart.types";

interface ShoppingCartProps {
    appearance?: "default" | "overlay";
}

const EMPTY_CART_ITEMS: CartLine[] = [];

export function ShoppingCartDrawer({
    appearance = "default",
}: ShoppingCartProps) {
    const [open, setOpen] = useState(false);
    const router = useRouter();

    const {
        authStatus,
        isLoading: isAuthLoading,
    } = useAuth();

    const cartQuery = useCart(
        !isAuthLoading && authStatus,
    );

    const setQuantity = useSetCartItemQuantity();
    const removeItem = useRemoveCartItem();

    const cart = cartQuery.data;
    const items = cart?.items ?? EMPTY_CART_ITEMS;
    const isOverlay = appearance === "overlay";

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

    // Combine the backend's availability result with the
    // frontend check, just as the Checkout page does.
    const hasUnavailableItems =
        Boolean(cart?.hasUnavailableItems) ||
        unavailableItems.length > 0;

    const continueShopping = () => {
        setOpen(false);
        router.push("/otc/medicine");
    };

    const signIn = () => {
        setOpen(false);

        const returnUrl =
            `${window.location.pathname}${window.location.search}`;

        router.push(
            `/sign-in?returnUrl=${encodeURIComponent(returnUrl)}`,
        );
    };

    const moveToCheckout = () => {
        if (hasUnavailableItems) {
            toast.error(
                "Remove unavailable products before continuing to checkout.",
            );
            return;
        }

        setOpen(false);
        router.push("/checkout");
    };

    const handleQuantity = async (
        item: CartLine,
        quantity: number,
    ) => {
        if (
            quantity < 1 ||
            quantity > 25 ||
            !item.isAvailable ||
            item.unitPrice == null ||
            item.lineTotal == null
        ) {
            return;
        }

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
        }
    };

    const handleRemove = async (item: CartLine) => {
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
        }
    };

    const itemCount = cart?.itemCount ?? 0;

    return (
        <Sheet open={open} onOpenChange={setOpen}>
            <SheetTrigger
                render={
                    <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        aria-label={`Shopping cart${
                            itemCount > 0
                                ? `, ${itemCount} ${
                                      itemCount === 1
                                          ? "item"
                                          : "items"
                                  }`
                                : ""
                        }`}
                        className={
                            isOverlay
                                ? "relative size-10 rounded-full text-white hover:bg-white/10 hover:text-white cursor-pointer"
                                : "relative size-10 rounded-full text-foreground hover:bg-muted cursor-pointer"
                        }
                    >
                        <FiShoppingCart className="size-5" />

                        {itemCount > 0 && (
                            <span
                                aria-hidden="true"
                                className="absolute -right-0.5 -top-0.5 flex size-5 items-center justify-center rounded-full border-2 border-background bg-primary text-[10px] font-bold leading-none text-primary-foreground"
                            >
                                {itemCount > 99 ? "99+" : itemCount}
                            </span>
                        )}
                    </Button>
                }
            />

            <SheetContent
                side="right"
                className="flex h-dvh w-screen max-w-none flex-col gap-0 overflow-hidden border-l border-border/70 bg-background p-0 sm:max-w-md"
            >
                <SheetHeader className="shrink-0 border-b border-border/70 px-5 py-4 sm:px-6">
                    <SheetTitle className="text-lg font-semibold">
                        Your cart
                    </SheetTitle>

                    <p className="text-xs text-muted-foreground">
                        {itemCount === 0
                            ? "Your next essentials start here"
                            : `${itemCount} ${
                                  itemCount === 1 ? "item" : "items"
                              } in your cart`}
                    </p>
                </SheetHeader>

                {isAuthLoading ? (
                    <CartLoading />
                ) : !authStatus ? (
                    <CartEmpty
                        title="Sign in to see your cart"
                        description="Your cart is connected to your Arovia account. Sign in to add products and keep them together."
                        actionLabel="Sign in"
                        onAction={signIn}
                    />
                ) : cartQuery.isPending ? (
                    <CartLoading />
                ) : cartQuery.isError ? (
                    <CartEmpty
                        title="Your cart could not load"
                        description={
                            cartQuery.error.message ||
                            "Please try again in a moment."
                        }
                        actionLabel="Try again"
                        onAction={() => void cartQuery.refetch()}
                    />
                ) : items.length === 0 ? (
                    <CartEmpty
                        title="Your cart is empty"
                        description="Explore trusted health essentials and add what you need to your cart."
                        actionLabel="Start shopping"
                        onAction={continueShopping}
                    />
                ) : (
                    <div className="flex min-h-0 flex-1 flex-col">
                        <ScrollArea className="min-h-0 flex-1">
                            <div className="flex flex-col gap-3 px-4 py-5 sm:px-5">
                                {hasUnavailableItems && (
                                    <div
                                        role="alert"
                                        className="rounded-xl border border-amber-500/20 bg-amber-500/5 px-3.5 py-3 text-xs leading-5 text-amber-800 dark:text-amber-200"
                                    >
                                        {unavailableItems.length > 0
                                            ? `${unavailableItems.length} ${
                                                  unavailableItems.length === 1
                                                      ? "product is"
                                                      : "products are"
                                              } unavailable or missing valid pricing.`
                                            : "Some products are currently unavailable."}{" "}
                                        Remove unavailable products before continuing to checkout.
                                    </div>
                                )}

                                <AnimatePresence mode="popLayout">
                                    {items.map((item) => (
                                        <CartItemCard
                                            key={item.id}
                                            item={item}
                                            isUpdating={setQuantity.isPending}
                                            isRemoving={removeItem.isPending}
                                            onQuantity={handleQuantity}
                                            onRemove={handleRemove}
                                        />
                                    ))}
                                </AnimatePresence>
                            </div>
                        </ScrollArea>

                        <CartSummary
                            subtotal={cart?.subtotal ?? 0}
                            hasUnavailableItems={hasUnavailableItems}
                            checkout={moveToCheckout}
                        />
                    </div>
                )}
            </SheetContent>
        </Sheet>
    );
}

function CartItemCard({
    item,
    isUpdating,
    isRemoving,
    onQuantity,
    onRemove,
}: {
    item: CartLine;
    isUpdating: boolean;
    isRemoving: boolean;
    onQuantity: (item: CartLine, quantity: number) => void;
    onRemove: (item: CartLine) => void;
}) {
    const unavailable =
        !item.isAvailable ||
        item.unitPrice == null ||
        item.lineTotal == null;

    return (
        <motion.article
            layout
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, x: -16 }}
            transition={{ duration: 0.22 }}
            className="rounded-2xl border border-border/70 bg-card p-3.5"
        >
            <div className="flex gap-3">
                <div className="relative size-[72px] shrink-0 overflow-hidden rounded-xl border border-border/60 bg-primary/5 sm:size-20">
                    {item.imageUrl ? (
                        <Image
                            src={item.imageUrl}
                            alt={item.productName}
                            fill
                            sizes="80px"
                            className="object-contain p-2"
                        />
                    ) : (
                        <div className="flex size-full items-center justify-center text-primary">
                            <ShoppingBag className="size-7 opacity-60" />
                        </div>
                    )}
                </div>

                <div className="min-w-0 flex-1">
                    <div className="flex items-start justify-between gap-2">
                        <div className="min-w-0">
                            <h3 className="line-clamp-2 text-sm font-semibold">
                                {item.productName}
                            </h3>

                            <p className="mt-1 text-xs text-muted-foreground">
                                {item.packageQuantity} {item.packageUnit}
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

                        <div className="shrink-0 text-right">
                            {unavailable ? (
                                <Badge
                                    variant="outline"
                                    className="border-amber-500/30 text-amber-700 dark:text-amber-300"
                                >
                                    Unavailable
                                </Badge>
                            ) : (
                                <span className="text-sm font-semibold">
                                    {formatPrice(item.lineTotal!)}
                                </span>
                            )}

                            {item.mrp != null &&
                                item.unitPrice != null &&
                                item.mrp > item.unitPrice && (
                                    <p className="mt-1 text-[11px] text-muted-foreground">
                                        MRP{" "}
                                        <span className="line-through">
                                            {formatPrice(
                                                item.mrp * item.quantity,
                                            )}
                                        </span>
                                    </p>
                                )}
                        </div>
                    </div>

                    <div className="mt-3 flex items-center justify-between">
                        <div className="inline-flex items-center rounded-full border border-border/70 bg-muted/40 p-1">
                            <QuantityButton
                                label={`Decrease ${item.productName} quantity`}
                                disabled={
                                    isUpdating ||
                                    unavailable ||
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
                                className="min-w-8 px-1 text-center text-xs font-semibold"
                            >
                                {item.quantity}
                            </span>

                            <QuantityButton
                                label={`Increase ${item.productName} quantity`}
                                disabled={
                                    isUpdating ||
                                    unavailable ||
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

                        <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            aria-label={`Remove ${item.productName}`}
                            disabled={isRemoving}
                            onClick={() => onRemove(item)}
                            className="size-8 rounded-full text-muted-foreground hover:bg-destructive/10 hover:text-destructive cursor-pointer"
                        >
                            {isRemoving ? (
                                <LoaderCircle className="size-4 animate-spin" />
                            ) : (
                                <Trash2 className="size-4" />
                            )}
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
            className="flex size-7 items-center justify-center rounded-full bg-background text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-40 cursor-pointer"
        >
            {children}
        </button>
    );
}

function CartEmpty({
    title,
    description,
    actionLabel,
    onAction,
}: {
    title: string;
    description: string;
    actionLabel: string;
    onAction: () => void;
}) {
    return (
        <div className="flex min-h-0 flex-1 items-center justify-center px-6 py-10">
            <motion.div
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                className="flex max-w-sm flex-col items-center text-center"
            >
                <div className="flex size-16 items-center justify-center rounded-2xl border border-primary/10 bg-primary/5 text-primary">
                    <ShoppingBag className="size-7" />
                </div>

                <h2 className="mt-5 text-xl font-semibold">
                    {title}
                </h2>

                <p className="mt-2 max-w-xs text-sm leading-6 text-muted-foreground">
                    {description}
                </p>

                <Button
                    type="button"
                    onClick={onAction}
                    className="mt-6 h-11 gap-2 rounded-xl px-5 cursor-pointer"
                >
                    {actionLabel === "Try again" && (
                        <RotateCcw className="size-4" />
                    )}

                    {actionLabel}
                    <ArrowRight className="size-4" />
                </Button>
            </motion.div>
        </div>
    );
}

function CartLoading() {
    return (
        <div
            role="status"
            aria-label="Loading your cart"
            className="flex flex-1 items-center justify-center"
        >
            <LoaderCircle className="size-7 animate-spin text-primary" />
        </div>
    );
}

function CartSummary({
    subtotal,
    hasUnavailableItems,
    checkout,
}: {
    subtotal: number;
    hasUnavailableItems: boolean;
    checkout: () => void;
}) {
    return (
        <div className="shrink-0 border-t border-border/70 bg-background px-5 pb-[max(1rem,env(safe-area-inset-bottom))] pt-4 sm:px-6">
            <div className="flex items-center justify-between gap-4">
                <span className="text-sm text-muted-foreground">
                    Subtotal
                </span>

                <span className="text-xl font-semibold">
                    {formatPrice(subtotal)}
                </span>
            </div>

            <p className="mt-1 text-right text-xs text-muted-foreground">
                Taxes and delivery charges are calculated at checkout.
            </p>

            <Button
                type="button"
                disabled={hasUnavailableItems}
                onClick={checkout}
                className="mt-4 h-12 w-full gap-2 rounded-xl cursor-pointer"
            >
                Checkout
                <PackageCheck className="size-4" />
            </Button>

            <p className="mt-2 text-center text-[11px] text-muted-foreground">
                {hasUnavailableItems
                    ? "Remove unavailable products to prepare your cart."
                    : "Your cart is saved to your account."}
            </p>
        </div>
    );
}