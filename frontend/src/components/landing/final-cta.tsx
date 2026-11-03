"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import { ArrowRight, HeartPulse } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function FinalCta() {
    return (
        <section className="relative overflow-hidden py-20 sm:py-24 lg:py-28">
            <div className="container mx-auto px-4 lg:px-6">
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true, margin: "-80px" }}
                    transition={{ duration: 0.6 }}
                    className="relative overflow-hidden rounded-[2rem] border bg-foreground px-6 py-14 text-background shadow-2xl sm:px-10 sm:py-16 lg:px-16"
                >
                    <div className="absolute -right-20 -top-20 size-64 rounded-full bg-primary/20 blur-3xl" />

                    <div className="absolute -bottom-24 -left-20 size-72 rounded-full bg-primary/10 blur-3xl" />

                    <div className="relative mx-auto max-w-3xl text-center">
                        <div className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-background/10">
                            <HeartPulse className="size-6" />
                        </div>

                        <h2 className="mt-6 text-balance text-3xl font-semibold tracking-tight sm:text-4xl lg:text-5xl">
                            Take a simpler step toward better health.
                        </h2>

                        <p className="mx-auto mt-4 max-w-2xl text-base leading-7 text-background/70 sm:text-lg">
                            Discover medicines, wellness essentials and
                            healthcare products brought together for everyday
                            care.
                        </p>

                        <div className="mt-8">
                            <Link
                                href="/otc/medicine"
                                className={cn(
                                    buttonVariants({
                                        size: "lg",
                                    }),
                                    "h-12 rounded-xl bg-background px-6 text-foreground shadow-lg hover:bg-background/90",
                                )}
                            >
                                Explore Arovia
                                <ArrowRight
                                    data-icon="inline-end"
                                    className="size-4"
                                />
                            </Link>
                        </div>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
