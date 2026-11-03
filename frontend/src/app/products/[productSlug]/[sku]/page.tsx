import type { Metadata } from "next";

import { ProductDetailPage } from "@/features/catalog/components/product-detail-page";

interface ProductRouteProps {
    params: Promise<{ productSlug: string; sku: string }>;
}

export async function generateMetadata({ params }: ProductRouteProps): Promise<Metadata> {
    const { productSlug } = await params;
    const name = decodeURIComponent(productSlug)
        .split("-")
        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
        .join(" ");

    return { title: name };
}

export default async function ProductRoute({ params }: ProductRouteProps) {
    const { productSlug, sku } = await params;

    return <ProductDetailPage productSlug={productSlug} sku={sku} />;
}
