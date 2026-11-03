import type { Metadata } from "next";
import { AboutPage } from "./_components/about-page";

export const metadata: Metadata = {
    title: "About Us",
    description:
        "Learn about Arovia and our approach to making everyday healthcare simpler, clearer, and easier to navigate.",
};

export default function AboutPageRoute() {
    return <AboutPage />;
}