import type { Metadata } from "next";

import { CategorySection } from "@/components/landing/category-section";
import { BenefitsSection } from "@/components/landing/benefits-section";
import { FeaturedSection } from "@/components/landing/featured-section";
import { TrustSection } from "@/components/landing/trust-section";
import { FinalCta } from "@/components/landing/final-cta";
import { HeroSection } from "@/components/landing/hero-section";
import { WebsiteLoader } from "@/components/common/website-loader";

export const metadata: Metadata = {
    title: "Arovia — Online Healthcare & Wellness Store",
    description:
        "Shop medicines, wellness essentials, personal care products, healthcare devices and more with Arovia. Discover trusted healthcare products and a simpler way to manage your everyday health.",
    openGraph: {
        title: "Arovia — Better Health. Made Simple.",
        description:
            "A simpler way to care for your health. Discover medicines, wellness essentials and trusted healthcare products with Arovia.",
        type: "website",
    },
};

export default function Home() {
    return (
        <>
            <WebsiteLoader />
            <main>
                <HeroSection />
                <CategorySection />
                <FeaturedSection />
                <BenefitsSection />
                <TrustSection />
                <FinalCta />
            </main>
        </>
    );
}
