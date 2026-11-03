"use client";

import Image from "next/image";
import { useState } from "react";
import Link from "next/link";
import { PackageOpen } from "lucide-react";
import { motion } from "framer-motion";

import { Card, CardContent } from "@/components/ui/card";
import { AddToCartButton } from "@/features/cart/components/add-to-cart-button";
import type { CatalogProduct } from "../types/catalog.types";
import { formatPrice } from "@/lib/formatters";

function packageDescription(product: CatalogProduct) {
    const packageSize = `${product.packageQuantity} ${product.packageUnit}`.trim();

    if (product.netQuantity == null || !product.netQuantityUnit) {
        return packageSize;
    }

    return `${packageSize} · ${product.netQuantity} ${product.netQuantityUnit}`;
}

export function ProductCard({
    product,
    index,
    compact = false,
}: {
    product: CatalogProduct;
    index: number;
    compact?: boolean;
}) {
    const [imageFailed, setImageFailed] = useState(false);
    const hasImage = Boolean(product.imageUrl) && !imageFailed;
    const discount =
        product.mrp != null && product.mrp > product.sellingPrice
            ? Math.round(((product.mrp - product.sellingPrice) / product.mrp) * 100)
            : 0;

    return (
        <motion.article
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.28, delay: Math.min(index * 0.025, 0.2) }}
            className={compact ? "h-full w-[232px] snap-start sm:w-[248px]" : "h-full"}
        >
            <Card className="group relative h-full gap-0 rounded-2xl border-border/70 bg-card py-0 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:border-primary/25 hover:shadow-lg hover:shadow-primary/5">
                <Link
                    href={`/products/${encodeURIComponent(product.productSlug)}/${encodeURIComponent(product.sku)}`}
                    aria-label={`View ${product.productName} details`}
                    className="absolute inset-0 z-10 rounded-2xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-primary"
                >
                    <span className="sr-only">View product details</span>
                </Link>

                <div className="relative mx-3 mt-3 flex aspect-[4/3] items-center justify-center">
                    {discount > 0 && (
                        <span className="absolute left-1 top-1 z-10 rounded-full bg-primary px-2.5 py-1 text-[10px] font-semibold tracking-wide text-primary-foreground">
                            {discount}% OFF
                        </span>
                    )}

                    {hasImage ? (
                        <Image
                            src={product.imageUrl!}
                            alt={product.productName}
                            width={400}
                            height={300}
                            sizes="(max-width: 640px) 45vw, (max-width: 1024px) 30vw, 240px"
                            className="h-full w-full object-contain p-2 transition-transform duration-500 group-hover:scale-[1.04]"
                            onError={() => setImageFailed(true)}
                        />
                    ) : (
                        <div
                            role="img"
                            aria-label={`${product.productName} product image unavailable`}
                            className="flex size-full flex-col items-center justify-center gap-2 text-muted-foreground/50"
                        >
                            <PackageOpen className="size-11 stroke-[1.4]" />
                            <span className="text-[10px] font-medium uppercase tracking-[0.14em]">
                                Image unavailable
                            </span>
                        </div>
                    )}
                </div>

                <CardContent className="pointer-events-none relative z-20 flex flex-1 flex-col px-4 pb-4 pt-2 sm:px-5 sm:pb-5">
                    <p className="min-h-5 truncate text-xs font-medium text-primary">
                        {product.brandName || "Trusted healthcare"}
                    </p>

                    <h2 className="mt-1 line-clamp-2 min-h-11 text-sm font-semibold leading-5 tracking-tight sm:text-[15px]">
                        {product.productName}
                    </h2>

                    <p className="mt-1 truncate text-xs text-muted-foreground">
                        {packageDescription(product)}
                    </p>

                    <div className="mt-auto flex items-end gap-2 pt-4">
                        <div className="min-w-0">
                            {product.mrp != null &&
                                product.mrp > product.sellingPrice && (
                                    <p className="mt-0.5 text-xs text-muted-foreground">
                                        MRP{" "}
                                        <span className="line-through">
                                            {formatPrice(product.mrp)}
                                        </span>
                                    </p>
                                )}

                            <p className="text-base font-bold tracking-tight">
                                {formatPrice(product.sellingPrice)}
                            </p>
                        </div>
                    </div>

                    <div className="pointer-events-auto relative z-20 mt-4">
                        <AddToCartButton
                            itemId={product.productVariantId}
                            productName={product.productName}
                        />
                    </div>
                </CardContent>
            </Card>
        </motion.article>
    );
}
