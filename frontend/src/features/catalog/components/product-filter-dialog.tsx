"use client";

import { useState } from "react";
import { ArrowDownWideNarrow, ArrowUpWideNarrow, Check, RotateCcw, SlidersHorizontal } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import {
    useCatalogBrands,
    useCatalogCategories,
    useCatalogManufacturers,
} from "../hooks/use-catalog";
import type {
    CatalogCategory,
    CatalogFilters,
    CatalogOption,
    CatalogSort,
    ProductTypeApi,
} from "../types/catalog.types";

interface ProductFilterDialogProps {
    filters: CatalogFilters;
    productType?: ProductTypeApi;
    onApply: (filters: CatalogFilters) => void;
    onClear: () => void;
}

function flattenCategories(
    categories: CatalogCategory[],
    productType?: ProductTypeApi,
    depth = 0,
): Array<CatalogOption & { depth: number }> {
    return categories.flatMap((category) => {
        if (productType && category.productType !== productType) {
            return flattenCategories(category.children ?? [], productType, depth);
        }

        return [
            { slug: category.slug, name: category.name, depth },
            ...flattenCategories(category.children ?? [], productType, depth + 1),
        ];
    });
}

function FilterSection({
    title,
    children,
}: {
    title: string;
    children: React.ReactNode;
}) {
    return (
        <section className="border-b border-border/70 py-5 last:border-b-0">
            <h3 className="mb-3 text-sm font-semibold">{title}</h3>
            {children}
        </section>
    );
}

function RadioRow({
    name,
    value,
    checked,
    onChange,
    children,
    indent = 0,
}: {
    name: string;
    value: string;
    checked: boolean;
    onChange: (value: string) => void;
    children: React.ReactNode;
    indent?: number;
}) {
    return (
        <label
            className="flex min-h-10 cursor-pointer items-center gap-3 rounded-lg px-2 text-sm text-foreground/90 transition-colors hover:bg-muted/70"
            style={{ paddingLeft: `${8 + Math.min(indent, 4) * 12}px` }}
        >
            <input
                type="radio"
                name={name}
                value={value}
                checked={checked}
                onChange={() => onChange(value)}
                className="size-4 shrink-0 accent-primary"
            />
            <span className="min-w-0 flex-1 truncate">{children}</span>
        </label>
    );
}

function OptionList({
    options,
    loading,
    error,
    selected,
    name,
    onSelect,
    onRetry,
}: {
    options: CatalogOption[];
    loading: boolean;
    error: boolean;
    selected?: string;
    name: string;
    onSelect: (value: string | undefined) => void;
    onRetry: () => void;
}) {
    const [search, setSearch] = useState("");
    const visibleOptions = options.filter((option) =>
        option.name.toLowerCase().includes(search.trim().toLowerCase()),
    );

    if (loading) {
        return <p className="px-2 py-3 text-sm text-muted-foreground">Loading options…</p>;
    }

    if (error) {
        return (
            <div className="px-2 py-3 text-sm text-destructive">
                <p>Options could not be loaded.</p>
                <button
                    type="button"
                    onClick={onRetry}
                    className="mt-1 font-semibold text-primary underline-offset-4 hover:underline"
                >
                    Try again
                </button>
            </div>
        );
    }

    return (
        <>
            {options.length > 7 && (
                <Input
                    value={search}
                    onChange={(event) => setSearch(event.target.value)}
                    placeholder={`Search ${name.toLowerCase()}`}
                    aria-label={`Search ${name.toLowerCase()}`}
                    className="mb-2 h-9 rounded-lg"
                />
            )}
            <div className="max-h-48 space-y-0.5 overflow-y-auto pr-1">
                <RadioRow
                    name={name}
                    value=""
                    checked={!selected}
                    onChange={() => onSelect(undefined)}
                >
                    All {name.toLowerCase()}
                </RadioRow>
                {visibleOptions.map((option) => (
                    <RadioRow
                        key={option.slug}
                        name={name}
                        value={option.slug}
                        checked={selected === option.slug}
                        onChange={(value) => onSelect(value)}
                    >
                        {option.name}
                    </RadioRow>
                ))}
                {visibleOptions.length === 0 && (
                    <p className="px-2 py-3 text-sm text-muted-foreground">
                        No matching {name.toLowerCase()}.
                    </p>
                )}
            </div>
        </>
    );
}

export function ProductFilterDialog({
    filters,
    productType,
    onApply,
    onClear,
}: ProductFilterDialogProps) {
    const [open, setOpen] = useState(false);
    const [draft, setDraft] = useState<CatalogFilters>(filters);
    const brands = useCatalogBrands();
    const manufacturers = useCatalogManufacturers();
    const categories = useCatalogCategories();
    const categoryOptions = flattenCategories(
        categories.data ?? [],
        productType,
    );
    const selectedCount = [
        filters.brand,
        filters.manufacturer,
        filters.category,
        filters.sort !== "relevance" ? filters.sort : undefined,
    ].filter(Boolean).length;

    const handleOpenChange = (nextOpen: boolean) => {
        if (nextOpen) {
            setDraft(filters);
        }
        setOpen(nextOpen);
    };

    const updateSort = (sort: string) => {
        setDraft((current) => ({
            ...current,
            sort: sort as CatalogSort,
        }));
    };

    return (
        <>
            <Button
                type="button"
                variant="outline"
                onClick={() => handleOpenChange(true)}
                className="h-10 gap-2 rounded-xl border-border/80 bg-card px-3.5 shadow-sm"
            >
                <SlidersHorizontal className="size-4" />
                Filters
                {selectedCount > 0 && (
                    <span className="flex size-5 items-center justify-center rounded-full bg-primary text-[11px] font-semibold text-primary-foreground">
                        {selectedCount}
                    </span>
                )}
            </Button>

            <Dialog open={open} onOpenChange={handleOpenChange}>
                <DialogContent className="flex max-h-[min(88vh,760px)] flex-col gap-0 overflow-hidden p-0 sm:max-w-xl">
                    <DialogHeader className="shrink-0 border-b px-6 py-5 pr-12">
                        <DialogTitle className="text-lg">Filters</DialogTitle>
                        <DialogDescription>
                            Narrow down the products to find what you need.
                        </DialogDescription>
                    </DialogHeader>

                    <div className="min-h-0 flex-1 overflow-y-auto px-6">
                        <FilterSection title="Sort by price">
                            <div className="space-y-0.5">
                                <RadioRow
                                    name="sort"
                                    value="relevance"
                                    checked={draft.sort === "relevance"}
                                    onChange={updateSort}
                                >
                                    Recommended
                                </RadioRow>
                                <RadioRow
                                    name="sort"
                                    value="price-low"
                                    checked={draft.sort === "price-low"}
                                    onChange={updateSort}
                                >
                                    <span className="flex items-center gap-2">
                                        <ArrowUpWideNarrow className="size-4 text-muted-foreground" />
                                        Price: low to high
                                    </span>
                                </RadioRow>
                                <RadioRow
                                    name="sort"
                                    value="price-high"
                                    checked={draft.sort === "price-high"}
                                    onChange={updateSort}
                                >
                                    <span className="flex items-center gap-2">
                                        <ArrowDownWideNarrow className="size-4 text-muted-foreground" />
                                        Price: high to low
                                    </span>
                                </RadioRow>
                            </div>
                            <p className="mt-2 px-2 text-xs leading-5 text-muted-foreground">
                                Price sorting applies to products on the current page.
                            </p>
                        </FilterSection>

                        <FilterSection title="Brands">
                            <OptionList
                                options={brands.data ?? []}
                                loading={brands.isPending}
                                error={brands.isError}
                                selected={draft.brand}
                                name="Brands"
                                onSelect={(brand) =>
                                    setDraft((current) => ({ ...current, brand }))
                                }
                                onRetry={() => void brands.refetch()}
                            />
                        </FilterSection>

                        <FilterSection title="Manufacturers">
                            <OptionList
                                options={manufacturers.data ?? []}
                                loading={manufacturers.isPending}
                                error={manufacturers.isError}
                                selected={draft.manufacturer}
                                name="Manufacturers"
                                onSelect={(manufacturer) =>
                                    setDraft((current) => ({ ...current, manufacturer }))
                                }
                                onRetry={() => void manufacturers.refetch()}
                            />
                        </FilterSection>

                        <FilterSection title="Categories">
                            {categories.isPending ? (
                                <p className="px-2 py-3 text-sm text-muted-foreground">
                                    Loading categories…
                                </p>
                            ) : categories.isError ? (
                                <div className="px-2 py-3 text-sm text-destructive">
                                    <p>Categories could not be loaded.</p>
                                    <button
                                        type="button"
                                        onClick={() => void categories.refetch()}
                                        className="mt-1 font-semibold text-primary underline-offset-4 hover:underline"
                                    >
                                        Try again
                                    </button>
                                </div>
                            ) : categoryOptions.length === 0 ? (
                                <p className="px-2 py-3 text-sm text-muted-foreground">
                                    No categories available.
                                </p>
                            ) : (
                                <div className="max-h-52 space-y-0.5 overflow-y-auto pr-1">
                                    <RadioRow
                                        name="Categories"
                                        value=""
                                        checked={!draft.category}
                                        onChange={() =>
                                            setDraft((current) => ({
                                                ...current,
                                                category: undefined,
                                            }))
                                        }
                                    >
                                        All categories
                                    </RadioRow>
                                    {categoryOptions.map((category) => (
                                        <RadioRow
                                            key={category.slug}
                                            name="Categories"
                                            value={category.slug}
                                            checked={draft.category === category.slug}
                                            onChange={(value) =>
                                                setDraft((current) => ({
                                                    ...current,
                                                    category: value,
                                                }))
                                            }
                                            indent={category.depth}
                                        >
                                            {category.name}
                                        </RadioRow>
                                    ))}
                                </div>
                            )}
                        </FilterSection>
                    </div>

                    <DialogFooter className="shrink-0 gap-2 border-t bg-background px-6 py-4 sm:justify-between">
                        <Button
                            type="button"
                            variant="ghost"
                            onClick={() => {
                                onClear();
                                setOpen(false);
                            }}
                            className="gap-2"
                        >
                            <RotateCcw className="size-4" />
                            Clear all
                        </Button>
                        <Button
                            type="button"
                            onClick={() => {
                                onApply(draft);
                                setOpen(false);
                            }}
                            className="gap-2"
                        >
                            <Check className="size-4" />
                            Show results
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </>
    );
}
