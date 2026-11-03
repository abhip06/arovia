"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import { usePathname } from "next/navigation";

import { cn } from "@/lib/utils";
import { navigationItems } from "./navigation-items";

interface DesktopNavProps {
    appearance?: "default" | "overlay";
}

export function DesktopNav({
    appearance = "default",
}: DesktopNavProps) {
    const pathname = usePathname();
    const isOverlay = appearance === "overlay";

    return (
        <nav
            aria-label="Primary navigation"
            className="flex items-center gap-1"
        >
            {navigationItems.map((item) => {
                const isActive =
                    pathname === item.href ||
                    pathname.startsWith(`${item.activePrefix}/`) ||
                    pathname === item.activePrefix;

                return (
                    <Link
                        key={item.href}
                        href={item.href}
                        aria-current={
                            isActive ? "page" : undefined
                        }
                        className={cn(
                            "relative rounded-lg px-3.5 py-2 text-sm font-medium leading-6 tracking-[0.01em]",
                            "transition-colors duration-200",
                            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                            isOverlay
                                ? "text-white/90 hover:bg-white/10 hover:text-white"
                                : "hover:bg-accent hover:text-accent-foreground",
                            isActive &&
                                (isOverlay
                                    ? "text-white"
                                    : "text-foreground"),
                            !isActive &&
                                !isOverlay &&
                                "text-muted-foreground",
                        )}
                    >
                        {item.label}

                        {isActive && (
                            <motion.span
                                layoutId="desktop-nav-active"
                                transition={{
                                    type: "spring",
                                    stiffness: 380,
                                    damping: 30,
                                }}
                                className={cn(
                                    "absolute inset-x-3 -bottom-1 h-0.5 rounded-full",
                                    isOverlay
                                        ? "bg-white"
                                        : "bg-primary",
                                )}
                            />
                        )}
                    </Link>
                );
            })}
        </nav>
    );
}
