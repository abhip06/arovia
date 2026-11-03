import type { Metadata } from "next";
import { ComingSoonPage } from "@/components/common/coming-soon-page";

export const metadata: Metadata = { title: "Lab tests — coming soon" };

export default function LabTestsPage() {
    return <ComingSoonPage service="labs" />;
}
