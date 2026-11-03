import { OrdersPage } from "@/features/orders/components/orders-page";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "My Orders",
    description:
        "Control center for managing and controlling user orders.",
};

export default function OrdersRoute() {
    return <OrdersPage />;
}
