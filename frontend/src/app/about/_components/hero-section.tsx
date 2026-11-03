"use client";

import { motion } from "framer-motion";
import {
    ArrowRight,
    Check,
    FlaskConical,
    HeartPulse,
    Pill,
    ShieldCheck,
    Sparkles,
    Truck,
} from "lucide-react";

import { useRouter } from "next/navigation";

import { BackgroundBeams } from "@/components/ui/background-beams";
import { Button } from "@/components/ui/button";

const services = [
    {
        icon: Pill,
        label: "Medicines",
        description: "Everyday essentials",
    },
    {
        icon: FlaskConical,
        label: "Lab tests",
        description: "Know your health",
    },
    {
        icon: Truck,
        label: "Delivery",
        description: "Care at your doorstep",
    },
];

export function HeroSection() {
    const router = useRouter();

    return (
        <section className="relative isolate min-h-[calc(100dvh-4rem)] overflow-hidden bg-background pt-5">
            {/* Background */}
            <div
                aria-hidden="true"
                className="absolute inset-0"
            >
                <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,color-mix(in_oklab,var(--primary)_12%,transparent),transparent_34%),radial-gradient(circle_at_80%_30%,color-mix(in_oklab,var(--primary)_7%,transparent),transparent_30%),radial-gradient(circle_at_70%_85%,color-mix(in_oklab,#3b82f6_7%,transparent),transparent_30%)]" />

                <div className="absolute left-1/2 top-0 h-[28rem] w-[44rem] -translate-x-1/2 rounded-full bg-primary/5 blur-[120px]" />

                <div className="absolute -bottom-32 -left-32 size-96 rounded-full bg-primary/5 blur-[110px]" />

                <div className="absolute -right-32 top-1/3 size-96 rounded-full bg-blue-500/5 blur-[110px]" />
            </div>

            <BackgroundBeams className="opacity-25 dark:opacity-15" />

            {/* Main */}
            <div className="relative z-10 flex min-h-[calc(100dvh-4rem)] items-center">
                <div className="container mx-auto px-4 py-16 sm:px-6 sm:py-20 lg:px-8 lg:py-24">
                    <div className="grid items-center gap-14 lg:grid-cols-[1.05fr_0.95fr] lg:gap-16 xl:gap-24">
                        {/* Copy */}
                        <motion.div
                            initial={{
                                opacity: 0,
                                y: 24,
                            }}
                            animate={{
                                opacity: 1,
                                y: 0,
                            }}
                            transition={{
                                duration: 0.7,
                                ease: [0.22, 1, 0.36, 1],
                            }}
                            className="max-w-2xl"
                        >
                            <div className="flex flex-col gap-6">
                                {/* Eyebrow */}
                                <motion.div
                                    initial={{
                                        opacity: 0,
                                        y: 8,
                                    }}
                                    animate={{
                                        opacity: 1,
                                        y: 0,
                                    }}
                                    transition={{
                                        duration: 0.5,
                                        delay: 0.05,
                                    }}
                                    className="flex w-fit items-center gap-2 rounded-full border border-primary/15 bg-primary/5 px-4 py-2 text-xs font-medium text-primary backdrop-blur-sm sm:text-sm"
                                >
                                    <Sparkles className="size-3.5" />

                                    <span>
                                        Better Health. Made Simple.
                                    </span>
                                </motion.div>

                                {/* Heading */}
                                <div className="flex flex-col gap-5">
                                    <h1 className="text-balance text-4xl font-semibold tracking-normal text-foreground sm:text-5xl lg:text-6xl xl:text-7xl">
                                        Better health.
                                        <br />
                                        Made{" "}
                                        <span className="text-primary">
                                            simpler.
                                        </span>
                                    </h1>

                                    <p className="max-w-xl text-pretty text-base text-muted-foreground sm:text-lg">
                                        A calmer way to discover medicines,
                                        wellness essentials, lab tests, and
                                        everyday healthcare services for you
                                        and your family.
                                    </p>
                                </div>

                                {/* Actions */}
                                <div className="flex flex-col gap-3 sm:flex-row">
                                    <Button
                                        type="button"
                                        size="lg"
                                        onClick={() =>
                                            router.push("/otc/medicine")
                                        }
                                        className="group h-12 rounded-xl px-6"
                                    >
                                        Explore Arovia

                                        <ArrowRight className="ml-2 size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
                                    </Button>

                                    <Button
                                        type="button"
                                        size="lg"
                                        variant="outline"
                                        onClick={() =>
                                            router.push("/about")
                                        }
                                        className="h-12 rounded-xl px-6"
                                    >
                                        About Arovia
                                    </Button>
                                </div>

                                {/* Trust signals */}
                                <div className="flex flex-wrap gap-x-7 gap-y-3 pt-2">
                                    <TrustItem text="Authentic products" />

                                    <TrustItem text="Secure experience" />

                                    <TrustItem text="Designed for everyday care" />
                                </div>
                            </div>
                        </motion.div>

                        {/* Visual */}
                        <motion.div
                            initial={{
                                opacity: 0,
                                scale: 0.96,
                            }}
                            animate={{
                                opacity: 1,
                                scale: 1,
                            }}
                            transition={{
                                duration: 0.8,
                                delay: 0.12,
                                ease: [0.22, 1, 0.36, 1],
                            }}
                            className="relative mx-auto w-full max-w-xl"
                        >
                            {/* Glow */}
                            <div
                                aria-hidden="true"
                                className="absolute -inset-10 rounded-[3rem] bg-primary/10 blur-3xl"
                            />

                            {/* Main visual */}
                            <div className="relative overflow-hidden rounded-[2.5rem] border border-border/70 bg-card/75 shadow-2xl shadow-primary/10 backdrop-blur-xl">
                                {/* Soft top gradient */}
                                <div
                                    aria-hidden="true"
                                    className="absolute inset-x-0 top-0 h-40 bg-gradient-to-b from-primary/10 to-transparent"
                                />

                                <div className="relative p-5 sm:p-7">
                                    {/* Header */}
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-3">
                                            <div className="flex size-11 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                                <HeartPulse className="size-5" />
                                            </div>

                                            <div className="flex flex-col gap-0.5">
                                                <p className="text-sm font-semibold">
                                                    Your everyday care
                                                </p>

                                                <p className="text-xs text-muted-foreground">
                                                    Everything in one place
                                                </p>
                                            </div>
                                        </div>

                                        <div className="flex size-9 items-center justify-center rounded-xl border border-border/70 bg-background/70">
                                            <ShieldCheck className="size-4 text-primary" />
                                        </div>
                                    </div>

                                    {/* Main feature */}
                                    <motion.div
                                        animate={{
                                            y: [0, -5, 0],
                                        }}
                                        transition={{
                                            duration: 6,
                                            repeat: Infinity,
                                            ease: "easeInOut",
                                        }}
                                        className="mt-6 rounded-3xl border border-primary/10 bg-gradient-to-br from-primary/10 via-background to-secondary/30 p-5 sm:p-6"
                                    >
                                        <div className="flex items-end justify-between gap-5">
                                            <div className="flex flex-col gap-2">
                                                <span className="text-xs font-medium uppercase tracking-[0.12em] text-primary">
                                                    A simpler routine
                                                </span>

                                                <p className="text-xl font-semibold tracking-normal sm:text-2xl">
                                                    Care that fits your day.
                                                </p>

                                                <p className="max-w-xs text-xs text-muted-foreground sm:text-sm">
                                                    Discover what you need
                                                    without unnecessary
                                                    complexity.
                                                </p>
                                            </div>

                                            <motion.div
                                                animate={{
                                                    rotate: [0, 2, 0, -2, 0],
                                                }}
                                                transition={{
                                                    duration: 7,
                                                    repeat: Infinity,
                                                    ease: "easeInOut",
                                                }}
                                                className="hidden size-20 shrink-0 items-center justify-center rounded-3xl bg-primary text-primary-foreground shadow-lg shadow-primary/20 sm:flex"
                                            >
                                                <HeartPulse className="size-9" />
                                            </motion.div>
                                        </div>
                                    </motion.div>

                                    {/* Services */}
                                    <div className="mt-5 grid gap-3">
                                        {services.map(
                                            (service, index) => {
                                                const Icon =
                                                    service.icon;

                                                return (
                                                    <motion.div
                                                        key={
                                                            service.label
                                                        }
                                                        initial={{
                                                            opacity: 0,
                                                            x: 12,
                                                        }}
                                                        animate={{
                                                            opacity: 1,
                                                            x: 0,
                                                        }}
                                                        transition={{
                                                            duration: 0.45,
                                                            delay:
                                                                0.3 +
                                                                index *
                                                                    0.08,
                                                        }}
                                                        className="flex items-center gap-4 rounded-2xl border border-border/70 bg-background/70 p-4 backdrop-blur-sm transition-colors duration-300 hover:border-primary/20 hover:bg-background"
                                                    >
                                                        <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                                            <Icon className="size-4" />
                                                        </div>

                                                        <div className="flex min-w-0 flex-1 flex-col gap-0.5">
                                                            <p className="text-sm font-semibold">
                                                                {
                                                                    service.label
                                                                }
                                                            </p>

                                                            <p className="truncate text-xs text-muted-foreground">
                                                                {
                                                                    service.description
                                                                }
                                                            </p>
                                                        </div>

                                                        <ArrowRight className="size-4 shrink-0 text-muted-foreground/60" />
                                                    </motion.div>
                                                );
                                            },
                                        )}
                                    </div>

                                    {/* Bottom note */}
                                    <div className="mt-5 flex items-center justify-between gap-4 rounded-2xl border border-border/70 bg-muted/40 px-4 py-3">
                                        <div className="flex items-center gap-2">
                                            <div className="flex size-7 items-center justify-center rounded-full bg-primary/10 text-primary">
                                                <Check className="size-3.5" />
                                            </div>

                                            <span className="text-xs font-medium text-muted-foreground">
                                                Built around everyday
                                                healthcare
                                            </span>
                                        </div>

                                        <Sparkles className="size-4 shrink-0 text-primary/70" />
                                    </div>
                                </div>
                            </div>

                            {/* Floating detail */}
                            <motion.div
                                initial={{
                                    opacity: 0,
                                    y: 15,
                                }}
                                animate={{
                                    opacity: 1,
                                    y: 0,
                                }}
                                transition={{
                                    duration: 0.5,
                                    delay: 0.65,
                                }}
                                className="absolute -bottom-16 -left-4 hidden rounded-2xl border border-border/70 bg-card/90 p-4 shadow-xl backdrop-blur-xl sm:block lg:-left-8"
                            >
                                <div className="flex items-center gap-3">
                                    <div className="flex size-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                        <ShieldCheck className="size-4" />
                                    </div>

                                    <div className="flex flex-col gap-0.5">
                                        <p className="text-xs font-semibold">
                                            Trust built in
                                        </p>

                                        <p className="text-[11px] text-muted-foreground">
                                            Simple. Secure. Thoughtful.
                                        </p>
                                    </div>
                                </div>
                            </motion.div>

                            {/* Floating delivery detail */}
                            <motion.div
                                initial={{
                                    opacity: 0,
                                    y: -15,
                                }}
                                animate={{
                                    opacity: 1,
                                    y: 0,
                                }}
                                transition={{
                                    duration: 0.5,
                                    delay: 0.8,
                                }}
                                className="absolute -right-3 -top-5 hidden rounded-2xl border border-border/70 bg-card/90 p-4 shadow-xl backdrop-blur-xl sm:block lg:-right-7"
                            >
                                <div className="flex items-center gap-3">
                                    <div className="flex size-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                        <Truck className="size-4" />
                                    </div>

                                    <div className="flex flex-col gap-0.5">
                                        <p className="text-xs font-semibold">
                                            Care delivered
                                        </p>

                                        <p className="text-[11px] text-muted-foreground">
                                            Right to your doorstep
                                        </p>
                                    </div>
                                </div>
                            </motion.div>
                        </motion.div>
                    </div>
                </div>
            </div>

            {/* Bottom cue */}
            <motion.div
                initial={{
                    opacity: 0,
                }}
                animate={{
                    opacity: 1,
                }}
                transition={{
                    delay: 1,
                    duration: 0.6,
                }}
                className="absolute bottom-24 left-1/2 hidden -translate-x-1/2 items-center gap-2 text-xs text-muted-foreground/70 lg:flex"
            >
                <span>Scroll to explore</span>

                <ArrowRight className="size-3.5 rotate-90" />
            </motion.div>
        </section>
    );
}

function TrustItem({
    text,
}: {
    text: string;
}) {
    return (
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Check className="size-4 shrink-0 text-primary" />

            <span>{text}</span>
        </div>
    );
}
