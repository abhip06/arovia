import { OrderDetailsPage } from "@/features/orders/components/order-details-page";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Order Info",
    description:
        "Order Info page.",
};

export default function OrderDetailsRoute() {
    return <OrderDetailsPage />;
}
