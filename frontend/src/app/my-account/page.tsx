import { AccountPage } from "@/components/account/account-page";
import type { Metadata } from "next";


export const metadata: Metadata = {
    title: "My Account",
    description:
        "Control center for managing and controlling user account data, addresses, orders and settings.",
};

export default function ContactPageRoute() {
    return <AccountPage />;
}