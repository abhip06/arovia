import Link from "next/link";
import type { Metadata } from "next";
import { ArrowLeft, ArrowRight, Compass, Home, Search } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export const metadata: Metadata = {
    title: "Page not found",
    description: "We couldn’t find that page. Explore the Arovia shop or return home.",
};

export default function NotFound() {
    return (
        <section className="mt-24 relative isolate flex min-h-[70vh] items-center justify-center overflow-hidden px-4 py-16 sm:px-6 lg:px-8">
            <div aria-hidden="true" className="absolute inset-0 -z-10 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-primary/10 via-background to-background" />
            <div aria-hidden="true" className="absolute -left-32 top-1/3 -z-10 size-72 rounded-full bg-sky-400/10 blur-[90px]" />
            <div className="mx-auto w-full max-w-xl text-center">
                <div className="mx-auto flex size-16 items-center justify-center rounded-2xl border border-primary/15 bg-primary/5 text-primary shadow-sm">
                    <Compass className="size-8" />
                </div>
                <p className="mt-7 text-sm font-semibold uppercase tracking-[0.16em] text-primary">A little off the path</p>
                <p className="mt-2 text-7xl font-bold tracking-[-0.07em] text-foreground sm:text-8xl">404</p>
                <h1 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">We can’t find that page.</h1>
                <p className="mx-auto mt-3 max-w-md text-sm leading-6 text-muted-foreground sm:text-base">
                    The link may have changed or the page may have moved. Let’s get you back to something useful.
                </p>
                <div className="mt-7 flex flex-col justify-center gap-3 sm:flex-row">
                    <Link href="/" className={cn(buttonVariants({ variant: "outline" }), "h-11 gap-2 rounded-xl px-5")}>
                        <Home className="size-4" /> Go home
                    </Link>
                    <Link href="/otc/medicine" className={cn(buttonVariants(), "h-11 gap-2 rounded-xl px-5")}>
                        <Search className="size-4" /> Browse products <ArrowRight className="size-4" />
                    </Link>
                </div>
                <Link href="/contact" className="mt-8 inline-flex items-center gap-2 text-xs font-medium text-muted-foreground transition-colors hover:text-foreground">
                    <ArrowLeft className="size-3.5" /> Need help? Contact us
                </Link>
            </div>
        </section>
    );
}
