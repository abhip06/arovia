"use client";

import { motion } from "framer-motion";
import {
    Clock3,
    HeartHandshake,
    ShieldCheck,
    Truck,
} from "lucide-react";

const benefits = [
    {
        icon: ShieldCheck,
        title: "Thoughtfully selected",
        description:
            "A focused collection of healthcare products designed around everyday needs.",
    },
    {
        icon: HeartHandshake,
        title: "Care made easier",
        description:
            "A simple shopping experience that keeps finding what you need straightforward.",
    },
    {
        icon: Truck,
        title: "Convenient delivery",
        description:
            "Get your essentials delivered without making healthcare shopping complicated.",
    },
    {
        icon: Clock3,
        title: "Built for everyday life",
        description:
            "From medicines to wellness, discover products that fit naturally into your routine.",
    },
];

export function BenefitsSection() {
    return (
        <section className="border-y bg-muted/30 py-20 sm:py-24">
            <div className="container mx-auto px-4 lg:px-6">
                <div className="grid gap-10 lg:grid-cols-[0.8fr_1.2fr] lg:items-center">
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true, margin: "-80px" }}
                        transition={{ duration: 0.5 }}
                        className="max-w-lg"
                    >
                        <p className="text-sm font-semibold text-primary">
                            Why Arovia
                        </p>

                        <h2 className="mt-3 text-balance text-3xl font-semibold tracking-tight sm:text-4xl">
                            Health, without the hassle.
                        </h2>

                        <p className="mt-4 text-base leading-7 text-muted-foreground">
                            Healthcare shopping should feel clear, dependable
                            and easy to navigate. Arovia brings everyday
                            healthcare essentials together with a simpler
                            experience.
                        </p>
                    </motion.div>

                    <div className="grid gap-4 sm:grid-cols-2">
                        {benefits.map((benefit, index) => {
                            const Icon = benefit.icon;

                            return (
                                <motion.div
                                    key={benefit.title}
                                    initial={{
                                        opacity: 0,
                                        y: 16,
                                    }}
                                    whileInView={{
                                        opacity: 1,
                                        y: 0,
                                    }}
                                    viewport={{
                                        once: true,
                                        margin: "-60px",
                                    }}
                                    transition={{
                                        duration: 0.45,
                                        delay: index * 0.06,
                                    }}
                                    className="rounded-2xl border bg-background p-6"
                                >
                                    <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
                                        <Icon className="size-5" />
                                    </div>

                                    <h3 className="mt-5 text-sm font-semibold">
                                        {benefit.title}
                                    </h3>

                                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                        {benefit.description}
                                    </p>
                                </motion.div>
                            );
                        })}
                    </div>
                </div>
            </div>
        </section>
    );
}