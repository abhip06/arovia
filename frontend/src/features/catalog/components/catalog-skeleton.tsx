export function CatalogSkeleton() {
    return (
        <div className="mx-auto w-full max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
            <div className="animate-pulse">
                <div className="h-3 w-36 rounded bg-muted" />
                <div className="mt-5 h-9 w-72 max-w-full rounded bg-muted" />
                <div className="mt-3 h-4 w-96 max-w-full rounded bg-muted" />
                <div className="mt-8 flex gap-3">
                    <div className="h-10 w-28 rounded-xl bg-muted" />
                    <div className="h-10 w-36 rounded-xl bg-muted" />
                </div>
                <div className="mt-7 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
                    {Array.from({ length: 12 }, (_, index) => (
                        <div key={index} className="rounded-2xl border border-border/70 p-3">
                            <div className="aspect-[4/3] rounded-xl bg-muted" />
                            <div className="mt-4 h-3 w-20 rounded bg-muted" />
                            <div className="mt-2 h-4 w-full rounded bg-muted" />
                            <div className="mt-2 h-4 w-2/3 rounded bg-muted" />
                            <div className="mt-5 h-5 w-24 rounded bg-muted" />
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
