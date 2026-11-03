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

export function LandingHeader() {
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
        <header className="absolute inset-x-0 top-0 z-50">
            <div className="container mx-auto">
                <div className="relative flex h-20 items-center px-4 lg:px-6">
                    {/* Left */}
                    <div className="flex items-center gap-2.5">
                        <MobileNav
                            isAuthenticated={authStatus}
                            user={user}
                            onSignOut={handleSignOut}
                            isSigningOut={logoutMutation.isPending}
                            appearance="overlay"
                        />

                        <Logo appearance="overlay" />
                    </div>

                    {/* Center */}
                    <div className="absolute left-1/2 hidden -translate-x-1/2 lg:block">
                        <DesktopNav appearance="overlay" />
                    </div>

                    {/* Right */}
                    <div className="ml-auto flex items-center gap-1.5">
                        <SearchDialog
                            appearance="overlay"
                        />

                        <ShoppingCartDrawer
                            appearance="overlay"
                        />

                        {!isLoading &&
                            (authStatus && user ? (
                                <div className="hidden lg:flex">
                                    <ProfileMenu
                                        user={user}
                                        onSignOut={handleSignOut}
                                        isSigningOut={logoutMutation.isPending}
                                        appearance="overlay"
                                    />
                                </div>
                            ) : (
                                <Button
                                    type="button"
                                    size="sm"
                                    className="ml-1 hidden h-9 rounded-lg bg-white/5 px-4 font-medium text-white ring-1 ring-white/20 transition-colors hover:bg-white/20 lg:inline-flex cursor-pointer"
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
