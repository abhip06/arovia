import Link from "next/link";
import {
    ArrowRight,
    BellRing,
    CalendarClock,
    FlaskConical,
    HeartHandshake,
    ShieldCheck,
    Sparkles,
} from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const services = {
    labs: {
        label: "Lab tests",
        eyebrow: "Your health, understood",
        title: "A clearer picture of your health is on its way.",
        description: "We’re preparing a thoughtful, convenient way to discover diagnostic tests and care options. We’ll be ready to help you take the next step soon.",
        icon: FlaskConical,
        accent: "from-sky-500/20 via-cyan-400/10 to-primary/10",
        features: ["Trusted diagnostic partners", "Simple test discovery", "Care that comes to you"],
    },
    insurance: {
        label: "Medical insurance",
        eyebrow: "More support for your care",
        title: "Health cover, made easier to explore.",
        description: "We’re working on a simpler way to discover medical insurance options and understand the support available to you.",
        icon: HeartHandshake,
        accent: "from-violet-500/20 via-fuchsia-400/10 to-primary/10",
        features: ["Plans for different needs", "Clear, useful information", "A more confident next step"],
    },
} as const;

export function ComingSoonPage({ service }: { service: keyof typeof services }) {
    const content = services[service];
    const Icon = content.icon;

    return (
        <section className="relative isolate flex min-h-[calc(100vh-4rem)] items-center overflow-hidden bg-background px-4 py-16 sm:px-6 lg:px-8">
            <div aria-hidden="true" className={`absolute inset-0 -z-10 bg-gradient-to-br ${content.accent}`} />
            <div aria-hidden="true" className="absolute -right-36 top-8 -z-10 size-[28rem] rounded-full bg-primary/10 blur-[100px]" />
            <div className="mx-auto grid w-full max-w-6xl items-center gap-12 lg:grid-cols-[1.1fr_0.9fr]">
                <div>
                    <span className="inline-flex items-center gap-2 rounded-full border border-primary/15 bg-background/75 px-3.5 py-2 text-xs font-semibold text-primary shadow-sm backdrop-blur">
                        <Sparkles className="size-3.5" /> Coming soon · {content.label}
                    </span>
                    <p className="mt-7 text-sm font-semibold uppercase tracking-[0.14em] text-primary">{content.eyebrow}</p>
                    <h1 className="mt-3 max-w-2xl text-balance text-4xl font-semibold tracking-tight sm:text-5xl lg:text-6xl">{content.title}</h1>
                    <p className="mt-5 max-w-xl text-base leading-7 text-muted-foreground">{content.description}</p>
                    <div className="mt-8 flex flex-col gap-3 sm:flex-row">
                        <Link href="/otc/medicine" className={cn(buttonVariants(), "h-11 gap-2 rounded-xl px-5")}>
                            Explore the shop <ArrowRight className="size-4" />
                        </Link>
                        <Link href="/contact" className={cn(buttonVariants({ variant: "outline" }), "h-11 gap-2 rounded-xl px-5")}>
                            <BellRing className="size-4" /> Talk to our team
                        </Link>
                    </div>
                </div>

                <div className="relative mx-auto w-full max-w-md">
                    <div aria-hidden="true" className="absolute -inset-5 rounded-[2.5rem] bg-primary/10 blur-2xl" />
                    <div className="relative overflow-hidden rounded-[2rem] border border-border/70 bg-card/90 p-6 shadow-2xl shadow-primary/10 backdrop-blur sm:p-8">
                        <div className="flex items-start justify-between gap-4">
                            <div className="flex size-14 items-center justify-center rounded-2xl bg-primary/10 text-primary"><Icon className="size-7" /></div>
                            <span className="rounded-full border border-border/70 bg-background px-3 py-1 text-[11px] font-medium text-muted-foreground">In the works</span>
                        </div>
                        <h2 className="mt-7 text-xl font-semibold">Designed around real care</h2>
                        <p className="mt-2 text-sm leading-6 text-muted-foreground">We’re building this experience with clarity, convenience and your wellbeing in mind.</p>
                        <div className="mt-6 space-y-3">
                            {content.features.map((feature) => (
                                <div key={feature} className="flex items-center gap-3 rounded-xl border border-border/60 bg-background/75 px-3.5 py-3">
                                    <ShieldCheck className="size-4 shrink-0 text-primary" />
                                    <span className="text-sm font-medium">{feature}</span>
                                </div>
                            ))}
                        </div>
                        <div className="mt-6 flex items-center gap-2 border-t border-border/70 pt-5 text-xs text-muted-foreground">
                            <CalendarClock className="size-4 text-primary" />
                            We’re getting the details right before launch.
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}
