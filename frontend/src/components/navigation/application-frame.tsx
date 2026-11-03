"use client";

import { usePathname } from "next/navigation";

import { NavigationShell } from "@/components/navigation/navigation-shell";
import { SiteFooter } from "@/components/navigation/site-footer";

export function ApplicationFrame({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const isOperationsApp = pathname === "/admin" || pathname.startsWith("/admin/") || pathname === "/inventory" || pathname.startsWith("/inventory/");

  if (isOperationsApp) {
    return <div className="min-h-screen">{children}</div>;
  }

  return (
    <div className="flex min-h-screen flex-col">
      <NavigationShell />
      <main className="flex-1">{children}</main>
      <SiteFooter />
    </div>
  );
}
