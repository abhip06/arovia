"use client";

import { useRouter } from "next/navigation";

import {
    ChevronDown,
    LogOut,
    MapPin,
    Package,
    Settings,
    ShoppingBag,
    User,
    LayoutDashboard,
    Boxes,
} from "lucide-react";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuGroup,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export interface ProfileMenuUser {
    firstName: string;
    lastName: string;
    email: string;
    avatarUrl?: string;
    role?: string;
}

interface ProfileMenuProps {
    user: ProfileMenuUser;
    onSignOut?: () => void;
    isSigningOut?: boolean;
    appearance?: "default" | "overlay";
}

function getInitials(firstName: string, lastName: string) {
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}

export function ProfileMenu({
    user,
    onSignOut,
    isSigningOut = false,
    appearance = "default",
}: ProfileMenuProps) {
    const router = useRouter();

    const fullName = `${user.firstName} ${user.lastName}`;
    const initials = getInitials(
        user.firstName,
        user.lastName,
    );

    const isOverlay = appearance === "overlay";
    const dashboardItems = user.role === "Admin"
        ? [{ label: "Admin dashboard", href: "/admin", icon: LayoutDashboard }, { label: "Inventory dashboard", href: "/inventory", icon: Boxes }]
        : user.role === "WarehouseStaff"
            ? [{ label: "Inventory dashboard", href: "/inventory", icon: Boxes }]
            : [];

    const handleNavigation = (path: string) => {
        router.push(path);
    };

    const handleSignOut = () => {
        onSignOut?.();
    };

    return (
        <DropdownMenu>
            <DropdownMenuTrigger
                render={
                    <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        aria-label="Open account menu"
                        className={
                            isOverlay
                                ? "hidden h-10 gap-2 rounded-xl px-2 text-white transition-colors hover:cursor-pointer hover:bg-white/10 hover:text-white lg:flex"
                                : "hidden h-10 gap-2 rounded-xl px-2 transition-colors hover:cursor-pointer hover:bg-muted lg:flex"
                        }
                    />
                }
            >
                <Avatar
                    className={
                        isOverlay
                            ? "size-8 border border-white/30"
                            : "size-8 border border-border/70"
                    }
                >
                    {user.avatarUrl ? (
                        <AvatarImage
                            src={user.avatarUrl}
                            alt={fullName}
                        />
                    ) : null}

                    <AvatarFallback
                        className={
                            isOverlay
                                ? "bg-white/15 text-xs font-semibold text-white"
                                : "bg-primary/10 text-xs font-semibold text-primary"
                        }
                    >
                        {initials}
                    </AvatarFallback>
                </Avatar>

                <span
                    className={
                        isOverlay
                            ? "hidden max-w-28 truncate text-sm font-medium text-white xl:block"
                            : "hidden max-w-28 truncate text-sm font-medium text-foreground xl:block"
                    }
                >
                    {user.firstName}
                </span>

                <ChevronDown
                    className={
                        isOverlay
                            ? "hidden size-4 text-white/80 xl:block"
                            : "hidden size-4 text-muted-foreground xl:block"
                    }
                />
            </DropdownMenuTrigger>

            <DropdownMenuContent
                align="end"
                sideOffset={10}
                className="w-[280px] rounded-2xl border-border/70 bg-popover p-2 shadow-xl"
            >
                {/* Account header */}
                <div className="px-3 py-2.5">
                    <div className="flex items-center gap-3">
                        <Avatar className="size-10 border border-border/70">
                            {user.avatarUrl ? (
                                <AvatarImage
                                    src={user.avatarUrl}
                                    alt={fullName}
                                />
                            ) : null}

                            <AvatarFallback className="bg-primary/10 font-semibold text-primary">
                                {initials}
                            </AvatarFallback>
                        </Avatar>

                        <div className="min-w-0 flex-1">
                            <p className="truncate text-sm font-semibold text-foreground">
                                {fullName}
                            </p>

                            <p className="truncate text-xs text-muted-foreground">
                                {user.email}
                            </p>
                        </div>
                    </div>
                </div>

                <DropdownMenuSeparator className="my-1 bg-border/70" />

                <DropdownMenuGroup>
                    {dashboardItems.map((item) => {
                        const Icon = item.icon;
                        return <DropdownMenuItem key={item.href} className="cursor-pointer gap-3 rounded-xl px-3 py-2.5" onClick={() => handleNavigation(item.href)}><Icon className="size-4 text-primary" /><span>{item.label}</span></DropdownMenuItem>;
                    })}
                    <DropdownMenuItem
                        className="cursor-pointer gap-3 rounded-xl px-3 py-2.5"
                        onClick={() =>
                            handleNavigation("/my-account")
                        }
                    >
                        <User className="size-4 text-muted-foreground" />
                        <span>My Account</span>
                    </DropdownMenuItem>

                    <DropdownMenuItem
                        className="cursor-pointer gap-3 rounded-xl px-3 py-2.5"
                        onClick={() =>
                            handleNavigation("/orders")
                        }
                    >
                        <Package className="size-4 text-muted-foreground" />
                        <span>My orders</span>
                    </DropdownMenuItem>

                    <DropdownMenuItem
                        className="cursor-pointer gap-3 rounded-xl px-3 py-2.5"
                        onClick={() =>
                            handleNavigation("/addresses")
                        }
                    >
                        <MapPin className="size-4 text-muted-foreground" />
                        <span>Saved addresses</span>
                    </DropdownMenuItem>

                    <DropdownMenuItem
                        className="cursor-pointer gap-3 rounded-xl px-3 py-2.5"
                        onClick={() =>
                            handleNavigation("/settings")
                        }
                    >
                        <Settings className="size-4 text-muted-foreground" />
                        <span>Settings</span>
                    </DropdownMenuItem>
                </DropdownMenuGroup>

                <DropdownMenuSeparator className="my-1 bg-border/70" />

                <DropdownMenuItem
                    disabled={isSigningOut}
                    className="cursor-pointer gap-3 rounded-xl px-3 py-2.5 text-destructive focus:text-destructive"
                    onClick={handleSignOut}
                >
                    <LogOut className="size-4" />
                    <span>{isSigningOut ? "Signing out..." : "Sign out"}</span>
                </DropdownMenuItem>

                <div className="flex items-center gap-2 px-3 pb-1 pt-2">
                    <ShoppingBag className="size-3.5 text-primary" />

                    <span className="text-[11px] text-muted-foreground">
                        Your Arovia account
                    </span>
                </div>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
