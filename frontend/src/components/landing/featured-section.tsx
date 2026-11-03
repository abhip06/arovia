import { ProductList } from "@/features/catalog/components/product-list";

const collections = [
    { title: "Hair Care", categorySlug: "hair-care", productTypeSlug: "personal-care" },
    { title: "Vitamins & Minerals", categorySlug: "vitamins-minerals", productTypeSlug: "wellness" },
    { title: "Sports Nutrition", categorySlug: "sports-nutrition", productTypeSlug: "wellness" },
    { title: "Oral Care", categorySlug: "oral-care", productTypeSlug: "personal-care" },
    { title: "Blood Glucose Tests", categorySlug: "blood-glucose-tests", productTypeSlug: "diagnostic" },
    { title: "Fever Relief", categorySlug: "fever-relief", productTypeSlug: "medicine" },
];

export function FeaturedSection() {
    return (
        <section className="bg-muted/20 py-16 sm:py-20 lg:py-24">
            <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                <div className="mb-7 flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
                    <div className="max-w-2xl">
                        <p className="text-sm font-semibold text-primary">Discover more</p>
                        <h2 className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl">Find what you need.</h2>
                        <p className="mt-3 text-sm leading-6 text-muted-foreground sm:text-base">
                            Browse trusted essentials by the care that matters to you.
                        </p>
                    </div>
                    {/* <Link href="/otc/medicine" className="group inline-flex h-10 shrink-0 items-center gap-2 rounded-xl border border-border/70 bg-card px-4 text-sm font-semibold transition-colors hover:border-primary/30 hover:bg-primary/5 hover:text-primary">
                        Browse all products <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
                    </Link> */}
                </div>

                <div className="divide-y divide-border/60">
                    {collections.map((collection) => (
                        <ProductList key={collection.categorySlug} {...collection} />
                    ))}
                </div>
            </div>
        </section>
    );
}
