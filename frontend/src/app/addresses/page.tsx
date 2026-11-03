import { AddressManagementPage } from "@/features/addresses/components/address-management-page";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Saved Address",
    description: "Manage all Saved Addresses - Create, Update, set Primary and Delete Addresses."
};

export default function Page() {
    return <AddressManagementPage />
}