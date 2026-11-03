"use client";

import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { DesktopNav } from "./desktop-nav";
import { MobileNav } from "./mobile-nav";
import { SearchDialog } from "./search-dialog";

import { Button } from "@/components/ui/button";
import { Logo } from "@/components/brand/logo";
import { ShoppingCartDrawer } from "@/components/cart/shopping-cart";
import { ProfileMenu } from "@/components/account/profile-menu";

import { useAuth } from "@/features/auth/context/auth-context";
import { useLogout } from "@/features/auth/hooks/use-logout";

export function SiteHeader() {
    const router = useRouter();
    const logoutMutation = useLogout();

    const {
        isLoading,
        authStatus,
        user,
    } = useAuth();

    const handleSignOut = () => {
        if (logoutMutation.isPending) {
            return;
        }

        logoutMutation.mutate(undefined, {
            onSuccess: () => {
                toast.success("You have signed out.");
                router.push("/");
            },
            onError: () => {
                toast.error("We couldn't end your session. Please try again.");
            },
        });
    };

    return (
        <header className="fixed inset-x-0 top-0 z-50 border-b border-border/70 bg-background/90 backdrop-blur-xl supports-[backdrop-filter]:bg-background/75">
            <div className="container mx-auto">
                <div className="relative flex h-16 items-center px-4 lg:px-6">
                    {/* Left */}
                    <div className="flex items-center gap-2.5">
                        <MobileNav
                            isAuthenticated={authStatus}
                            user={user}
                            onSignOut={handleSignOut}
                            isSigningOut={logoutMutation.isPending}
                        />

                        <Logo />
                    </div>

                    {/* Center */}
                    <div className="absolute left-1/2 hidden -translate-x-1/2 lg:block">
                        <DesktopNav />
                    </div>

                    {/* Right */}
                    <div className="ml-auto flex items-center gap-1.5">
                        <SearchDialog />

                        <ShoppingCartDrawer />

                        {!isLoading &&
                            (authStatus && user ? (
                                <div className="hidden lg:flex">
                                    <ProfileMenu
                                        user={user}
                                        onSignOut={handleSignOut}
                                        isSigningOut={logoutMutation.isPending}
                                    />
                                </div>
                            ) : (
                                <Button
                                    type="button"
                                    size="sm"
                                    className="ml-1 hidden h-9 cursor-pointer rounded-lg px-4 font-medium transition-all hover:-translate-y-px lg:inline-flex"
                                    onClick={() =>
                                        router.push("/sign-in")
                                    }
                                >
                                    Sign In
                                </Button>
                            ))}
                    </div>
                </div>
            </div>
        </header>
    );
}
