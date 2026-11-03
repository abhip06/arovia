"use client";

import Image from "next/image";
import Link from "next/link";
import { motion } from "framer-motion";
import {
    ArrowUpRight,
    HeartPulse,
    Microscope,
    PackageCheck,
    Pill,
    Sparkles,
    Stethoscope,
} from "lucide-react";

const categories = [
    {
        title: "Medicines",
        description: "Everyday treatments and health essentials.",
        href: "/otc/medicine",
        image: "https://images.unsplash.com/photo-1585435557343-3b092031a831?auto=format&fit=crop&w=1200&q=85",
        icon: Pill,
        eyebrow: "Everyday health",
    },
    {
        title: "Medical Devices",
        description: "Reliable tools for care at home.",
        href: "/otc/medical-device",
        image: "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?auto=format&fit=crop&w=1200&q=85",
        icon: Stethoscope,
        eyebrow: "Care at home",
    },
    {
        title: "Diagnostics",
        description: "Essentials that help you understand your health.",
        href: "/otc/diagnostic",
        image: "https://images.unsplash.com/photo-1579684385127-1ef15d508118?auto=format&fit=crop&w=1200&q=85",
        icon: Microscope,
        eyebrow: "Know your health",
    },
    {
        title: "Wellness",
        description: "Support for the routines that keep you feeling well.",
        href: "/otc/wellness",
        image: "https://images.unsplash.com/photo-1610740654950-070baeb919ee?auto=format&fit=crop&w=1200&q=85",
        icon: HeartPulse,
        eyebrow: "Feel your best",
    },
    {
        title: "Personal Care",
        description: "Thoughtful picks for your everyday routine.",
        href: "/otc/personal-care",
        image: "https://images.unsplash.com/photo-1629380108599-ea06489d66f5?auto=format&fit=crop&w=1200&q=85",
        icon: Sparkles,
        eyebrow: "Daily rituals",
    },
    {
        title: "Healthcare Accessories",
        description: "Useful details that make care a little easier.",
        href: "/otc/healthcare-accessory",
        image: "https://images.unsplash.com/photo-1603398938378-e54eab446dde?auto=format&fit=crop&w=1200&q=85",
        icon: PackageCheck,
        eyebrow: "Little helpers",
    },
];

export function CategorySection() {
    return (
        <section className="py-16 sm:py-20 lg:py-24">
            <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                <motion.div
                    initial={{ opacity: 0, y: 14 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true, margin: "-70px" }}
                    transition={{ duration: 0.45 }}
                    className="mx-auto max-w-2xl text-center"
                >
                    <p className="text-sm font-semibold text-primary">Explore Arovia</p>
                    <h2 className="mt-3 text-balance text-3xl font-semibold tracking-tight sm:text-4xl">
                        Everyday care, all in one place.
                    </h2>
                    <p className="mt-4 text-base leading-7 text-muted-foreground">
                        Explore the products and care essentials that fit your life.
                    </p>
                </motion.div>

                <div className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {categories.map((category, index) => {
                        const Icon = category.icon;

                        return (
                            <motion.div
                                key={category.href}
                                initial={{ opacity: 0, y: 16 }}
                                whileInView={{ opacity: 1, y: 0 }}
                                viewport={{ once: true, margin: "-50px" }}
                                transition={{ duration: 0.4, delay: index * 0.045 }}
                            >
                                <Link
                                    href={category.href}
                                    className="group relative isolate flex min-h-56 overflow-hidden rounded-3xl border border-border/60 bg-muted shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-primary/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary sm:min-h-64"
                                >
                                    <Image
                                        src={category.image}
                                        alt=""
                                        fill
                                        sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                                        className="-z-10 object-cover transition-transform duration-700 group-hover:scale-[1.04]"
                                    />
                                    <div aria-hidden="true" className="absolute inset-0 -z-0 bg-gradient-to-t from-slate-950/90 via-slate-950/40 to-slate-950/5" />
                                    <div className="relative z-10 mt-auto flex w-full items-end justify-between gap-4 p-5 text-white sm:p-6">
                                        <div className="min-w-0">
                                            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full border border-white/20 bg-white/10 px-2.5 py-1 text-[10px] font-semibold uppercase tracking-[0.12em] text-white/90 backdrop-blur-sm">
                                                <Icon className="size-3.5" /> {category.eyebrow}
                                            </div>
                                            <h3 className="text-xl font-semibold tracking-tight sm:text-2xl">{category.title}</h3>
                                            <p className="mt-1.5 max-w-sm text-sm leading-5 text-white/75">{category.description}</p>
                                        </div>
                                        <span aria-hidden="true" className="flex size-10 shrink-0 items-center justify-center rounded-full border border-white/25 bg-white/10 backdrop-blur-sm transition-all group-hover:border-white/50 group-hover:bg-white/20">
                                            <ArrowUpRight className="size-5" />
                                        </span>
                                    </div>
                                </Link>
                            </motion.div>
                        );
                    })}
                </div>
            </div>
        </section>
    );
}
