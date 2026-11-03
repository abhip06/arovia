import { AdminOverview } from "@/features/dashboard/components/admin-overview";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Admin",
    description:
        "Manage Warehouses, Product Catalogs, Orders, Users & Staff, Shipment and Payments.",
};

export default function AdminPage() { return <AdminOverview />; }
