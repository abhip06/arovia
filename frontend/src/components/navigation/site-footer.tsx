"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import {
    ArrowUpRight,
    Mail,
    MapPin,
    ShieldCheck,
} from "lucide-react";

import {
    FaInstagram,
    FaLinkedinIn,
    FaXTwitter,
    FaGithub,
} from "react-icons/fa6";

import { Logo } from "../brand/logo";
import { ThemeToggle } from "./theme-toggle";

const footerGroups = [
    {
        title: "Shop",
        links: [
            { label: "Medicines", href: "/otc/medicine" },
            { label: "Wellness", href: "/otc/wellness" },
            { label: "Personal Care", href: "/otc/personal-care" },
            { label: "Healthcare Devices", href: "/otc/medical-device" },
        ],
    },
    {
        title: "Help",
        links: [
            { label: "Contact Us", href: "/contact" },
            { label: "FAQs", href: "/faqs" },
            { label: "Shipping & Delivery", href: "/shipping" },
            { label: "Returns & Refunds", href: "/returns" },
        ],
    },
    {
        title: "Company",
        links: [
            { label: "About Arovia", href: "/about" },
            { label: "Our Approach", href: "/about/approach" },
            { label: "Careers", href: "/careers" },
            { label: "Blog", href: "/blog" },
        ],
    },
];

const socialLinks = [
    {
        label: "Instagram",
        href: "https://instagram.com/abhi.p9",
        icon: FaInstagram,
    },
    {
        label: "Github",
        href: "https://github.com/abhip06",
        icon: FaGithub,
    },
    {
        label: "LinkedIn",
        href: "https://linkedin.com/in/abhinav-patel-38480b254",
        icon: FaLinkedinIn,
    },
    {
        label: "X",
        href: "https://x.com/abhinavsde",
        icon: FaXTwitter,
    },
];

export function SiteFooter() {
    return (
        <footer className="border-t bg-muted/30">
            <div className="container mx-auto px-4 lg:px-6">
                {/* Main footer */}
                <div className="grid gap-12 py-14 sm:py-16 lg:grid-cols-[1.5fr_repeat(3,1fr)] lg:gap-10 lg:py-20">
                    {/* Brand */}
                    <motion.div
                        initial={{
                            opacity: 0,
                            y: 14,
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
                            duration: 0.5,
                        }}
                        className="max-w-sm"
                    >
                        <Logo
                            contentType="FOOTER"
                            textClassName="text-4xl md:text-5xl"
                            imageSize={64}
                        />

                        <p className="mt-5 text-sm leading-6 text-muted-foreground">
                            A simpler way to discover medicines, wellness
                            essentials, personal care products, and healthcare
                            devices for everyday life.
                        </p>

                        <div className="mt-6 flex items-center gap-2 text-xs text-muted-foreground">
                            <ShieldCheck className="size-4 text-primary" />

                            <span>
                                Thoughtfully brought together for everyday
                                care.
                            </span>
                        </div>

                        <div className="mt-7 flex items-center gap-2">
                            {socialLinks.map((social) => {
                                const Icon = social.icon;

                                return (
                                    <Link
                                        key={social.label}
                                        href={social.href}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        aria-label={social.label}
                                        className="flex size-9 items-center justify-center rounded-lg border bg-background text-muted-foreground transition-colors hover:border-primary/20 hover:bg-primary/5 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                                    >
                                        <Icon className="size-4" />
                                    </Link>
                                );
                            })}
                        </div>
                    </motion.div>

                    {/* Link groups */}
                    {footerGroups.map((group, groupIndex) => (
                        <motion.div
                            key={group.title}
                            initial={{
                                opacity: 0,
                                y: 14,
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
                                delay: 0.06 * (groupIndex + 1),
                            }}
                        >
                            <h2 className="text-sm font-semibold tracking-tight">
                                {group.title}
                            </h2>

                            <nav
                                aria-label={`${group.title} links`}
                                className="mt-5 flex flex-col gap-3"
                            >
                                {group.links.map((link) => (
                                    <Link
                                        key={link.href}
                                        href={link.href}
                                        className="group inline-flex w-fit items-center gap-1.5 text-sm leading-6 text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                                    >
                                        <span>{link.label}</span>

                                        <ArrowUpRight className="size-3.5 opacity-0 transition-all duration-200 group-hover:-translate-y-0.5 group-hover:translate-x-0.5 group-hover:opacity-100" />
                                    </Link>
                                ))}
                            </nav>
                        </motion.div>
                    ))}
                </div>

                {/* Contact strip */}
                <div className="border-t py-6">
                    <div className="flex flex-col gap-4 text-sm text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
                            <a
                                href="mailto:support@arovia.in"
                                className="inline-flex items-center gap-2 transition-colors hover:text-foreground"
                            >
                                <Mail className="size-4 text-primary" />
                                support@arovia.in
                            </a>

                            <div className="inline-flex items-center gap-2">
                                <MapPin className="size-4 text-primary" />

                                <span>
                                    Serving healthcare, from Bhilai,
                                    Chhattisgarh.
                                </span>
                            </div>
                        </div>

                        <Link
                            href="/contact"
                            className="inline-flex items-center gap-2 font-medium text-foreground transition-colors hover:text-primary"
                        >
                            Get in touch
                            <ArrowUpRight className="size-4" />
                        </Link>
                    </div>
                </div>

                {/* Bottom bar */}
                <div className="border-t py-6">
                    <div className="flex flex-col gap-4 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
                        <p>
                            © {new Date().getFullYear()} Arovia. All rights
                            reserved.
                        </p>

                        <div className="flex flex-wrap items-center gap-5">
                            <nav
                                aria-label="Legal navigation"
                                className="flex flex-wrap gap-x-5 gap-y-2"
                            >
                                <Link
                                    href="/privacy-policy"
                                    className="transition-colors hover:text-foreground"
                                >
                                    Privacy Policy
                                </Link>

                                <Link
                                    href="/terms-of-service"
                                    className="transition-colors hover:text-foreground"
                                >
                                    Terms of Service
                                </Link>

                                <Link
                                    href="/cookie-policy"
                                    className="transition-colors hover:text-foreground"
                                >
                                    Cookie Policy
                                </Link>
                            </nav>

                            {/* Desktop only */}
                            <div className="hidden items-center border-l border-border pl-5 lg:flex">
                                <ThemeToggle />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </footer>
    );
}
