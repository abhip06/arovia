import { CheckoutPage } from "@/features/checkout/components/checkout-page";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Checkout" };

export default function Page() {
    return <CheckoutPage />;
}