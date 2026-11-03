import type { Metadata } from "next";
import { ComingSoonPage } from "@/components/common/coming-soon-page";

export const metadata: Metadata = { title: "Medical insurance — coming soon" };

export default function InsurancePage() {
    return <ComingSoonPage service="insurance" />;
}
