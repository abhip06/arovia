"use client";

import { usePathname } from "next/navigation";

import { LandingHeaderController } from "./landing-header-controller";
import { SiteHeader } from "./site-header";

export function NavigationShell() {
    const pathname = usePathname();

    if (pathname === "/") {
        return <LandingHeaderController />;
    }

    return <SiteHeader />;
}