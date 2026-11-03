"use client";

import { useRouter } from "next/navigation";
import { LoaderCircle, ShoppingCart } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/features/auth/context/auth-context";
import { cn } from "@/lib/utils";
import { useAddProductToCart } from "../hooks/use-cart";

export function AddToCartButton({
    itemId,
    productName,
    className,
}: {
    itemId: string;
    productName: string;
    className?: string;
}) {
    const router = useRouter();
    const { authStatus, isLoading: isAuthLoading } = useAuth();
    const addToCart = useAddProductToCart();

    const handleAdd = async () => {
        if (isAuthLoading) return;

        if (!authStatus) {
            const returnUrl = `${window.location.pathname}${window.location.search}`;
            router.push(`/sign-in?returnUrl=${encodeURIComponent(returnUrl)}`);
            return;
        }

        try {
            await addToCart.mutateAsync({ itemId, quantity: 1 });
            toast.success(`${productName} added to your cart.`);
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Unable to add this product. Please try again.",
            );
        }
    };

    return (
        <Button
            type="button"
            onClick={handleAdd}
            disabled={isAuthLoading || addToCart.isPending}
            aria-label={
                addToCart.isPending
                    ? `Adding ${productName} to cart`
                    : `Add ${productName} to cart`
            }
            className={cn("h-10 w-full cursor-pointer gap-2 rounded-xl", className)}
        >
            {addToCart.isPending ? (
                <LoaderCircle className="size-4 animate-spin" />
            ) : (
                <ShoppingCart className="size-4" />
            )}
            {addToCart.isPending
                ? "Adding…"
                : authStatus
                  ? "Add to cart"
                  : "Sign in to add"}
        </Button>
    );
}
