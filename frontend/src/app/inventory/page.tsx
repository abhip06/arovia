import { InventoryWorkspace } from "@/features/dashboard/components/inventory-workspace";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Inventory",
    description:
        "Manage Product Variants, Prices, Stocks and Packaging.",
};

export default function InventoryOverviewPage() { return <InventoryWorkspace view="overview" />; }
