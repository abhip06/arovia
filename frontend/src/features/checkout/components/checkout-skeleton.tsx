export function CheckoutSkeleton() {
    return (
        <main className="min-h-screen bg-background">
            <div className="container mx-auto px-4 pb-16 pt-24 sm:px-6 lg:px-8">
                <div className="mx-auto max-w-7xl">
                    {/* Back navigation */}
                    <div className="flex items-center gap-2">
                        <SkeletonBlock className="h-4 w-4 rounded-full" />
                        <SkeletonBlock className="h-4 w-28" />
                    </div>

                    {/* Page heading */}
                    <div className="mt-7 flex flex-col gap-3">
                        <SkeletonBlock className="h-4 w-28" />
                        <SkeletonBlock className="h-10 w-52 sm:h-11 sm:w-60" />
                        <SkeletonBlock className="h-4 w-full max-w-2xl" />
                    </div>
                </div>

                <div className="mx-auto mt-8 grid max-w-7xl gap-6 lg:grid-cols-[minmax(0,1fr)_380px]">
                    {/* Main column */}
                    <div className="flex min-w-0 flex-col gap-6">
                        {/* Address */}
                        <CheckoutSectionSkeleton
                            variant="address"
                        />

                        {/* Items */}
                        <CheckoutSectionSkeleton
                            variant="items"
                        />

                        {/* Delivery */}
                        <CheckoutSectionSkeleton
                            variant="delivery"
                        />

                        {/* Payment */}
                        <CheckoutSectionSkeleton
                            variant="payment"
                        />

                        {/* Security note */}
                        <div className="flex items-center gap-3 px-1">
                            <SkeletonBlock className="size-4 rounded-full" />
                            <SkeletonBlock className="h-4 w-full max-w-lg" />
                        </div>
                    </div>

                    {/* Summary */}
                    <div className="lg:sticky lg:top-24">
                        <CheckoutSummarySkeleton />
                    </div>
                </div>
            </div>
        </main>
    );
}

function CheckoutSectionSkeleton({
    variant,
}: {
    variant: "address" | "items" | "delivery" | "payment";
}) {
    return (
        <section className="rounded-3xl border border-border/70 bg-card p-4 sm:p-6">
            {/* Section heading */}
            <div className="flex items-start gap-3">
                <SkeletonBlock className="size-10 shrink-0 rounded-xl" />

                <div className="flex min-w-0 flex-1 flex-col gap-2">
                    <SkeletonBlock className="h-5 w-36" />
                    <SkeletonBlock className="h-4 w-full max-w-md" />
                </div>
            </div>

            {variant === "address" && (
                <div className="mt-5 rounded-2xl border border-border/60 bg-muted/20 p-4 sm:p-5">
                    <div className="flex items-start gap-3">
                        <SkeletonBlock className="size-10 shrink-0 rounded-xl" />

                        <div className="flex min-w-0 flex-1 flex-col gap-2">
                            <SkeletonBlock className="h-4 w-40" />
                            <SkeletonBlock className="h-4 w-full max-w-md" />
                            <SkeletonBlock className="h-4 w-3/4 max-w-sm" />
                        </div>
                    </div>

                    <SkeletonBlock className="mt-5 h-11 w-36 rounded-xl" />
                </div>
            )}

            {variant === "items" && (
                <div className="mt-5 flex flex-col divide-y divide-border/70">
                    <CheckoutItemSkeleton />
                    <CheckoutItemSkeleton />
                    <CheckoutItemSkeleton />
                </div>
            )}

            {variant === "delivery" && (
                <div className="mt-5 grid gap-3 sm:grid-cols-2">
                    <InfoTileSkeleton />
                    <InfoTileSkeleton />
                </div>
            )}

            {variant === "payment" && (
                <div className="mt-5 rounded-2xl border border-border/60 bg-muted/20 p-4 sm:p-5">
                    <div className="flex items-start gap-3">
                        <SkeletonBlock className="size-10 shrink-0 rounded-xl" />

                        <div className="flex min-w-0 flex-1 flex-col gap-2">
                            <SkeletonBlock className="h-4 w-36" />
                            <SkeletonBlock className="h-4 w-full max-w-lg" />
                            <SkeletonBlock className="h-4 w-4/5 max-w-md" />
                        </div>
                    </div>
                </div>
            )}
        </section>
    );
}

function CheckoutItemSkeleton() {
    return (
        <div className="flex gap-4 py-4 first:pt-0 last:pb-0">
            <SkeletonBlock className="size-20 shrink-0 rounded-xl" />

            <div className="flex min-w-0 flex-1 flex-col gap-3">
                <div className="flex items-start justify-between gap-4">
                    <div className="flex min-w-0 flex-1 flex-col gap-2">
                        <SkeletonBlock className="h-4 w-3/4 max-w-xs" />
                        <SkeletonBlock className="h-3.5 w-24" />
                        <SkeletonBlock className="h-5 w-20 rounded-md" />
                    </div>

                    <div className="flex shrink-0 flex-col items-end gap-2">
                        <SkeletonBlock className="h-4 w-20" />
                        <SkeletonBlock className="h-3 w-16" />
                    </div>
                </div>

                <div className="flex items-center justify-between gap-4">
                    <SkeletonBlock className="h-4 w-28" />
                    <SkeletonBlock className="h-8 w-20 rounded-lg" />
                </div>
            </div>
        </div>
    );
}

function InfoTileSkeleton() {
    return (
        <div className="rounded-2xl border border-border/60 bg-muted/20 p-4">
            <div className="flex items-center gap-2.5">
                <SkeletonBlock className="size-8 shrink-0 rounded-lg" />
                <SkeletonBlock className="h-4 w-24" />
            </div>

            <div className="mt-4 flex flex-col gap-2">
                <SkeletonBlock className="h-3.5 w-full" />
                <SkeletonBlock className="h-3.5 w-5/6" />
                <SkeletonBlock className="h-3.5 w-2/3" />
            </div>
        </div>
    );
}

function CheckoutSummarySkeleton() {
    return (
        <div className="rounded-3xl border border-border/70 bg-card p-5 sm:p-6">
            <div className="flex flex-col gap-3">
                <SkeletonBlock className="h-4 w-28" />
                <SkeletonBlock className="h-7 w-48" />
            </div>

            <SkeletonBlock className="my-5 h-px w-full" />

            <div className="flex flex-col gap-4">
                <SummaryRowSkeleton />
                <SummaryRowSkeleton />
                <SummaryRowSkeleton />
            </div>

            <SkeletonBlock className="my-5 h-px w-full" />

            <div className="flex items-end justify-between gap-4">
                <div className="flex flex-col gap-2">
                    <SkeletonBlock className="h-4 w-28" />
                    <SkeletonBlock className="h-3 w-40" />
                </div>

                <SkeletonBlock className="h-7 w-24" />
            </div>

            <SkeletonBlock className="mt-6 h-12 w-full rounded-xl" />

            <SkeletonBlock className="mx-auto mt-4 h-3 w-56" />

            <div className="mt-5 flex flex-col gap-3 border-t border-border/70 pt-5">
                <div className="flex items-center gap-2.5">
                    <SkeletonBlock className="size-4 rounded-full" />
                    <SkeletonBlock className="h-3.5 w-40" />
                </div>

                <div className="flex items-center gap-2.5">
                    <SkeletonBlock className="size-4 rounded-full" />
                    <SkeletonBlock className="h-3.5 w-48" />
                </div>
            </div>
        </div>
    );
}

function SummaryRowSkeleton() {
    return (
        <div className="flex items-center justify-between gap-4">
            <SkeletonBlock className="h-4 w-20" />
            <SkeletonBlock className="h-4 w-24" />
        </div>
    );
}

function SkeletonBlock({
    className = "",
}: {
    className?: string;
}) {
    return (
        <div
            aria-hidden="true"
            className={`animate-pulse rounded-md bg-muted ${className}`}
        />
    );
}