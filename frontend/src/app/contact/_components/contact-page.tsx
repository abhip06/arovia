"use client";

import Link from "next/link";

import { motion } from "framer-motion";
import {
    Clock3,
    Mail,
    MapPin,
    MessageCircle,
    Phone,
} from "lucide-react";

import { BackgroundBeams } from "@/components/ui/background-beams";
import { Card, CardContent } from "@/components/ui/card";
import { ContactForm } from "./contact-form";

const contactDetails = [
    {
        icon: Mail,
        title: "Email us",
        description:
            "For general questions, support, and order assistance.",
        value: "support@arovia.in",
        href: "mailto:support@arovia.in",
    },
    {
        icon: Phone,
        title: "Call us",
        description:
            "Speak with our support team during business hours.",
        value: "+91 1800 123 4567",
        href: "tel:+9118001234567",
    },
    {
        icon: Clock3,
        title: "Support hours",
        description:
            "Our support team is available during the following hours.",
        value: "Mon – Sat · 9:00 AM – 6:00 PM",
    },
    {
        icon: MapPin,
        title: "Our location",
        description:
            "Our registered office and business location.",
        value: "Bhilai, Chhattisgarh, IN",
    },
];

const helpTopics = [
    "Order and delivery support",
    "Product questions",
    "Account and login issues",
    "Returns and refunds",
    "Payments",
    "General feedback",
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
            viewport={{ once: true, margin: "-80px" }}
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

export function ContactPage() {
    return (
        <main className="overflow-hidden bg-background pt-12">
            {/* Hero */}
            <section className="relative isolate">
                <div className="container relative mx-auto px-4 pb-16 pt-16 sm:pb-20 sm:pt-20 lg:px-6 lg:pb-24 lg:pt-28">
                    <BackgroundBeams />

                    <div className="relative mx-auto max-w-3xl text-center">
                        <FadeUp>
                            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-primary/15 bg-primary/5 px-4 py-2 text-sm font-medium text-primary">
                                <MessageCircle className="size-4" />
                                We&apos;re here to help
                            </div>
                        </FadeUp>

                        <FadeUp delay={0.08}>
                            <h1 className="text-balance text-4xl font-bold tracking-normal text-foreground sm:text-5xl lg:text-6xl">
                                Let&apos;s talk.
                            </h1>
                        </FadeUp>

                        <FadeUp delay={0.16}>
                            <p className="mx-auto mt-6 max-w-2xl text-pretty text-base leading-7 text-muted-foreground sm:text-lg">
                                Have a question, need help with an order, or
                                just want to share some feedback? We&apos;d
                                love to hear from you.
                            </p>
                        </FadeUp>
                    </div>
                </div>
            </section>

            {/* Contact information */}
            <section className="border-y bg-muted/30">
                <div className="container mx-auto px-4 py-16 sm:py-20 lg:px-6 lg:py-24">
                    <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
                        {contactDetails.map((item, index) => {
                            const Icon = item.icon;

                            return (
                                <FadeUp
                                    key={item.title}
                                    delay={index * 0.06}
                                >
                                    <Card className="h-full rounded-2xl border-border/70 bg-card/70 shadow-none transition-all duration-300 hover:-translate-y-1 hover:border-primary/20 hover:bg-card">
                                        <CardContent className="p-6">
                                            <div className="flex size-11 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                                <Icon className="size-5" />
                                            </div>

                                            <h2 className="mt-5 text-base font-semibold">
                                                {item.title}
                                            </h2>

                                            <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                                {item.description}
                                            </p>

                                            {item.href ? (
                                                <Link
                                                    href={item.href}
                                                    className="mt-4 inline-flex text-sm font-semibold text-primary underline-offset-4 hover:underline"
                                                >
                                                    {item.value}
                                                </Link>
                                            ) : (
                                                <p className="mt-4 text-sm font-semibold text-foreground">
                                                    {item.value}
                                                </p>
                                            )}
                                        </CardContent>
                                    </Card>
                                </FadeUp>
                            );
                        })}
                    </div>
                </div>
            </section>

            {/* Main contact area */}
            <section>
                <div className="container mx-auto px-4 py-20 sm:py-24 lg:px-6 lg:py-32">
                    <div className="grid gap-12 lg:grid-cols-[0.75fr_1.25fr] lg:gap-20">
                        {/* Left information */}
                        <FadeUp>
                            <div className="lg:sticky lg:top-28">
                                <p className="text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                                    Contact support
                                </p>

                                <h2 className="mt-3 text-3xl font-bold tracking-normal sm:text-4xl">
                                    How can we help?
                                </h2>

                                <p className="mt-5 text-base leading-7 text-muted-foreground">
                                    Tell us what&apos;s on your mind. Whether
                                    it&apos;s a question about an order or a
                                    suggestion about Arovia, our team is here
                                    to help.
                                </p>

                                <div className="mt-8 space-y-3">
                                    {helpTopics.map((topic) => (
                                        <div
                                            key={topic}
                                            className="flex items-center gap-3"
                                        >
                                            <div className="flex size-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary">
                                                <span className="size-1.5 rounded-full bg-primary" />
                                            </div>

                                            <span className="text-sm text-muted-foreground">
                                                {topic}
                                            </span>
                                        </div>
                                    ))}
                                </div>

                                <div className="mt-10 rounded-2xl border border-primary/10 bg-primary/5 p-5">
                                    <p className="text-sm font-medium text-foreground">
                                        Looking for something specific?
                                    </p>

                                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                        For privacy-related requests, please
                                        use our dedicated{" "}
                                        <Link
                                            href="/privacy-policy"
                                            className="font-medium text-primary underline-offset-4 hover:underline"
                                        >
                                            Privacy Policy
                                        </Link>{" "}
                                        contact information.
                                    </p>
                                </div>
                            </div>
                        </FadeUp>

                        {/* Form */}
                        <FadeUp delay={0.1}>
                            <Card className="rounded-3xl border-border/70 bg-card shadow-sm">
                                <CardContent className="p-6 sm:p-8 lg:p-10">
                                    <div className="mb-8">
                                        <h2 className="text-2xl font-semibold tracking-normal">
                                            Send us a message
                                        </h2>

                                        <p className="mt-2 text-sm leading-6 text-muted-foreground">
                                            Fill out the form and our team will
                                            get back to you.
                                        </p>
                                    </div>

                                    <ContactForm />
                                </CardContent>
                            </Card>
                        </FadeUp>
                    </div>
                </div>
            </section>

            {/* Reassurance */}
            <section className="pb-20 sm:pb-24 lg:pb-32">
                <div className="container mx-auto px-4 lg:px-6">
                    <FadeUp>
                        <div className="mx-auto max-w-4xl rounded-[2rem] border border-primary/10 bg-primary/5 px-6 py-12 text-center sm:px-10 sm:py-14">
                            <div className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                <HeartIcon />
                            </div>

                            <h2 className="mt-5 text-2xl font-semibold sm:text-3xl">
                                Your experience matters to us.
                            </h2>

                            <p className="mx-auto mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                                We&apos;re building Arovia to make everyday
                                healthcare simpler. Your questions, feedback,
                                and suggestions help us make it better.
                            </p>
                        </div>
                    </FadeUp>
                </div>
            </section>
        </main>
    );
}

function HeartIcon() {
    return (
        <svg
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            className="size-6"
            aria-hidden="true"
        >
            <path
                d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78L12 21.23l8.84-8.84a5.5 5.5 0 0 0 0-7.78Z"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeLinejoin="round"
            />
        </svg>
    );
}
