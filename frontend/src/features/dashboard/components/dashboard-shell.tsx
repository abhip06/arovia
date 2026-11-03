"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Activity,
  ArrowLeft,
  Boxes,
  Building2,
  ChartNoAxesCombined,
  ChevronLeft,
  ChevronRight,
  ClipboardList,
  CreditCard,
  LayoutDashboard,
  LogOut,
  Menu,
  PackageSearch,
  Settings2,
  Truck,
  Users,
  X,
} from "lucide-react";

import { Logo } from "@/components/brand/logo";
import { ThemeToggle } from "@/components/navigation/theme-toggle";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTitle } from "@/components/ui/sheet";
import { useAuth } from "@/features/auth/context/auth-context";
import { useLogout } from "@/features/auth/hooks/use-logout";
import { useDashboardUiStore } from "../stores/dashboard-ui.store";

type DashboardKind = "admin" | "inventory";
type NavItem = {
  label: string;
  href: string;
  icon: typeof LayoutDashboard;
  badge?: "LIVE" | "API" | "SOON";
};

const adminLinks: NavItem[] = [
  { label: "Overview", href: "/admin", icon: LayoutDashboard },
  { label: "Catalog", href: "/admin/catalog", icon: PackageSearch, badge: "API" },
  { label: "Orders", href: "/admin/orders", icon: ClipboardList, badge: "LIVE" },
  { label: "Payments", href: "/admin/payments", icon: CreditCard, badge: "LIVE" },
  { label: "Users & staff", href: "/admin/users", icon: Users, badge: "LIVE" },
  { label: "Shipping", href: "/admin/shipping", icon: Truck, badge: "SOON" },
  { label: "Reports", href: "/admin/reports", icon: ChartNoAxesCombined, badge: "SOON" },
  { label: "Settings", href: "/admin/settings", icon: Settings2, badge: "SOON" },
];

const inventoryLinks: NavItem[] = [
  { label: "Overview", href: "/inventory", icon: LayoutDashboard },
  { label: "Stock", href: "/inventory/stock", icon: Boxes, badge: "LIVE" },
  { label: "Batches & expiry", href: "/inventory/batches", icon: PackageSearch, badge: "LIVE" },
  { label: "Movements", href: "/inventory/movements", icon: Activity, badge: "LIVE" },
  { label: "Warehouses", href: "/inventory/warehouses", icon: Building2, badge: "LIVE" },
  { label: "Replenishment", href: "/inventory/replenishment", icon: ClipboardList, badge: "SOON" },
];

const pageTitles: Record<string, string> = {
  "/admin": "Operations overview",
  "/admin/catalog": "Catalog management",
  "/admin/orders": "Order management",
  "/admin/payments": "Payment activity",
  "/admin/users": "Users & staff",
  "/admin/shipping": "Shipping",
  "/admin/reports": "Reports",
  "/admin/settings": "Platform settings",
  "/inventory": "Inventory overview",
  "/inventory/stock": "Stock management",
  "/inventory/batches": "Batches & expiry",
  "/inventory/movements": "Stock movements",
  "/inventory/warehouses": "Warehouses",
  "/inventory/replenishment": "Replenishment",
};

function AccessMessage({
  loading,
  allowed,
  signedIn,
}: {
  loading: boolean;
  allowed: boolean;
  signedIn: boolean;
}) {
  if (loading) {
    return (
      <div className="grid min-h-[55vh] place-items-center">
        <div className="flex items-center gap-3 text-sm text-muted-foreground">
          <span className="size-2 animate-pulse rounded-full bg-primary" />
          Checking workspace access…
        </div>
      </div>
    );
  }

  if (allowed) return null;

  if (!signedIn) {
    return (
      <AccessNotice
        icon={LogOut}
        title="Sign in to continue"
        description="Sign in with an account that has access to this workspace."
        linkHref="/sign-in"
        linkLabel="Continue to sign in"
        actionIcon={ArrowLeft}
        rotateActionIcon
      />
    );
  }

  return (
    <AccessNotice
      icon={Users}
      title="Workspace access required"
      description="Your signed-in role does not have access to this workspace. The backend enforces authorization for supported operations."
      linkHref="/"
      linkLabel="Return to storefront"
      actionIcon={ArrowLeft}
    />
  );
}

function AccessNotice({
  icon: Icon,
  title,
  description,
  linkHref,
  linkLabel,
  actionIcon: ActionIcon,
  rotateActionIcon = false,
}: {
  icon: typeof LogOut;
  title: string;
  description: string;
  linkHref: string;
  linkLabel: string;
  actionIcon: typeof ArrowLeft;
  rotateActionIcon?: boolean;
}) {
  return (
    <div className="mx-auto grid min-h-[65vh] max-w-lg place-items-center px-6 text-center">
      <div>
        <div className="mx-auto grid size-14 place-items-center rounded-2xl bg-primary/10 text-primary">
          <Icon className="size-6" />
        </div>
        <h1 className="mt-5 text-2xl font-semibold">{title}</h1>
        <p className="mt-2 text-sm leading-6 text-muted-foreground">{description}</p>
        <Link href={linkHref} className="mt-5 inline-flex items-center gap-2 text-sm font-medium text-primary">
          {!rotateActionIcon && <ActionIcon className="size-4" />}
          {linkLabel}
          {rotateActionIcon && <ActionIcon className="size-4 rotate-180" />}
        </Link>
      </div>
    </div>
  );
}

function DashboardSidebar({
  kind,
  mobile = false,
  collapsed = false,
  onClose,
  onToggleCollapse,
}: {
  kind: DashboardKind;
  mobile?: boolean;
  collapsed?: boolean;
  onClose?: () => void;
  onToggleCollapse?: () => void;
}) {
  const pathname = usePathname();
  const links = kind === "admin" ? adminLinks : inventoryLinks;
  const compact = collapsed && !mobile;

  return (
    <aside
      className={`${mobile ? "relative h-full w-full" : `sticky top-0 hidden h-screen shrink-0 border-r transition-[width] duration-200 lg:block ${compact ? "w-[76px]" : "w-[258px]"}`} border-border bg-card`}
    >
      <div className="flex h-16 items-center justify-between border-b px-4">
        <Logo contentType="HEADER" textClassName={compact ? "hidden" : "text-2xl"} imageSize={36} />
        {mobile ? (
          <Button variant="ghost" size="icon" aria-label="Close navigation" onClick={onClose}>
            <X />
          </Button>
        ) : (
          <Button
            variant="ghost"
            size="icon"
            aria-label={compact ? "Expand sidebar" : "Collapse sidebar"}
            title={compact ? "Expand sidebar" : "Collapse sidebar"}
            onClick={onToggleCollapse}
            className="hidden lg:inline-flex"
          >
            {compact ? <ChevronRight /> : <ChevronLeft />}
          </Button>
        )}
      </div>

      <div className="px-3 pt-5">
        {!compact && (
          <div className="mb-5 rounded-xl border bg-background/70 p-3">
            <p className="text-[9px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">Workspace</p>
            <p className="mt-1 text-sm font-semibold">{kind === "admin" ? "Platform control" : "Inventory operations"}</p>
            <p className="mt-1 text-[10px] leading-4 text-muted-foreground">
              {kind === "admin" ? "Catalog, orders and access" : "Stock, lots and audit trail"}
            </p>
          </div>
        )}

        <p className={`mb-3 px-2 text-[10px] font-semibold uppercase tracking-[0.16em] text-muted-foreground ${compact ? "sr-only" : ""}`}>
          {kind === "admin" ? "Manage" : "Operations"}
        </p>
        <nav aria-label={`${kind} navigation`} className="space-y-1">
          {links.map(({ label, href, icon: Icon, badge }) => {
            const active = pathname === href || (
              href !== "/admin" &&
              href !== "/inventory" &&
              pathname.startsWith(`${href}/`)
            );

            return (
              <Link
                key={href}
                href={href}
                onClick={mobile ? onClose : undefined}
                title={compact ? label : undefined}
                aria-current={active ? "page" : undefined}
                className={`group flex h-10 items-center gap-3 rounded-lg px-3 text-sm transition-colors ${
                  active
                    ? "bg-primary/10 font-medium text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                <Icon className="size-[18px] shrink-0" />
                <span className={compact ? "sr-only" : "flex-1"}>{label}</span>
                {badge && !compact && <NavigationBadge kind={badge} />}
              </Link>
            );
          })}
        </nav>
      </div>

      <div className="absolute inset-x-3 bottom-4 border-t pt-3">
        <Link href="/" className="flex items-center gap-3 rounded-lg px-3 py-2 text-xs text-muted-foreground hover:bg-muted hover:text-foreground">
          <ArrowLeft className="size-4 shrink-0" />
          <span className={compact ? "sr-only" : ""}>Back to storefront</span>
        </Link>
      </div>
    </aside>
  );
}

function NavigationBadge({ kind }: { kind: NonNullable<NavItem["badge"]> }) {
  const style = {
    LIVE: "bg-emerald-500/10 text-emerald-700 dark:text-emerald-300",
    API: "bg-primary/10 text-primary",
    SOON: "bg-muted text-muted-foreground",
  }[kind];

  return (
    <span className={`rounded-full px-1.5 py-0.5 text-[8px] font-semibold tracking-wide ${style}`}>
      {kind}
    </span>
  );
}

export function DashboardShell({
  kind,
  children,
}: {
  kind: DashboardKind;
  children: React.ReactNode;
}) {
  const { isLoading, user } = useAuth();
  const role = user?.role ?? "";
  const allowed = kind === "admin"
    ? role === "Admin"
    : role === "Admin" || role === "WarehouseStaff";
  const { sidebarCollapsed, toggleSidebar } = useDashboardUiStore();
  const [mobileOpen, setMobileOpen] = useState(false);
  const logout = useLogout();
  const title = pageTitles[usePathname()] ?? (kind === "admin" ? "Admin workspace" : "Inventory operations");

  return (
    <div className="min-h-screen bg-muted/25 text-foreground">
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent side="left" showCloseButton={false} className="w-[280px] max-w-[85vw] p-0">
          <SheetTitle className="sr-only">{kind === "admin" ? "Admin" : "Inventory"} navigation</SheetTitle>
          <DashboardSidebar kind={kind} mobile onClose={() => setMobileOpen(false)} />
        </SheetContent>
      </Sheet>

      <div className="flex min-h-screen">
        <DashboardSidebar kind={kind} collapsed={sidebarCollapsed} onToggleCollapse={toggleSidebar} />
        <div className="min-w-0 flex-1">
          <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b bg-background/90 px-4 backdrop-blur-md sm:px-6 lg:px-8">
            <div className="flex min-w-0 items-center gap-3">
              <Button
                variant="ghost"
                size="icon"
                className="lg:hidden"
                aria-label="Open navigation"
                onClick={() => setMobileOpen(true)}
              >
                <Menu />
              </Button>
              <div className="min-w-0">
                <p className="text-[10px] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                  Arovia · {kind === "admin" ? "Admin" : "Inventory"}
                </p>
                <p className="truncate text-sm font-semibold">{title}</p>
              </div>
              <span className="ml-2 hidden items-center gap-1.5 rounded-full border bg-card px-2.5 py-1 text-[10px] text-muted-foreground md:inline-flex">
                <span className="size-1.5 rounded-full bg-emerald-500" />
                Live workspace
              </span>
            </div>

            <div className="flex shrink-0 items-center gap-2">
              <ThemeToggle />
              <div className="hidden border-l pl-3 sm:block">
                <p className="max-w-44 truncate text-xs font-medium">
                  {user ? `${user.firstName} ${user.lastName}` : "Signed out"}
                </p>
                <p className="text-[10px] text-muted-foreground">{role || "—"}</p>
              </div>
              {user && (
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label="Sign out"
                  title="Sign out"
                  disabled={logout.isPending}
                  onClick={() => logout.mutate()}
                >
                  <LogOut />
                </Button>
              )}
            </div>
          </header>

          <AccessMessage loading={isLoading} allowed={allowed} signedIn={Boolean(user)} />
          {!isLoading && allowed && (
            <main className="mx-auto w-full max-w-[1600px] px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
              {children}
            </main>
          )}
        </div>
      </div>
    </div>
  );
}
