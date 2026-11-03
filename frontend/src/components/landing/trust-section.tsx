"use client";

import { motion } from "framer-motion";
import {
    BadgeCheck,
    LockKeyhole,
    PackageCheck,
    ShieldCheck,
} from "lucide-react";

const trustItems = [
    {
        icon: BadgeCheck,
        title: "Authentic products",
        description:
            "Products selected with quality and everyday healthcare needs in mind.",
    },
    {
        icon: LockKeyhole,
        title: "Secure experience",
        description:
            "A secure shopping experience designed to keep your information protected.",
    },
    {
        icon: PackageCheck,
        title: "Reliable delivery",
        description:
            "Convenient delivery designed to make healthcare shopping easier.",
    },
];

export function TrustSection() {
    return (
        <section className="border-y bg-muted/30 py-20 sm:py-24">
            <div className="container mx-auto px-4 lg:px-6">
                <div className="mx-auto max-w-3xl text-center">
                    <motion.div
                        initial={{ opacity: 0, y: 16 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true, margin: "-80px" }}
                        transition={{ duration: 0.5 }}
                    >
                        <div className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                            <ShieldCheck className="size-6" />
                        </div>

                        <p className="mt-6 text-sm font-semibold text-primary">
                            Care you can feel good about
                        </p>

                        <h2 className="mt-3 text-balance text-3xl font-semibold tracking-tight sm:text-4xl">
                            Healthcare you can feel good about.
                        </h2>

                        <p className="mx-auto mt-4 max-w-2xl text-base leading-7 text-muted-foreground">
                            Authentic products. Secure checkout. Reliable
                            delivery. A simpler healthcare experience.
                        </p>
                    </motion.div>

                    <div className="mt-12 grid gap-4 md:grid-cols-3">
                        {trustItems.map((item, index) => {
                            const Icon = item.icon;

                            return (
                                <motion.div
                                    key={item.title}
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
                                        duration: 0.4,
                                        delay: index * 0.07,
                                    }}
                                    className="rounded-2xl border bg-background p-6 text-left"
                                >
                                    <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
                                        <Icon className="size-5" />
                                    </div>

                                    <h3 className="mt-5 text-sm font-semibold">
                                        {item.title}
                                    </h3>

                                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                        {item.description}
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