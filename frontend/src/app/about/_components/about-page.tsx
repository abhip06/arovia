"use client";
import { motion } from "framer-motion";

import {
    ArrowRight,
    HeartPulse,
    Leaf,
    ShieldCheck,
    Sparkles,
    Truck,
} from "lucide-react";

import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import { HeroSection } from "./hero-section";

const principles = [
    {
        icon: HeartPulse,
        title: "People first",
        description:
            "Healthcare is personal. Every part of Arovia is designed around making everyday health decisions feel easier and more comfortable.",
    },
    {
        icon: ShieldCheck,
        title: "Trust by design",
        description:
            "Clear information, thoughtful experiences, and responsible product presentation should make people feel confident at every step.",
    },
    {
        icon: Sparkles,
        title: "Simple by default",
        description:
            "We believe healthcare experiences should remove unnecessary complexity instead of adding more of it.",
    },
    {
        icon: Leaf,
        title: "Built for everyday life",
        description:
            "Better health is rarely about one dramatic decision. It is built through small, consistent choices that fit real life.",
    },
];

const experienceItems = [
    {
        icon: Sparkles,
        label: "Discover",
        title: "Find what fits your needs",
        description:
            "Explore healthcare essentials through an experience designed to keep discovery clear and focused.",
    },
    {
        icon: ShieldCheck,
        label: "Choose",
        title: "Make informed decisions",
        description:
            "Useful product information and a straightforward interface help you choose with greater confidence.",
    },
    {
        icon: Truck,
        label: "Receive",
        title: "A smoother journey",
        description:
            "From checkout to delivery, the experience should feel predictable, transparent, and effortless.",
    },
];

function FadeUp({
    children,
    delay = 0,
    className,
}: {
    children: React.ReactNode;
    delay?: number;
    className?: string;
}) {
    return (
        <motion.div
            initial={{ opacity: 0, y: 24 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{
                once: true,
                margin: "-80px",
            }}
            transition={{
                duration: 0.6,
                delay,
                ease: [0.22, 1, 0.36, 1],
            }}
            className={className}
        >
            {children}
        </motion.div>
    );
}

export function AboutPage() {
    const router = useRouter();

    return (
        <div className="overflow-hidden bg-background">
            {/* Hero */}
            <HeroSection />

            {/* Introduction */}
            <section className="border-y bg-muted/30">
                <div className="container mx-auto px-4 py-20 sm:py-24 lg:px-6 lg:py-28">
                    <div className="grid gap-12 lg:grid-cols-[0.8fr_1.2fr] lg:items-center lg:gap-20">
                        <FadeUp>
                            <div className="flex flex-col gap-3">
                                <p className="text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                                    Why Arovia
                                </p>

                                <h2 className="max-w-lg text-3xl font-bold tracking-normal text-foreground sm:text-4xl">
                                    Better experiences can make everyday
                                    healthcare feel less overwhelming.
                                </h2>
                            </div>
                        </FadeUp>

                        <FadeUp delay={0.08}>
                            <div className="flex flex-col gap-5 text-base text-muted-foreground sm:text-lg">
                                <p>
                                    Healthcare touches almost every part of
                                    life, yet the experience around it can
                                    often feel unnecessarily complicated.
                                </p>

                                <p>
                                    Arovia is designed to bring more clarity to
                                    the everyday side of healthcare — helping
                                    people discover products, understand their
                                    options, and complete routine purchases
                                    through a calmer digital experience.
                                </p>

                                <p>
                                    We are not trying to make healthcare louder.
                                    We are trying to make it easier to
                                    navigate.
                                </p>
                            </div>
                        </FadeUp>
                    </div>
                </div>
            </section>

            {/* Philosophy */}
            <section>
                <div className="container mx-auto px-4 py-20 sm:py-24 lg:px-6 lg:py-32">
                    <FadeUp className="mx-auto flex max-w-3xl flex-col items-center gap-3 text-center">
                        <p className="text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                            Our philosophy
                        </p>

                        <h2 className="text-3xl font-bold tracking-normal sm:text-4xl">
                            Thoughtful by design.
                            <br />
                            Simple by experience.
                        </h2>

                        <p className="mt-2 max-w-2xl text-base text-muted-foreground sm:text-lg">
                            We care about the details people notice — and the
                            unnecessary details they should never have to
                            notice.
                        </p>
                    </FadeUp>

                    <div className="mt-12 grid gap-5 sm:grid-cols-2 lg:mt-16 lg:grid-cols-4">
                        {principles.map((principle, index) => {
                            const Icon = principle.icon;

                            return (
                                <FadeUp
                                    key={principle.title}
                                    delay={index * 0.06}
                                >
                                    <Card className="h-full rounded-2xl border-border/70 bg-card/70 shadow-none transition-all duration-300 hover:-translate-y-1 hover:border-primary/20 hover:bg-card">
                                        <CardContent className="flex h-full flex-col p-6">
                                            <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                                <Icon className="size-5" />
                                            </div>

                                            <h3 className="mt-6 text-lg font-semibold tracking-normal">
                                                {principle.title}
                                            </h3>

                                            <p className="mt-3 text-sm text-muted-foreground">
                                                {principle.description}
                                            </p>
                                        </CardContent>
                                    </Card>
                                </FadeUp>
                            );
                        })}
                    </div>
                </div>
            </section>

            {/* What we're building */}
            <section className="bg-muted/30">
                <div className="container mx-auto px-4 py-20 sm:py-24 lg:px-6 lg:py-32">
                    <div className="grid gap-12 lg:grid-cols-[1fr_0.9fr] lg:items-center lg:gap-20">
                        <FadeUp>
                            <div className="flex flex-col gap-5">
                                <div className="flex flex-col gap-3">
                                    <p className="text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                                        What we&apos;re building
                                    </p>

                                    <h2 className="max-w-xl text-3xl font-bold tracking-normal sm:text-4xl">
                                        A healthcare experience that respects
                                        your time and attention.
                                    </h2>
                                </div>

                                <div className="flex max-w-xl flex-col gap-5 text-base text-muted-foreground sm:text-lg">
                                    <p>
                                        Arovia brings healthcare essentials
                                        together in one thoughtfully designed
                                        digital experience.
                                    </p>

                                    <p>
                                        The goal is not to overwhelm people
                                        with endless choices or unnecessary
                                        complexity. The goal is to help them
                                        get from need to decision with greater
                                        clarity.
                                    </p>

                                    <p>
                                        That means paying attention to the
                                        small things: useful organization,
                                        understandable information, a calm
                                        interface, and a purchase journey that
                                        stays out of the way.
                                    </p>
                                </div>
                            </div>
                        </FadeUp>

                        <FadeUp delay={0.1}>
                            <div className="relative">
                                <div
                                    aria-hidden="true"
                                    className="absolute -inset-6 rounded-[2rem] bg-primary/10 blur-3xl"
                                />

                                <Card className="relative overflow-hidden rounded-[2rem] border-primary/10 bg-card/90 shadow-sm backdrop-blur">
                                    <CardContent className="p-8 sm:p-10">
                                        <div className="flex items-center gap-4">
                                            <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-primary text-primary-foreground">
                                                <HeartPulse className="size-6" />
                                            </div>

                                            <div className="flex flex-col gap-1">
                                                <p className="text-sm font-medium text-muted-foreground">
                                                    The Arovia approach
                                                </p>

                                                <h3 className="text-xl font-semibold tracking-normal">
                                                    Less friction. More clarity.
                                                </h3>
                                            </div>
                                        </div>

                                        <div className="mt-8 flex flex-col gap-5">
                                            {[
                                                "Clear and thoughtful experiences",
                                                "Focused product discovery",
                                                "Simple, predictable interactions",
                                                "Designed around everyday needs",
                                            ].map((item, index) => (
                                                <div
                                                    key={item}
                                                    className="flex items-start gap-3"
                                                >
                                                    <div className="mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary">
                                                        <span className="text-xs font-bold">
                                                            {index + 1}
                                                        </span>
                                                    </div>

                                                    <p className="text-sm text-muted-foreground">
                                                        {item}
                                                    </p>
                                                </div>
                                            ))}
                                        </div>
                                    </CardContent>
                                </Card>
                            </div>
                        </FadeUp>
                    </div>
                </div>
            </section>

            {/* Experience */}
            <section>
                <div className="container mx-auto px-4 py-20 sm:py-24 lg:px-6 lg:py-32">
                    <FadeUp className="mx-auto flex max-w-3xl flex-col items-center gap-3 text-center">
                        <p className="text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                            The Arovia experience
                        </p>

                        <h2 className="text-3xl font-bold tracking-normal sm:text-4xl">
                            From discovery to delivery,
                            <br className="hidden sm:block" />
                            every step should feel natural.
                        </h2>
                    </FadeUp>

                    <div className="relative mx-auto mt-14 max-w-5xl">
                        <div className="absolute left-[10%] right-[10%] top-10 hidden h-px bg-border lg:block" />

                        <div className="grid gap-10 lg:grid-cols-3 lg:gap-8">
                            {experienceItems.map((item, index) => {
                                const Icon = item.icon;

                                return (
                                    <FadeUp
                                        key={item.label}
                                        delay={index * 0.08}
                                    >
                                        <div className="relative flex flex-col items-center gap-3 text-center">
                                            <div className="relative flex size-20 items-center justify-center rounded-full border border-primary/15 bg-background shadow-sm">
                                                <div className="flex size-12 items-center justify-center rounded-full bg-primary/10 text-primary">
                                                    <Icon className="size-5" />
                                                </div>
                                            </div>

                                            <p className="mt-4 text-xs font-semibold uppercase tracking-[0.14em] text-primary">
                                                {item.label}
                                            </p>

                                            <h3 className="text-xl font-semibold tracking-normal">
                                                {item.title}
                                            </h3>

                                            <p className="max-w-sm text-sm text-muted-foreground">
                                                {item.description}
                                            </p>
                                        </div>
                                    </FadeUp>
                                );
                            })}
                        </div>
                    </div>
                </div>
            </section>

            {/* Closing statement */}
            <section className="pb-20 sm:pb-24 lg:pb-32">
                <div className="container mx-auto px-4 lg:px-6">
                    <FadeUp>
                        <div
                            className={cn(
                                "relative overflow-hidden rounded-[2rem]",
                                "border border-primary/10",
                                "bg-primary/5",
                                "px-6 py-14 text-center",
                                "sm:px-10 sm:py-16",
                                "lg:px-16 lg:py-20",
                            )}
                        >
                            <div
                                aria-hidden="true"
                                className="absolute inset-0 bg-[radial-gradient(circle_at_top,rgba(16,185,129,0.12),transparent_55%)]"
                            />

                            <div className="relative mx-auto flex max-w-2xl flex-col items-center gap-5">
                                <p className="text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                                    Our direction
                                </p>

                                <h2 className="text-3xl font-bold tracking-normal sm:text-4xl">
                                    Your health.
                                    <br />
                                    Your care.
                                    <br />
                                    A simpler experience.
                                </h2>

                                <p className="max-w-xl text-base text-muted-foreground">
                                    We are building Arovia with the belief that
                                    good healthcare experiences should feel
                                    reassuring, useful, and uncomplicated.
                                </p>

                                <div className="mt-3 flex flex-col items-center gap-3 sm:flex-row">
                                    <Button
                                        type="button"
                                        className="group h-11 rounded-xl px-6"
                                        onClick={() =>
                                            router.push("/otc/medicine")
                                        }
                                    >
                                        Explore Arovia

                                        <ArrowRight className="ml-2 size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
                                    </Button>

                                    <Button
                                        type="button"
                                        variant="ghost"
                                        className="h-11 rounded-xl px-6"
                                        onClick={() =>
                                            router.push("/contact")
                                        }
                                    >
                                        Get in touch
                                    </Button>
                                </div>
                            </div>
                        </div>
                    </FadeUp>
                </div>
            </section>
        </div>
    );
}
