"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { motion } from "framer-motion";
import {
    ArrowLeft,
    ArrowRight,
    CheckCircle2,
    ClipboardList,
    Info,
    PackageCheck,
    PackageOpen,
    RotateCcw,
    ShieldCheck,
    Truck,
} from "lucide-react";

import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";

import { AddToCartButton } from "@/features/cart/components/add-to-cart-button";
import { formatPrice } from "@/lib/formatters";
import { cn } from "@/lib/utils";
import { useLocationStore } from "@/store/location-store";
import { useProductDetail } from "../hooks/use-product-detail";

export function ProductDetailPage({
    productSlug,
    sku,
}: {
    productSlug: string;
    sku: string;
}) {
    const detailQuery = useProductDetail(productSlug, sku);

    const city = useLocationStore((state) => state.city);
    const locationState = useLocationStore((state) => state.state);
    const postalCode = useLocationStore((state) => state.pincode);

    const [selectedImageUrl, setSelectedImageUrl] =
        useState<string | null>(null);

    useEffect(() => {
        setSelectedImageUrl(null);
    }, [productSlug, sku]);

    if (detailQuery.isPending) {
        return <ProductDetailSkeleton />;
    }

    if (detailQuery.isError) {
        return (
            <section className="flex min-h-[65vh] items-center justify-center px-4 py-16">
                <div className="flex max-w-md flex-col items-center text-center">
                    <div className="grid size-16 place-items-center rounded-2xl border border-primary/10 bg-primary/5 text-primary">
                        <PackageOpen className="size-7" />
                    </div>

                    <h1 className="mt-5 text-2xl font-semibold tracking-tight">
                        Product details are unavailable
                    </h1>

                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                        {detailQuery.error instanceof Error
                            ? detailQuery.error.message
                            : "This product may have been removed. Please try another item."}
                    </p>

                    <Button
                        variant="outline"
                        className="mt-6 rounded-xl"
                        onClick={() => void detailQuery.refetch()}
                    >
                        Try again
                    </Button>

                    <Link
                        href="/otc/medicine"
                        className={cn(
                            buttonVariants({ variant: "ghost" }),
                            "mt-2 gap-2 rounded-xl text-muted-foreground",
                        )}
                    >
                        <ArrowLeft className="size-4" />
                        Back to shopping
                    </Link>
                </div>
            </section>
        );
    }

    const {
        product,
        variant,
        information,
        availability,
        images,
    } = detailQuery.data;

    const sortedImages = [...images].sort((left, right) => {
        if (left.isPrimary !== right.isPrimary) {
            return left.isPrimary ? -1 : 1;
        }

        return left.sortOrder - right.sortOrder;
    });

    const primaryImage = sortedImages[0];

    const activeImage =
        sortedImages.find(
            (image) => image.url === selectedImageUrl,
        ) ?? primaryImage;

    const price = variant.sellingPrice;
    const mrp = availability?.mrp;

    const discountPercentage =
        mrp != null && mrp > price && mrp > 0
            ? Math.round(((mrp - price) / mrp) * 100)
            : 0;

    const primaryCategory = product.categories?.[0];

    const productTypePath = product.type
        .replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`)
        .replace(/^-/, "");

    const deliveryLocation =
        city && locationState
            ? `${city}, ${locationState}${postalCode ? ` · ${postalCode}` : ""}`
            : postalCode ?? availability?.pincode ?? null;

    const highlights = information?.highlights ?? [];
    const specifications = information?.specifications ?? [];
    const faqs = information?.faqs ?? [];
    const warnings = information?.warnings ?? [];
    const sideEffects = information?.sideEffects ?? [];
    const ingredients = product.ingredients ?? [];

    return (
        <main className="min-h-screen bg-background pb-16">
            <div className="mx-auto max-w-7xl px-4 pb-12 pt-24 sm:px-6 sm:pt-28 lg:px-8">
                {/* Breadcrumb */}
                <nav
                    aria-label="Breadcrumb"
                    className="mb-7 flex min-w-0 flex-wrap items-center gap-2 text-xs text-muted-foreground"
                >
                    <Link
                        href="/otc/medicine"
                        className="transition-colors hover:text-foreground"
                    >
                        Shop
                    </Link>

                    <span aria-hidden="true">/</span>

                    <Link
                        href={`/otc/${productTypePath}`}
                        className="transition-colors hover:text-foreground"
                    >
                        {formatLabel(product.type)}
                    </Link>

                    {primaryCategory && (
                        <>
                            <span aria-hidden="true">/</span>

                            <Link
                                href={`/otc/${productTypePath}?category=${encodeURIComponent(primaryCategory.slug)}`}
                                className="transition-colors hover:text-foreground"
                            >
                                {primaryCategory.name}
                            </Link>
                        </>
                    )}

                    <span aria-hidden="true">/</span>

                    <span
                        aria-current="page"
                        className="max-w-full truncate font-medium text-foreground"
                    >
                        {product.name}
                    </span>
                </nav>

                {/* Product overview */}
                <div className="grid items-start gap-8 lg:grid-cols-[1.05fr_0.95fr] lg:gap-12">
                    {/* Product gallery */}
                    <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.35 }}
                        className="min-w-0"
                    >
                        <div className="relative flex min-h-[340px] items-center justify-center overflow-hidden rounded-[1.75rem] border border-border/70 bg-gradient-to-br from-emerald-50 via-white to-sky-50 p-6 dark:from-emerald-950/35 dark:via-card dark:to-sky-950/25 sm:min-h-[480px] lg:min-h-[540px]">
                            {activeImage ? (
                                <Image
                                    key={activeImage.url}
                                    src={activeImage.url}
                                    alt={
                                        activeImage.altText ||
                                        product.name
                                    }
                                    fill
                                    priority
                                    sizes="(max-width: 1024px) 100vw, 52vw"
                                    className="object-contain p-8 transition-transform duration-500 hover:scale-[1.035] sm:p-12"
                                />
                            ) : (
                                <div className="flex flex-col items-center gap-3 text-primary/50">
                                    <PackageOpen className="size-16 stroke-[1.3]" />
                                    <span className="text-xs font-semibold uppercase tracking-[0.16em]">
                                        Arovia essentials
                                    </span>
                                </div>
                            )}

                            <div className="absolute left-4 top-4 flex flex-wrap gap-2 sm:left-5 sm:top-5">
                                <Badge
                                    variant="secondary"
                                    className="border border-white/50 bg-background/90 text-foreground shadow-sm backdrop-blur"
                                >
                                    {formatLabel(product.type)}
                                </Badge>

                                {product.requiresPrescription && (
                                    <Badge
                                        variant="outline"
                                        className="border-amber-300/80 bg-amber-50/95 text-amber-900 shadow-sm dark:border-amber-800 dark:bg-amber-950/80 dark:text-amber-200"
                                    >
                                        Prescription required
                                    </Badge>
                                )}
                            </div>

                            {sortedImages.length > 1 && (
                                <div className="absolute bottom-4 right-4 rounded-full border border-white/60 bg-background/90 px-3 py-1.5 text-[11px] font-medium text-muted-foreground shadow-sm backdrop-blur">
                                    {sortedImages.findIndex(
                                        (image) =>
                                            image.url ===
                                            activeImage?.url,
                                    ) + 1}{" "}
                                    / {sortedImages.length}
                                </div>
                            )}
                        </div>

                        {/* Thumbnail gallery */}
                        {sortedImages.length > 1 && (
                            <div
                                aria-label="Product images"
                                className="mt-4 flex gap-3 overflow-x-auto pb-1"
                            >
                                {sortedImages.map((image, index) => {
                                    const isSelected =
                                        image.url === activeImage?.url;

                                    return (
                                        <button
                                            key={`${image.url}-${image.sortOrder}`}
                                            type="button"
                                            aria-label={`View product image ${index + 1}`}
                                            aria-pressed={isSelected}
                                            onClick={() =>
                                                setSelectedImageUrl(
                                                    image.url,
                                                )
                                            }
                                            className={cn(
                                                "relative size-[76px] shrink-0 overflow-hidden rounded-xl border bg-card p-2 transition-all duration-200 sm:size-[88px]",
                                                isSelected
                                                    ? "border-primary ring-2 ring-primary/15"
                                                    : "border-border/70 hover:border-primary/40",
                                            )}
                                        >
                                            <Image
                                                src={image.url}
                                                alt={
                                                    image.altText ||
                                                    `${product.name} image ${index + 1}`
                                                }
                                                width={88}
                                                height={88}
                                                sizes="88px"
                                                className="size-full object-contain"
                                            />
                                        </button>
                                    );
                                })}
                            </div>
                        )}
                    </motion.div>

                    {/* Product information and purchase */}
                    <motion.div
                        initial={{ opacity: 0, y: 14 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{
                            duration: 0.4,
                            delay: 0.06,
                        }}
                        className="flex min-w-0 flex-col"
                    >
                        <div className="flex flex-wrap items-center gap-2">
                            <span className="text-sm font-semibold text-primary">
                                {product.brand?.name ??
                                    product.manufacturer.name}
                            </span>

                            {product.returnable && (
                                <Badge
                                    variant="outline"
                                    className="gap-1 rounded-full border-primary/20 bg-primary/5 text-primary"
                                >
                                    <RotateCcw className="size-3" />
                                    {product.returnWindowDays}-day returns
                                </Badge>
                            )}
                        </div>

                        <h1 className="mt-3 text-3xl font-semibold leading-tight tracking-tight sm:text-4xl lg:text-[2.65rem]">
                            {product.name}
                        </h1>

                        <p className="mt-3 text-sm leading-6 text-muted-foreground">
                            {variant.packageQuantity}{" "}
                            {variant.packageUnit}

                            {variant.netQuantity != null &&
                                variant.netQuantityUnit && (
                                    <>
                                        {" "}
                                        <span aria-hidden="true">·</span>{" "}
                                        {variant.netQuantity}{" "}
                                        {variant.netQuantityUnit}
                                    </>
                                )}
                        </p>

                        <div className="mt-6 flex flex-wrap items-center gap-x-3 gap-y-2">
                            <span className="text-3xl font-bold tracking-tight">
                                {formatPrice(price)}
                            </span>

                            {mrp != null && mrp > price && (
                                <>
                                    <span className="text-sm text-muted-foreground">
                                        MRP{" "}
                                        <span className="line-through">
                                            {formatPrice(mrp)}
                                        </span>
                                    </span>

                                    {discountPercentage > 0 && (
                                        <Badge className="rounded-full border-0 bg-emerald-100 text-emerald-800 hover:bg-emerald-100 dark:bg-emerald-950 dark:text-emerald-300">
                                            Save {discountPercentage}%
                                        </Badge>
                                    )}
                                </>
                            )}
                        </div>

                        {mrp == null && (
                            <p className="mt-2 text-xs text-muted-foreground">
                                MRP information is not currently
                                available.
                            </p>
                        )}

                        <Card className="mt-7 overflow-hidden rounded-2xl border-border/70 shadow-sm">
                            <CardContent className="p-5 sm:p-6">
                                <div className="flex items-start gap-3">
                                    <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                        <Truck className="size-5" />
                                    </span>

                                    <div className="min-w-0 flex-1">
                                        <h2 className="text-sm font-semibold">
                                            Delivery information
                                        </h2>

                                        {deliveryLocation ? (
                                            <>
                                                <p className="mt-1 break-words text-sm text-muted-foreground">
                                                    Location:{" "}
                                                    {deliveryLocation}
                                                </p>

                                                <p className="mt-2 text-xs leading-5 text-muted-foreground">
                                                    {availability
                                                        ? "Location-specific pricing and availability information was returned."
                                                        : "Delivery serviceability has not been confirmed by the product response yet."}
                                                </p>
                                            </>
                                        ) : (
                                            <>
                                                <p className="mt-1 text-sm text-muted-foreground">
                                                    Select a delivery
                                                    location to check
                                                    serviceability.
                                                </p>

                                                <Link
                                                    href="/account/addresses"
                                                    className="mt-2 inline-flex items-center gap-1 text-xs font-semibold text-primary hover:underline"
                                                >
                                                    Manage saved addresses
                                                    <ArrowRight className="size-3.5" />
                                                </Link>
                                            </>
                                        )}
                                    </div>
                                </div>

                                <Separator className="my-5" />

                                <AddToCartButton
                                    itemId={variant.id}
                                    productName={product.name}
                                    className="h-12 w-full rounded-xl text-sm"
                                />

                                <div className="mt-4 grid gap-3 sm:grid-cols-2">
                                    <Assurance
                                        icon={ShieldCheck}
                                        title="Secure shopping"
                                    />

                                    {product.returnable ? (
                                        <Assurance
                                            icon={PackageCheck}
                                            title={`Returns within ${product.returnWindowDays} days`}
                                        />
                                    ) : (
                                        <Assurance
                                            icon={CheckCircle2}
                                            title="Product details verified"
                                        />
                                    )}
                                </div>
                            </CardContent>
                        </Card>

                        <div className="mt-6">
                            <p className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                                Product information
                            </p>

                            <div className="grid gap-3 sm:grid-cols-2">
                                {product.brand && (
                                    <DetailTile
                                        label="Brand"
                                        value={product.brand.name}
                                    />
                                )}

                                <DetailTile
                                    label="Manufacturer"
                                    value={product.manufacturer.name}
                                />

                                {product.categories.map((category) => (
                                    <DetailTile
                                        key={category.slug}
                                        label="Category"
                                        value={category.name}
                                    />
                                ))}
                            </div>
                        </div>
                    </motion.div>
                </div>

                {/* Detailed product information */}
                {information && (
                    <div className="mt-14 space-y-6 sm:mt-16">
                        <div className="max-w-2xl">
                            <p className="text-xs font-semibold uppercase tracking-[0.14em] text-primary">
                                Get to know your product
                            </p>

                            <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">
                                Product information
                            </h2>

                            <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                The details, directions, and
                                information provided for this product.
                            </p>
                        </div>

                        <div className="grid items-start gap-5 lg:grid-cols-[1.15fr_0.85fr]">
                            {/* Description, highlights and specifications */}
                            <div className="space-y-5">
                                <Card className="rounded-2xl border-border/70 shadow-sm">
                                    <CardContent className="p-5 sm:p-7">
                                        <div className="flex items-center gap-2">
                                            <span className="flex size-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                                <Info className="size-4" />
                                            </span>

                                            <h3 className="text-base font-semibold">
                                                About this product
                                            </h3>
                                        </div>

                                        {information.description && (
                                            <p className="mt-4 text-sm leading-7 text-muted-foreground">
                                                {information.description}
                                            </p>
                                        )}

                                        {highlights.length > 0 && (
                                            <div className="mt-6">
                                                <h4 className="text-sm font-semibold">
                                                    Highlights
                                                </h4>

                                                <ul className="mt-3 grid gap-3 sm:grid-cols-2">
                                                    {highlights.map(
                                                        (highlight) => (
                                                            <li
                                                                key={highlight}
                                                                className="flex items-start gap-2.5 rounded-xl border border-border/60 bg-muted/20 p-3 text-sm leading-5"
                                                            >
                                                                <CheckCircle2 className="mt-0.5 size-4 shrink-0 text-primary" />
                                                                <span>
                                                                    {highlight}
                                                                </span>
                                                            </li>
                                                        ),
                                                    )}
                                                </ul>
                                            </div>
                                        )}

                                        {specifications.length > 0 && (
                                            <div className="mt-7">
                                                <h4 className="text-sm font-semibold">
                                                    Specifications
                                                </h4>

                                                <dl className="mt-3 overflow-hidden rounded-xl border border-border/70">
                                                    {specifications.map(
                                                        (
                                                            specification,
                                                            index,
                                                        ) => (
                                                            <div
                                                                key={`${specification.key}-${specification.value}`}
                                                                className={cn(
                                                                    "grid grid-cols-[0.8fr_1.2fr] gap-4 px-4 py-3 text-sm",
                                                                    index % 2 === 0
                                                                        ? "bg-muted/20"
                                                                        : "bg-background",
                                                                )}
                                                            >
                                                                <dt className="text-muted-foreground">
                                                                    {
                                                                        specification.key
                                                                    }
                                                                </dt>

                                                                <dd className="break-words font-medium">
                                                                    {
                                                                        specification.value
                                                                    }
                                                                </dd>
                                                            </div>
                                                        ),
                                                    )}
                                                </dl>
                                            </div>
                                        )}
                                    </CardContent>
                                </Card>

                                {ingredients.length > 0 && (
                                    <Card className="rounded-2xl border-border/70 shadow-sm">
                                        <CardContent className="p-5 sm:p-6">
                                            <h3 className="text-base font-semibold">
                                                Ingredients
                                            </h3>

                                            <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                                Ingredients listed for this product.
                                            </p>

                                            <div className="mt-4 flex flex-wrap gap-2">
                                                {ingredients.map(
                                                    (ingredient) => (
                                                        <Badge
                                                            key={`${ingredient.slug}-${ingredient.role}`}
                                                            variant="secondary"
                                                            className="h-auto rounded-lg px-3 py-2 font-normal"
                                                        >
                                                            <span className="font-medium">
                                                                {
                                                                    ingredient.name
                                                                }
                                                            </span>

                                                            {ingredient.strengthValue != null &&
                                                                ingredient.strengthUnit && (
                                                                    <span className="ml-1 text-muted-foreground">
                                                                        ·{" "}
                                                                        {
                                                                            ingredient.strengthValue
                                                                        }{" "}
                                                                        {
                                                                            ingredient.strengthUnit
                                                                        }
                                                                    </span>
                                                                )}
                                                        </Badge>
                                                    ),
                                                )}
                                            </div>
                                        </CardContent>
                                    </Card>
                                )}
                            </div>

                            {/* Usage, directions, storage and safety */}
                            <div className="space-y-5">
                                {information.howToUse && (
                                    <InfoCard
                                        title="How to use"
                                        content={information.howToUse}
                                        icon={CheckCircle2}
                                    />
                                )}

                                {information.directions && (
                                    <InfoCard
                                        title="Directions"
                                        content={information.directions}
                                        icon={ClipboardList}
                                    />
                                )}

                                {information.storageInstructions && (
                                    <InfoCard
                                        title="Storage instructions"
                                        content={
                                            information.storageInstructions
                                        }
                                        icon={PackageOpen}
                                    />
                                )}

                                {warnings.length > 0 && (
                                    <InfoListCard
                                        title="Warnings"
                                        values={warnings}
                                        tone="warning"
                                    />
                                )}

                                {sideEffects.length > 0 && (
                                    <InfoListCard
                                        title="Possible side effects"
                                        values={sideEffects}
                                        tone="neutral"
                                    />
                                )}
                            </div>
                        </div>

                        {/* FAQs */}
                        {faqs.length > 0 && (
                            <section
                                aria-labelledby="product-faq-title"
                                className="grid gap-6 rounded-3xl border border-border/70 bg-card p-5 shadow-sm sm:p-8 lg:grid-cols-[0.65fr_1.35fr] lg:gap-12"
                            >
                                <div>
                                    <div className="flex size-11 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                        <Info className="size-5" />
                                    </div>

                                    <h2
                                        id="product-faq-title"
                                        className="mt-4 text-xl font-semibold tracking-tight sm:text-2xl"
                                    >
                                        Frequently asked questions
                                    </h2>

                                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                        Helpful answers to common
                                        questions about {product.name}.
                                    </p>

                                    <p className="mt-4 text-xs text-muted-foreground">
                                        {faqs.length} question
                                        {faqs.length === 1 ? "" : "s"}
                                    </p>
                                </div>

                                <Accordion
                                    className="w-full"
                                >
                                    {faqs.map((faq, index) => (
                                        <AccordionItem
                                            key={`${faq.question}-${index}`}
                                            value={`faq-${index}`}
                                            className="border-border/70"
                                        >
                                            <AccordionTrigger className="gap-4 py-4 text-left text-sm font-medium hover:no-underline">
                                                {faq.question}
                                            </AccordionTrigger>

                                            <AccordionContent className="pb-5 text-sm leading-7 text-muted-foreground">
                                                {faq.answer}
                                            </AccordionContent>
                                        </AccordionItem>
                                    ))}
                                </Accordion>
                            </section>
                        )}
                    </div>
                )}

                {/* Return to shopping */}
                <div className="mt-12 flex flex-col items-center justify-center gap-3 border-t border-border/70 pt-8 text-center">
                    <p className="text-sm text-muted-foreground">
                        Looking for something else for your everyday care?
                    </p>

                    <Link
                        href="/otc/medicine"
                        className={cn(
                            buttonVariants({ variant: "outline" }),
                            "gap-2 rounded-xl",
                        )}
                    >
                        <ArrowLeft className="size-4" />
                        Continue shopping
                    </Link>
                </div>
            </div>
        </main>
    );
}

function formatLabel(value: string) {
    return value
        .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
        .replace(/[-_]/g, " ")
        .replace(/\s+/g, " ")
        .trim()
        .replace(/\b\w/g, (letter) => letter.toUpperCase());
}

function Assurance({
    icon: Icon,
    title,
}: {
    icon: typeof ShieldCheck;
    title: string;
}) {
    return (
        <div className="flex items-start gap-2 text-xs leading-5 text-muted-foreground">
            <Icon className="mt-0.5 size-4 shrink-0 text-primary" />
            <span>{title}</span>
        </div>
    );
}

function DetailTile({
    label,
    value,
}: {
    label: string;
    value: string;
}) {
    return (
        <div className="rounded-xl border border-border/70 bg-card px-4 py-3.5 transition-colors hover:bg-muted/20">
            <p className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground">
                {label}
            </p>

            <p className="mt-1.5 text-sm font-medium">
                {value}
            </p>
        </div>
    );
}

function InfoCard({
    title,
    content,
    icon: Icon,
}: {
    title: string;
    content: string;
    icon: typeof CheckCircle2;
}) {
    return (
        <Card className="rounded-2xl border-border/70 shadow-sm">
            <CardContent className="p-5">
                <div className="flex items-center gap-2.5">
                    <span className="flex size-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <Icon className="size-4" />
                    </span>

                    <h3 className="text-sm font-semibold">
                        {title}
                    </h3>
                </div>

                <p className="mt-3 text-sm leading-7 text-muted-foreground">
                    {content}
                </p>
            </CardContent>
        </Card>
    );
}

function InfoListCard({
    title,
    values,
    tone,
}: {
    title: string;
    values: string[];
    tone: "warning" | "neutral";
}) {
    const isWarning = tone === "warning";

    return (
        <Card className="rounded-2xl border-border/70 shadow-sm">
            <CardContent className="p-5">
                <div className="flex items-center gap-2.5">
                    <span
                        className={cn(
                            "flex size-9 items-center justify-center rounded-xl",
                            isWarning
                                ? "bg-amber-500/10 text-amber-700 dark:text-amber-300"
                                : "bg-muted text-muted-foreground",
                        )}
                    >
                        <Info className="size-4" />
                    </span>

                    <h3 className="text-sm font-semibold">
                        {title}
                    </h3>
                </div>

                <ul className="mt-4 space-y-3">
                    {values.map((value) => (
                        <li
                            key={value}
                            className="flex items-start gap-2.5 text-sm leading-6 text-muted-foreground"
                        >
                            <span
                                className={cn(
                                    "mt-2 size-1.5 shrink-0 rounded-full",
                                    isWarning
                                        ? "bg-amber-500"
                                        : "bg-primary/60",
                                )}
                            />
                            {value}
                        </li>
                    ))}
                </ul>
            </CardContent>
        </Card>
    );
}

function ProductDetailSkeleton() {
    return (
        <main className="min-h-screen animate-pulse bg-background">
            <div className="mx-auto max-w-7xl px-4 pb-12 pt-28 sm:px-6 lg:px-8">
                <div className="h-3 w-64 max-w-full rounded bg-muted" />

                <div className="mt-8 grid gap-8 lg:grid-cols-2 lg:gap-12">
                    <div className="min-h-[340px] rounded-3xl bg-muted sm:min-h-[480px]" />

                    <div className="space-y-5 py-2">
                        <div className="h-4 w-36 rounded bg-muted" />
                        <div className="h-10 w-4/5 rounded-lg bg-muted" />
                        <div className="h-4 w-48 rounded bg-muted" />
                        <div className="h-9 w-36 rounded bg-muted" />
                        <div className="mt-8 h-48 rounded-2xl bg-muted" />
                        <div className="grid grid-cols-2 gap-3">
                            <div className="h-16 rounded-xl bg-muted" />
                            <div className="h-16 rounded-xl bg-muted" />
                        </div>
                    </div>
                </div>

                <div className="mt-14 grid gap-6 lg:grid-cols-2">
                    <div className="h-72 rounded-2xl bg-muted" />
                    <div className="h-72 rounded-2xl bg-muted" />
                </div>
            </div>
        </main>
    );
}