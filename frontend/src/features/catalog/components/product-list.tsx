"use client";

import Link from "next/link";
import { ArrowRight, PackageOpen, RotateCcw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { ProductCard } from "./product-card";
import { useCatalogProducts } from "../hooks/use-catalog";

interface ProductListProps {
    title: string;
    categorySlug: string;
    productTypeSlug: string;
}

export function ProductList({ title, categorySlug, productTypeSlug }: ProductListProps) {
    const query = useCatalogProducts({ page: 1, category: categorySlug });
    const products = query.data?.items.slice(0, 12) ?? [];
    const moreHref = `/otc/${productTypeSlug}?category=${encodeURIComponent(categorySlug)}`;

    return (
        <section aria-labelledby={`product-list-${categorySlug}`} className="border-t border-border/60 py-9 first:border-t-0 sm:py-11">
            <div className="mb-5 flex items-end justify-between gap-4">
                <div>
                    {/* <p className="text-xs font-semibold uppercase tracking-[0.14em] text-primary">Shop by category</p> */}
                    <h3 id={`product-list-${categorySlug}`} className="mt-1.5 text-xl font-semibold tracking-tight sm:text-2xl">{title}</h3>
                </div>
                <Link
                    href={moreHref}
                    className="group inline-flex h-10 shrink-0 items-center gap-2 rounded-xl border border-border/70 bg-card px-3.5 text-sm font-semibold transition-colors hover:border-primary/30 hover:bg-primary/5 hover:text-primary sm:px-4"
                >
                    See more <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
                </Link>
            </div>

            {query.isPending ? (
                <div className="flex gap-4 overflow-hidden pb-2" aria-label={`Loading ${title} products`}>
                    {Array.from({ length: 4 }, (_, index) => (
                        <div key={index} className="w-[232px] shrink-0 animate-pulse rounded-2xl border border-border/70 p-3">
                            <div className="aspect-[4/3] rounded-xl bg-muted" />
                            <div className="mt-4 h-3 w-20 rounded bg-muted" />
                            <div className="mt-2 h-4 w-full rounded bg-muted" />
                            <div className="mt-2 h-5 w-24 rounded bg-muted" />
                            <div className="mt-4 h-10 rounded-xl bg-muted" />
                        </div>
                    ))}
                </div>
            ) : query.isError ? (
                <Card className="flex min-h-40 flex-col items-center justify-center rounded-2xl border-border/70 bg-card/70 p-6 text-center">
                    <p className="text-sm font-medium">We couldn’t load this collection.</p>
                    <Button type="button" variant="ghost" onClick={() => void query.refetch()} className="mt-2 gap-2 text-primary">
                        <RotateCcw className="size-4" /> Try again
                    </Button>
                </Card>
            ) : products.length === 0 ? (
                <div className="flex min-h-40 flex-col items-center justify-center rounded-2xl border border-dashed border-border/80 bg-card/50 px-5 py-8 text-center">
                    <PackageOpen className="size-8 text-primary/50" />
                    <p className="mt-3 text-sm font-medium">More {title.toLowerCase()} picks are on the way.</p>
                    <p className="mt-1 text-xs text-muted-foreground">Check back soon or explore the full shop.</p>
                </div>
            ) : (
                <div className="grid auto-cols-[232px] grid-flow-col gap-3 overflow-x-auto overscroll-x-contain pb-4 snap-x snap-mandatory [scrollbar-width:none] [&::-webkit-scrollbar]:hidden sm:auto-cols-[248px] sm:gap-4">
                    {products.map((product, index) => (
                        <ProductCard key={`${product.productSlug}-${product.sku}`} product={product} index={index} compact />
                    ))}
                </div>
            )}
        </section>
    );
}
