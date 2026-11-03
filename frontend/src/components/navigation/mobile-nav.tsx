"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
    LogOut,
    MapPin,
    Menu,
    Package,
    Settings,
    UserRound,
    LayoutDashboard,
    Boxes,
} from "lucide-react";
import { motion } from "framer-motion";
import { useState } from "react";

import {
    Sheet,
    SheetContent,
    SheetHeader,
    SheetTrigger,
} from "@/components/ui/sheet";

import { Button } from "@/components/ui/button";

import { ThemeToggle } from "./theme-toggle";
import { Logo } from "../brand/logo";
import type { CurrentUser } from "@/types/user.types";
import { cn } from "@/lib/utils";
import { navigationItems } from "./navigation-items";

interface MobileNavProps {
    isAuthenticated: boolean;
    user?: CurrentUser | null;
    onSignOut?: () => void;
    isSigningOut?: boolean;
    appearance?: "default" | "overlay";
}

const accountItems = [
    {
        label: "My Account",
        href: "/my-account",
        icon: UserRound,
    },
    {
        label: "My orders",
        href: "/orders",
        icon: Package,
    },
    {
        label: "Saved addresses",
        href: "/addresses",
        icon: MapPin,
    },
    {
        label: "Settings",
        href: "/settings",
        icon: Settings,
    },
];

export function MobileNav({
    isAuthenticated,
    user,
    onSignOut,
    isSigningOut = false,
    appearance = "default",
}: MobileNavProps) {
    const [open, setOpen] = useState(false);
    const pathname = usePathname();

    const fullName = user
        ? `${user.firstName} ${user.lastName}`
        : "";

    const isOverlay = appearance === "overlay";
    const roleItems = user?.role === "Admin"
        ? [{ label: "Admin dashboard", href: "/admin", icon: LayoutDashboard }, { label: "Inventory dashboard", href: "/inventory", icon: Boxes }]
        : user?.role === "WarehouseStaff"
            ? [{ label: "Inventory dashboard", href: "/inventory", icon: Boxes }]
            : [];

    const handleClose = () => {
        setOpen(false);
    };

    const handleSignOut = () => {
        onSignOut?.();
        setOpen(false);
    };

    return (
        <Sheet
            open={open}
            onOpenChange={setOpen}
        >
            <SheetTrigger
    render={
        <Button
            type="button"
            variant="ghost"
            size="icon"
            aria-label="Open navigation menu"
            className={
                isOverlay
                    ? "size-9 rounded-lg text-white hover:bg-white/10 hover:text-white lg:hidden"
                    : "size-9 rounded-lg lg:hidden"
            }
        >
            <Menu className="size-5" />
        </Button>
    }
/>

            <SheetContent
                side="left"
                className="flex w-[320px] flex-col p-0 sm:w-[380px]"
            >
                <SheetHeader className="border-b px-6 py-5">
                    <Logo />
                </SheetHeader>

                {/* User summary */}
                {isAuthenticated && user && (
                    <div className="flex min-w-0 flex-col gap-1 border-b px-7 pb-4 pt-5">
                        <p className="truncate text-sm font-semibold text-foreground">
                            {fullName}
                        </p>

                        <p className="truncate text-xs text-muted-foreground">
                            {user.email}
                        </p>
                    </div>
                )}

                <div className="flex min-h-0 flex-1 flex-col px-4">
                    {/* Primary navigation */}
                    <nav
                        aria-label="Mobile navigation"
                        className="mt-7 flex flex-col gap-1"
                    >
                        {navigationItems.map((item, index) => {
                            const isActive =
                                pathname === item.activePrefix ||
                                pathname.startsWith(`${item.activePrefix}/`);

                            return (
                                <motion.div
                                    key={item.href}
                                    initial={{ opacity: 0, x: -8 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    transition={{ duration: 0.2, delay: index * 0.035 }}
                                >
                                    <Link
                                        href={item.href}
                                        onClick={handleClose}
                                        aria-current={isActive ? "page" : undefined}
                                        className={cn(
                                            "flex min-h-11 items-center rounded-lg px-3.5 py-2.5 text-sm font-medium tracking-[0.01em] transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                                            isActive ? "bg-accent/70 text-primary" : "text-foreground",
                                        )}
                                    >
                                        {item.label}
                                    </Link>
                                </motion.div>
                            );
                        })}
                    </nav>

                    {/* Account */}
                    {isAuthenticated && user && (
                        <section
                            aria-label="Account"
                            className="mt-7 border-t pt-5"
                        >
                            <nav
                                aria-label="Account navigation"
                                className="flex flex-col gap-1"
                            >
                                {[...roleItems, ...accountItems].map(
                                    (item) => {
                                        const Icon =
                                            item.icon;

                                        return (
                                            <Link
                                                key={
                                                    item.href
                                                }
                                                href={
                                                    item.href
                                                }
                                                onClick={
                                                    handleClose
                                                }
                                                className="flex min-h-11 items-center gap-3 rounded-lg px-3.5 py-2.5 text-sm font-medium tracking-[0.01em] text-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                                            >
                                                <Icon className="size-[18px] text-muted-foreground" />

                                                <span>
                                                    {
                                                        item.label
                                                    }
                                                </span>
                                            </Link>
                                        );
                                    },
                                )}
                            </nav>

                            {/* Sign out */}
                            <div className="mt-3 border-t pt-3">
                                <button
                                    type="button"
                                    onClick={
                                        handleSignOut
                                    }
                                    disabled={isSigningOut}
                                    className="flex min-h-11 w-full items-center gap-3 rounded-lg px-3.5 py-2.5 text-left text-sm font-medium tracking-[0.01em] text-destructive transition-colors hover:bg-destructive/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-60"
                                >
                                    <LogOut className="size-[18px]" />
                                    <span>
                                        {isSigningOut ? "Signing out..." : "Sign out"}
                                    </span>
                                </button>
                            </div>
                        </section>
                    )}

                    {/* Appearance */}
                    <div className="mt-auto border-t pb-6 pt-4">
                        <div className="flex min-h-11 items-center justify-between px-3.5">
                            <span className="text-sm font-medium">
                                Appearance
                            </span>

                            <ThemeToggle />
                        </div>
                    </div>
                </div>
            </SheetContent>
        </Sheet>
    );
}
