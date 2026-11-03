import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Suspense } from "react";

import { CatalogSkeleton } from "@/features/catalog/components/catalog-skeleton";
import { ProductCatalogPage } from "@/features/catalog/components/product-catalog-page";
import { getProductType } from "@/features/catalog/types/catalog.types";

interface CatalogRouteProps {
    params: Promise<{ productType: string }>;
}

export async function generateMetadata({ params }: CatalogRouteProps): Promise<Metadata> {
    const { productType } = await params;

    if (productType === "search") {
        return { title: "Search products" };
    }

    const typeConfig = getProductType(productType);
    return typeConfig ? { title: typeConfig.label } : { title: "Products" };
}

export default async function CatalogRoute({ params }: CatalogRouteProps) {
    const { productType } = await params;

    if (productType !== "search" && !getProductType(productType)) {
        notFound();
    }

    return (
        <Suspense fallback={<CatalogSkeleton />}>
            <ProductCatalogPage routeType={productType} />
        </Suspense>
    );
}
