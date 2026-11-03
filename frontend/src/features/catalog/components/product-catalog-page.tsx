"use client";

import { useEffect, useMemo } from "react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { ArrowDownNarrowWide, ArrowUpNarrowWide, PackageSearch, RotateCcw, Search, SlidersHorizontal, MapPin } from "lucide-react";

import { useLocationStore } from "@/store/location-store";

import { Button } from "@/components/ui/button";
import { CatalogPagination } from "./catalog-pagination";
import { CatalogSkeleton } from "./catalog-skeleton";
import { ProductCard } from "./product-card";
import { ProductFilterDialog } from "./product-filter-dialog";
import { useCatalogProducts } from "../hooks/use-catalog";
import type {
    CatalogFilters,
    CatalogProductParams,
    CatalogSort,
    ProductTypeApi,
} from "../types/catalog.types";
import { getProductType } from "../types/catalog.types";

const sortValues: CatalogSort[] = ["relevance", "price-low", "price-high"];

function readSort(value: string | null): CatalogSort {
    return sortValues.includes(value as CatalogSort)
        ? (value as CatalogSort)
        : "relevance";
}

function readPage(value: string | null) {
    const page = Number(value);
    return Number.isSafeInteger(page) && page > 0 ? page : 1;
}

function humanize(value: string) {
    return value
        .split("-")
        .filter(Boolean)
        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
        .join(" ");
}

function setParam(params: URLSearchParams, key: string, value?: string) {
    if (value) {
        params.set(key, value);
    } else {
        params.delete(key);
    }
}

export function ProductCatalogPage({
    routeType,
}: {
    routeType: string;
}) {
    const router = useRouter();
    const pathname = usePathname();
    const searchParams = useSearchParams();
    const typeConfig = getProductType(routeType);
    const page = readPage(searchParams.get("page"));
    const searchTerm = searchParams.get("q")?.trim() ?? "";
    const filters: CatalogFilters = {
        brand: searchParams.get("brand") || undefined,
        manufacturer: searchParams.get("manufacturer") || undefined,
        category: searchParams.get("category") || undefined,
        sort: readSort(searchParams.get("sort")),
    };

    const city = useLocationStore((state) => state.city);
    const state = useLocationStore((state) => state.state);
    const postalCode = useLocationStore((state) => state.pincode);

    const deliveryLocation =
        city && state
            ? `${city}, ${state}${postalCode ? ` - ${postalCode}` : ""}`
            : postalCode;

    const params: CatalogProductParams = {
        page,
        ...(typeConfig ? { type: typeConfig.apiType } : {}),
        ...(filters.brand ? { brand: filters.brand } : {}),
        ...(filters.manufacturer ? { manufacturer: filters.manufacturer } : {}),
        ...(filters.category ? { category: filters.category } : {}),
        ...(searchTerm ? { search: searchTerm } : {}),
    };
    const catalog = useCatalogProducts(params);

    const sortedItems = useMemo(() => {
        const items = [...(catalog.data?.items ?? [])];

        if (filters.sort === "price-low") {
            return items.sort((left, right) => left.sellingPrice - right.sellingPrice);
        }

        if (filters.sort === "price-high") {
            return items.sort((left, right) => right.sellingPrice - left.sellingPrice);
        }

        return items;
    }, [catalog.data?.items, filters.sort]);

    const navigateWithParams = (
        updates: Record<string, string | null | undefined>,
        replace = false,
    ) => {
        const nextParams = new URLSearchParams(searchParams.toString());

        for (const [key, value] of Object.entries(updates)) {
            setParam(nextParams, key, value ?? undefined);
        }

        const query = nextParams.toString();
        const href = query ? `${pathname}?${query}` : pathname;
        if (replace) {
            router.replace(href, { scroll: false });
        } else {
            router.push(href);
        }
    };

    useEffect(() => {
        const result = catalog.data;
        if (!result) {
            return;
        }

        if (result.totalPages === 0 && page > 1) {
            navigateWithParams({ page: "1" }, true);
        } else if (result.totalPages > 0 && page > result.totalPages) {
            navigateWithParams({ page: String(result.totalPages) }, true);
        }
    }, [catalog.data, page]);

    const applyFilters = (nextFilters: CatalogFilters) => {
        navigateWithParams({
            brand: nextFilters.brand,
            manufacturer: nextFilters.manufacturer,
            category: nextFilters.category,
            sort: nextFilters.sort === "relevance" ? null : nextFilters.sort,
            page: "1",
        });
    };

    const clearFilters = () => {
        navigateWithParams({
            brand: null,
            manufacturer: null,
            category: null,
            sort: null,
            page: "1",
        });
    };

    const resetCatalog = () => {
        router.push(pathname);
    };

    const hasActiveFilters = Boolean(
        filters.brand ||
        filters.manufacturer ||
        filters.category ||
        filters.sort !== "relevance",
    );
    const heading = typeConfig?.label ?? (searchTerm ? "Search results" : "Browse products");
    const description = typeConfig?.description ?? (
        searchTerm
            ? `Products matching “${searchTerm}”.`
            : "Search and explore our healthcare essentials."
    );

    if (catalog.isPending) {
        return <CatalogSkeleton />;
    }

    if (catalog.isError) {
        return (
            <section className="mx-auto flex min-h-[60vh] w-full max-w-7xl flex-col items-center justify-center px-4 py-20 text-center sm:px-6 lg:px-8">
                <div className="flex size-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
                    <PackageSearch className="size-7" />
                </div>
                <h1 className="mt-6 text-2xl font-semibold tracking-tight">We couldn’t load these products</h1>
                <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
                    {catalog.error.message || "Please check your connection and try again."}
                </p>
                <Button type="button" onClick={() => void catalog.refetch()} className="mt-6 gap-2 rounded-xl">
                    <RotateCcw className="size-4" />
                    Try again
                </Button>
            </section>
        );
    }

    const result = catalog.data;
    const hasProducts = result.items.length > 0;

    return (
        <section className="min-h-[70vh] bg-background">
            <div className="mx-auto w-full max-w-7xl px-4 pb-16 pt-8 sm:px-6 sm:pt-10 lg:px-8 lg:pb-20">
                <nav aria-label="Breadcrumb" className="mb-5 flex items-center gap-2 text-xs text-muted-foreground">
                    <Link href="/" className="transition-colors hover:text-foreground">Home</Link>
                    <span aria-hidden="true">/</span>
                    <span aria-current="page" className="truncate text-foreground/75">
                        {typeConfig?.label ?? "Search"}
                    </span>
                </nav>

                <div className="relative overflow-hidden rounded-[1.75rem] border border-primary/10 bg-gradient-to-r from-primary/8 via-card to-sky-500/5 px-5 py-7 sm:px-8 sm:py-9">
                    <div aria-hidden="true" className="pointer-events-none absolute -right-12 -top-24 size-64 rounded-full bg-primary/10 blur-3xl" />
                    <div className="relative flex flex-col justify-between gap-6 sm:flex-row sm:items-end">
                        <div className="max-w-2xl">
                            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-primary">
                                Arovia · Shop by care
                            </p>
                            <h1 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl lg:text-[2.1rem]">
                                {searchTerm && routeType === "search" ? (
                                    <>Results for <span className="text-primary">“{searchTerm}”</span></>
                                ) : heading}
                            </h1>
                            <p className="mt-2 max-w-xl text-sm leading-6 text-muted-foreground sm:text-base">
                                {description}
                            </p>
                            {deliveryLocation && (
                                <p className="mt-3 flex max-w-xl items-start gap-2 text-sm leading-6 text-primary">
                                    <MapPin className="mt-1 size-4 shrink-0" />

                                    <span>
                                        Delivering to {deliveryLocation}.
                                    </span>
                                </p>
                            )}
                        </div>

                        {/* Product count */}
                        {hasProducts && (
                            <p className="shrink-0 rounded-full border border-border/70 bg-background/75 px-4 py-2 text-sm text-muted-foreground backdrop-blur">
                                <span className="font-semibold text-foreground">
                                    {result.totalCount.toLocaleString("en-IN")}
                                </span>{" "}
                                {result.totalCount === 1
                                    ? "product"
                                    : "products"}
                            </p>
                        )}
                    </div>
                </div>

                <div className="mt-7 flex flex-col gap-3 border-b border-border/70 pb-5 sm:flex-row sm:items-center sm:justify-between">
                    <div className="flex flex-wrap items-center gap-2">
                        <ProductFilterDialog
                            filters={filters}
                            productType={typeConfig?.apiType as ProductTypeApi | undefined}
                            onApply={applyFilters}
                            onClear={clearFilters}
                        />
                        <span className="hidden h-5 w-px bg-border sm:block" />
                        <span className="px-1 text-xs font-medium text-muted-foreground">Sort:</span>
                        <Button
                            type="button"
                            variant={filters.sort === "relevance" ? "secondary" : "ghost"}
                            onClick={() => navigateWithParams({ sort: null, page: "1" })}
                            className="h-9 rounded-xl px-3 text-xs"
                        >
                            Recommended
                        </Button>
                        <Button
                            type="button"
                            variant={filters.sort === "price-low" ? "secondary" : "ghost"}
                            onClick={() => navigateWithParams({ sort: "price-low", page: "1" })}
                            className="h-9 gap-1.5 rounded-xl px-3 text-xs"
                        >
                            <ArrowUpNarrowWide className="size-3.5" /> Price low to high
                        </Button>
                        <Button
                            type="button"
                            variant={filters.sort === "price-high" ? "secondary" : "ghost"}
                            onClick={() => navigateWithParams({ sort: "price-high", page: "1" })}
                            className="h-9 gap-1.5 rounded-xl px-3 text-xs"
                        >
                            <ArrowDownNarrowWide className="size-3.5" /> Price high to low
                        </Button>
                    </div>

                    {result.totalPages > 1 && (
                        <p className="shrink-0 text-xs text-muted-foreground">
                            Page {result.page} of {result.totalPages}
                        </p>
                    )}
                </div>

                {(hasActiveFilters || searchTerm) && (
                    <div className="flex flex-wrap items-center gap-2 py-4">
                        {searchTerm && (
                            <span className="inline-flex items-center gap-1.5 rounded-full bg-muted px-3 py-1.5 text-xs text-muted-foreground">
                                <Search className="size-3.5" /> “{searchTerm}”
                            </span>
                        )}
                        {filters.brand && <FilterChip label={humanize(filters.brand)} onClear={() => navigateWithParams({ brand: null, page: "1" })} />}
                        {filters.manufacturer && <FilterChip label={humanize(filters.manufacturer)} onClear={() => navigateWithParams({ manufacturer: null, page: "1" })} />}
                        {filters.category && <FilterChip label={humanize(filters.category)} onClear={() => navigateWithParams({ category: null, page: "1" })} />}
                        {filters.sort !== "relevance" && (
                            <FilterChip
                                label={filters.sort === "price-low" ? "Price: low to high" : "Price: high to low"}
                                onClear={() => navigateWithParams({ sort: null, page: "1" })}
                            />
                        )}
                        {hasActiveFilters && (
                            <button
                                type="button"
                                onClick={clearFilters}
                                className="ml-1 text-xs font-semibold text-primary underline-offset-4 hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                            >
                                Clear filters
                            </button>
                        )}
                    </div>
                )}

                {hasProducts ? (
                    <>
                        <div className="mt-5 grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-4 lg:grid-cols-4 xl:grid-cols-5">
                            {sortedItems.map((product, index) => (
                                <ProductCard
                                    key={`${product.productSlug}-${product.sku}`}
                                    product={product}
                                    index={index}
                                />
                            ))}
                        </div>

                        <div className="mt-9">
                            <CatalogPagination
                                page={result.page}
                                pageSize={result.pageSize}
                                totalCount={result.totalCount}
                                totalPages={result.totalPages}
                                onPageChange={(nextPage) => navigateWithParams({ page: String(nextPage) })}
                            />
                        </div>
                    </>
                ) : (
                    <div className="flex min-h-72 flex-col items-center justify-center rounded-3xl border border-dashed border-border/80 bg-card/60 px-5 py-12 text-center">
                        <div className="flex size-14 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                            <SlidersHorizontal className="size-6" />
                        </div>
                        <h2 className="mt-5 text-lg font-semibold">No products found</h2>
                        <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
                            Try a different search or remove a filter to see more products.
                        </p>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={resetCatalog}
                            className="mt-5 gap-2 rounded-xl"
                        >
                            <RotateCcw className="size-4" />
                            Reset search and filters
                        </Button>
                    </div>
                )}
            </div>
        </section>
    );
}

function FilterChip({ label, onClear }: { label: string; onClear: () => void }) {
    return (
        <button
            type="button"
            onClick={onClear}
            aria-label={`Remove ${label} filter`}
            className="inline-flex items-center gap-2 rounded-full border border-primary/15 bg-primary/5 px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-primary/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
            {label}
            <span aria-hidden="true" className="text-muted-foreground">×</span>
        </button>
    );
}
