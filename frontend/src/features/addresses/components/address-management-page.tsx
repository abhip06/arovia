"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import {
    ArrowRight,
    Building2,
    Check,
    CheckCircle2,
    Clock,
    House,
    LoaderCircle,
    MapPin,
    MapPinned,
    MoreHorizontal,
    Pencil,
    Plus,
    RefreshCw,
    ShieldCheck,
    Star,
    Trash2,
} from "lucide-react";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
} from "@/components/ui/card";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";

import { useAuth } from "@/features/auth/context/auth-context";
import {
    useAddresses,
    useDeleteAddress,
    useSetPrimaryAddress,
} from "@/features/addresses/hooks/use-addresses";

import { AddressFormDialog } from "./address-form-dialog";
import type { Address } from "@/types/address.types";

function getAddressIcon(label: string) {
    const normalized = label.trim().toLowerCase();

    if (normalized === "home") {
        return House;
    }

    if (
        normalized === "work" ||
        normalized === "office"
    ) {
        return Building2;
    }

    return MapPinned;
}

function formatAddress(address: Address) {
    return [
        address.addressLine1,
        address.addressLine2,
        `${address.city}, ${address.state} ${address.postalCode}`,
        address.country,
    ]
        .filter(Boolean)
        .join(", ");
}

export function AddressManagementPage() {
    const router = useRouter();

    const {
        authStatus,
        isLoading: isAuthLoading,
        user,
    } = useAuth();

    const isAuthenticated =
        !isAuthLoading && Boolean(authStatus && user);

    const addressesQuery = useAddresses(isAuthenticated);
    const deleteAddress = useDeleteAddress();
    const setPrimaryAddress = useSetPrimaryAddress();

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingAddress, setEditingAddress] =
        useState<Address | null>(null);

    const [addressToDelete, setAddressToDelete] =
        useState<Address | null>(null);

    const addresses = addressesQuery.data ?? [];

    const sortedAddresses = [...addresses].sort(
        (first, second) =>
            Number(second.isPrimary) - Number(first.isPrimary),
    );

    const isAddressActionPending =
        deleteAddress.isPending ||
        setPrimaryAddress.isPending;

    const openCreateDialog = () => {
        setEditingAddress(null);
        setIsFormOpen(true);
    };

    const openEditDialog = (address: Address) => {
        setEditingAddress(address);
        setIsFormOpen(true);
    };

    const closeFormDialog = (open: boolean) => {
        setIsFormOpen(open);

        if (!open) {
            setEditingAddress(null);
        }
    };

    const handleAddressSaved = () => {
        setIsFormOpen(false);
        setEditingAddress(null);
    };

    const handleSetPrimary = async (address: Address) => {
        if (address.isPrimary || isAddressActionPending) {
            return;
        }

        try {
            await setPrimaryAddress.mutateAsync(address.id);

            toast.success(
                `${address.label || "Address"} is now your primary address.`,
            );
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Could not update your primary address.",
            );
        }
    };

    const handleDeleteAddress = async () => {
        if (!addressToDelete || isAddressActionPending) {
            return;
        }

        try {
            await deleteAddress.mutateAsync(
                addressToDelete.id,
            );

            toast.success("Address deleted successfully.");
            setAddressToDelete(null);
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Could not delete this address.",
            );
        }
    };

    if (isAuthLoading) {
        return (
            <main className="mx-auto w-full max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
                <AddressPageSkeleton />
            </main>
        );
    }

    if (!authStatus || !user) {
        return (
            <main className="flex min-h-[65vh] items-center justify-center px-4 py-12">
                <Card className="w-full max-w-md rounded-3xl border-border/70 shadow-sm">
                    <CardContent className="flex flex-col items-center p-8 text-center sm:p-10">
                        <div className="flex size-16 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                            <ShieldCheck className="size-7" />
                        </div>

                        <h1 className="mt-5 text-xl font-semibold tracking-tight">
                            Your addresses, all in one place
                        </h1>

                        <p className="mt-2 text-sm leading-6 text-muted-foreground">
                            Sign in to manage your saved delivery
                            addresses and choose where your orders
                            should arrive.
                        </p>

                        <Button
                            className="mt-6 rounded-xl"
                            onClick={() =>
                                router.push(
                                    `/sign-in?returnUrl=${encodeURIComponent(
                                        "/account/addresses",
                                    )}`,
                                )
                            }
                        >
                            Sign in to continue
                            <ArrowRight className="ml-2 size-4" />
                        </Button>
                    </CardContent>
                </Card>
            </main>
        );
    }

    return (
        <main className="min-h-[65vh] bg-background mt-16">
            <div className="mx-auto w-full max-w-6xl px-4 py-7 sm:px-6 sm:py-10 lg:px-8">
                {/* Breadcrumb */}
                <nav
                    aria-label="Breadcrumb"
                    className="mb-7 flex items-center gap-2 text-xs text-muted-foreground"
                >
                    <button
                        type="button"
                        onClick={() => router.push("/")}
                        className="transition-colors hover:text-foreground cursor-pointer"
                    >
                        Home
                    </button>

                    <span aria-hidden="true">/</span>

                    <button
                        type="button"
                        onClick={() =>
                            router.push("/account")
                        }
                        className="transition-colors hover:text-foreground cursor-pointer"
                    >
                        Account
                    </button>

                    <span aria-hidden="true">/</span>

                    <span
                        aria-current="page"
                        className="font-medium text-foreground"
                    >
                        Addresses
                    </span>
                </nav>

                {/* Page heading */}
                <motion.header
                    initial={{ opacity: 0, y: 8 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.25 }}
                    className="mb-8 flex flex-col justify-between gap-5 sm:flex-row sm:items-end"
                >
                    <div className="max-w-2xl">
                        <div className="mb-3 flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.14em] text-primary">
                            <span className="size-1.5 rounded-full bg-primary" />
                            Your Arovia account
                        </div>

                        <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">
                            Saved addresses
                        </h1>

                        <p className="mt-3 max-w-xl text-sm leading-6 text-muted-foreground sm:text-base">
                            Manage your delivery details in one
                            calm, convenient place. Choose a
                            primary address for a smoother checkout.
                        </p>
                    </div>

                    <Button
                        onClick={openCreateDialog}
                        className="w-full rounded-xl shadow-sm sm:w-auto cursor-pointer"
                    >
                        <Plus className="mr-2 size-4" />
                        Add new address
                    </Button>
                </motion.header>

                {/* Overview */}
                <div className="mb-8 grid gap-4 sm:grid-cols-2">
                    <Card className="overflow-hidden rounded-2xl border-border/70 shadow-sm">
                        <CardContent className="flex items-center justify-between gap-4 p-5 sm:p-6">
                            <div>
                                <p className="text-sm font-medium text-muted-foreground">
                                    Saved addresses
                                </p>

                                <p className="mt-2 text-3xl font-semibold tracking-tight">
                                    {addressesQuery.isError
                                        ? "—"
                                        : addressesQuery.isPending
                                            ? "…"
                                            : addresses.length}
                                </p>

                                <p className="mt-1 text-xs text-muted-foreground">
                                    {addresses.length === 1
                                        ? "One place saved for delivery"
                                        : "Your delivery locations"}
                                </p>
                            </div>

                            <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                <MapPin className="size-5" />
                            </div>
                        </CardContent>
                    </Card>

                    <Card className="overflow-hidden rounded-2xl border-border/70 shadow-sm">
                        <CardContent className="flex items-center justify-between gap-4 p-5 sm:p-6">
                            <div className="min-w-0">
                                <p className="text-sm font-medium text-muted-foreground">
                                    Primary address
                                </p>

                                <p className="mt-2 truncate text-lg font-semibold tracking-tight">
                                    {addressesQuery.isError
                                        ? "Unavailable"
                                        : addressesQuery.isPending
                                            ? "Loading…"
                                            : addresses.find(
                                                (address) =>
                                                    address.isPrimary,
                                            )?.label ??
                                            "Not selected"}
                                </p>

                                <p className="mt-1 truncate text-xs text-muted-foreground">
                                    {addresses.find(
                                        (address) =>
                                            address.isPrimary,
                                    )?.postalCode ??
                                        "Choose an address for delivery"}
                                </p>
                            </div>

                            <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-blue-500/10 text-blue-600 dark:text-blue-400">
                                <Star className="size-5" />
                            </div>
                        </CardContent>
                    </Card>
                </div>

                {/* Error state */}
                {addressesQuery.isError && (
                    <Card className="rounded-2xl border-destructive/20">
                        <CardContent className="flex flex-col items-start gap-4 p-6 sm:flex-row sm:items-center sm:justify-between">
                            <div className="flex items-start gap-3">
                                <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-destructive/10 text-destructive">
                                    <MapPin className="size-5" />
                                </div>

                                <div>
                                    <h2 className="font-semibold">
                                        We couldn't load your addresses
                                    </h2>

                                    <p className="mt-1 text-sm leading-6 text-muted-foreground">
                                        {addressesQuery.error instanceof Error
                                            ? addressesQuery.error.message
                                            : "Please check your connection and try again."}
                                    </p>
                                </div>
                            </div>

                            <Button
                                variant="outline"
                                className="rounded-xl cursor-pointer"
                                onClick={() =>
                                    void addressesQuery.refetch()
                                }
                                disabled={addressesQuery.isFetching}
                            >
                                <RefreshCw
                                    className={`mr-2 size-4 ${addressesQuery.isFetching
                                            ? "animate-spin"
                                            : ""
                                        }`}
                                />
                                Try again
                            </Button>
                        </CardContent>
                    </Card>
                )}

                {/* Loading state */}
                {addressesQuery.isPending &&
                    !addressesQuery.isError && (
                        <AddressPageSkeleton />
                    )}

                {/* Address list */}
                {!addressesQuery.isPending &&
                    !addressesQuery.isError &&
                    addresses.length > 0 && (
                        <section aria-labelledby="address-list-title">
                            <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
                                <div>
                                    <h2
                                        id="address-list-title"
                                        className="text-lg font-semibold tracking-tight"
                                    >
                                        Your delivery locations
                                    </h2>

                                    <p className="mt-1 text-sm text-muted-foreground">
                                        Update details or choose which
                                        address to use by default.
                                    </p>
                                </div>

                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="rounded-lg cursor-pointer"
                                    onClick={() =>
                                        void addressesQuery.refetch()
                                    }
                                    disabled={addressesQuery.isFetching}
                                >
                                    <RefreshCw
                                        className={`mr-2 size-3.5 ${addressesQuery.isFetching
                                                ? "animate-spin"
                                                : ""
                                            }`}
                                    />
                                    Refresh
                                </Button>
                            </div>

                            <div className="grid gap-4 lg:grid-cols-2">
                                {sortedAddresses.map(
                                    (address, index) => {
                                        const AddressIcon =
                                            getAddressIcon(
                                                address.label,
                                            );

                                        return (
                                            <motion.div
                                                key={address.id}
                                                initial={{
                                                    opacity: 0,
                                                    y: 8,
                                                }}
                                                animate={{
                                                    opacity: 1,
                                                    y: 0,
                                                }}
                                                transition={{
                                                    duration: 0.22,
                                                    delay:
                                                        Math.min(
                                                            index,
                                                            5,
                                                        ) * 0.035,
                                                }}
                                            >
                                                <Card
                                                    className={`h-full overflow-hidden rounded-2xl shadow-sm transition-all duration-200 hover:-translate-y-0.5 hover:shadow-md ${address.isPrimary
                                                            ? "border-primary/35 ring-1 ring-primary/10"
                                                            : "border-border/70"
                                                        }`}
                                                >
                                                    <CardContent className="flex h-full flex-col p-5 sm:p-6">
                                                        <div className="flex items-start justify-between gap-3">
                                                            <div className="flex min-w-0 items-center gap-3">
                                                                <div
                                                                    className={`flex size-12 shrink-0 items-center justify-center rounded-2xl ${address.isPrimary
                                                                            ? "bg-primary/10 text-primary"
                                                                            : "bg-muted text-muted-foreground"
                                                                        }`}
                                                                >
                                                                    <AddressIcon className="size-5" />
                                                                </div>

                                                                <div className="min-w-0">
                                                                    <div className="flex flex-wrap items-center gap-2">
                                                                        <h3 className="truncate font-semibold">
                                                                            {address.label ||
                                                                                "Saved address"}
                                                                        </h3>

                                                                        {address.isPrimary && (
                                                                            <Badge className="rounded-full border-0 bg-primary/10 text-primary hover:bg-primary/10">
                                                                                <Check className="mr-1 size-3" />
                                                                                Primary
                                                                            </Badge>
                                                                        )}
                                                                    </div>

                                                                    <p className="mt-1 text-xs text-muted-foreground">
                                                                        {address.city},{" "}
                                                                        {address.state}
                                                                    </p>
                                                                </div>
                                                            </div>

                                                            {address.isPrimary && (
                                                                <div
                                                                    className="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary"
                                                                    title="Primary delivery address"
                                                                >
                                                                    <CheckCircle2 className="size-4" />
                                                                </div>
                                                            )}
                                                        </div>

                                                        <Separator className="my-5" />

                                                        <div className="flex-1 space-y-3">
                                                            <div>
                                                                <p className="text-sm font-medium">
                                                                    {address.recipientName}
                                                                </p>

                                                                {address.recipientPhone && (
                                                                    <p className="mt-1 text-xs text-muted-foreground">
                                                                        {address.recipientPhone}
                                                                    </p>
                                                                )}
                                                            </div>

                                                            <div className="flex items-start gap-2.5">
                                                                <MapPin className="mt-0.5 size-4 shrink-0 text-muted-foreground" />

                                                                <p className="text-sm leading-6 text-muted-foreground">
                                                                    {address.addressLine1}

                                                                    {address.addressLine2 && (
                                                                        <>
                                                                            <br />
                                                                            {address.addressLine2}
                                                                        </>
                                                                    )}

                                                                    <br />
                                                                    {address.city},{" "}
                                                                    {address.state}{" "}
                                                                    {address.postalCode}
                                                                    <br />
                                                                    {address.country}
                                                                </p>
                                                            </div>
                                                        </div>

                                                        <Separator className="my-5" />

                                                        <div className="flex flex-wrap items-center gap-2">
                                                            {!address.isPrimary ? (
                                                                <Button
                                                                    type="button"
                                                                    variant="outline"
                                                                    size="sm"
                                                                    className="rounded-xl cursor-pointer"
                                                                    disabled={
                                                                        isAddressActionPending
                                                                    }
                                                                    onClick={() =>
                                                                        void handleSetPrimary(
                                                                            address,
                                                                        )
                                                                    }
                                                                >
                                                                    {setPrimaryAddress.isPending ? (
                                                                        <LoaderCircle className="mr-2 size-3.5 animate-spin" />
                                                                    ) : (
                                                                        <Star className="mr-2 size-3.5" />
                                                                    )}
                                                                    Set as primary
                                                                </Button>
                                                            ) : (
                                                                <span className="inline-flex items-center gap-1.5 px-2 text-xs font-medium text-primary">
                                                                    <CheckCircle2 className="size-3.5" />
                                                                    Default for delivery
                                                                </span>
                                                            )}

                                                            <div className="ml-auto flex items-center gap-1">
                                                                <Button
                                                                    type="button"
                                                                    variant="ghost"
                                                                    size="sm"
                                                                    className="rounded-xl cursor-pointer"
                                                                    disabled={
                                                                        isAddressActionPending
                                                                    }
                                                                    onClick={() =>
                                                                        openEditDialog(
                                                                            address,
                                                                        )
                                                                    }
                                                                >
                                                                    <Pencil className="mr-1.5 size-3.5" />
                                                                    Edit
                                                                </Button>

                                                                <Button
                                                                    type="button"
                                                                    variant="ghost"
                                                                    size="icon"
                                                                    aria-label={`Delete ${address.label || "saved"} address`}
                                                                    title="Delete address"
                                                                    className="size-9 rounded-xl text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                                                                    disabled={
                                                                        isAddressActionPending
                                                                    }
                                                                    onClick={() =>
                                                                        setAddressToDelete(
                                                                            address,
                                                                        )
                                                                    }
                                                                >
                                                                    <Trash2 className="size-4" />
                                                                </Button>
                                                            </div>
                                                        </div>
                                                    </CardContent>
                                                </Card>
                                            </motion.div>
                                        );
                                    },
                                )}
                            </div>
                        </section>
                    )}

                {/* Empty state */}
                {!addressesQuery.isPending &&
                    !addressesQuery.isError &&
                    addresses.length === 0 && (
                        <Card className="rounded-3xl border-dashed border-border shadow-none">
                            <CardContent className="flex flex-col items-center px-5 py-14 text-center sm:px-10 sm:py-16">
                                <div className="flex size-16 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                    <MapPinned className="size-7" />
                                </div>

                                <h2 className="mt-5 text-xl font-semibold tracking-tight">
                                    A place for your deliveries
                                </h2>

                                <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
                                    Add your home, work, or another
                                    delivery address. Your saved
                                    details make checkout easier.
                                </p>

                                <Button
                                    className="mt-6 rounded-xl"
                                    onClick={openCreateDialog}
                                >
                                    <Plus className="mr-2 size-4" />
                                    Add your first address
                                </Button>
                            </CardContent>
                        </Card>
                    )}

                {/* Reassurance */}
                <div className="mt-7 flex items-start gap-3 rounded-2xl border border-border/70 bg-muted/25 p-4 sm:p-5">
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <ShieldCheck className="size-5" />
                    </div>

                    <div>
                        <h3 className="text-sm font-medium">
                            Your delivery details stay in your account
                        </h3>

                        <p className="mt-1 text-xs leading-5 text-muted-foreground">
                            Your primary address helps Arovia select
                            your default delivery location. You can
                            change it whenever your needs change.
                        </p>
                    </div>
                </div>
            </div>

            {/* Create / edit dialog */}
            <AddressFormDialog
                open={isFormOpen}
                onOpenChange={closeFormDialog}
                address={editingAddress}
                onSaved={handleAddressSaved}
            />

            {/* Delete confirmation */}
            <Dialog
                open={Boolean(addressToDelete)}
                onOpenChange={(open) => {
                    if (!deleteAddress.isPending) {
                        if (!open) {
                            setAddressToDelete(null);
                        }
                    }
                }}
            >
                <DialogContent className="sm:max-w-md">
                    <DialogHeader>
                        <div className="mb-2 flex size-11 items-center justify-center rounded-xl bg-destructive/10 text-destructive">
                            <Trash2 className="size-5" />
                        </div>

                        <DialogTitle>
                            Delete this address?
                        </DialogTitle>

                        <DialogDescription>
                            Remove{" "}
                            <span className="font-medium text-foreground">
                                {addressToDelete?.label ||
                                    "this saved address"}
                            </span>{" "}
                            from your account? This action cannot be
                            undone.
                        </DialogDescription>
                    </DialogHeader>

                    {addressToDelete && (
                        <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                            <p className="text-sm font-medium">
                                {addressToDelete.recipientName}
                            </p>

                            <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                {formatAddress(addressToDelete)}
                            </p>
                        </div>
                    )}

                    {addressToDelete?.isPrimary && (
                        <p className="text-xs leading-5 text-muted-foreground">
                            This is your primary address. Your default
                            delivery location may change after deletion.
                        </p>
                    )}

                    <DialogFooter className="gap-2">
                        <Button
                            type="button"
                            variant="outline"
                            className="rounded-xl"
                            disabled={deleteAddress.isPending}
                            onClick={() =>
                                setAddressToDelete(null)
                            }
                        >
                            Keep address
                        </Button>

                        <Button
                            type="button"
                            variant="destructive"
                            className="rounded-xl"
                            disabled={deleteAddress.isPending}
                            onClick={() =>
                                void handleDeleteAddress()
                            }
                        >
                            {deleteAddress.isPending && (
                                <LoaderCircle className="mr-2 size-4 animate-spin" />
                            )}

                            {deleteAddress.isPending
                                ? "Deleting..."
                                : "Delete address"}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </main>
    );
}

function AddressPageSkeleton() {
    return (
        <div className="animate-pulse space-y-7">
            <div className="space-y-3">
                <div className="h-3 w-32 rounded bg-muted" />
                <div className="h-9 w-64 max-w-full rounded-lg bg-muted" />
                <div className="h-4 w-full max-w-xl rounded bg-muted" />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
                {Array.from({ length: 2 }, (_, index) => (
                    <div
                        key={index}
                        className="h-28 rounded-2xl border border-border/70 bg-card"
                    />
                ))}
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
                {Array.from({ length: 4 }, (_, index) => (
                    <div
                        key={index}
                        className="h-72 rounded-2xl border border-border/70 bg-card"
                    />
                ))}
            </div>
        </div>
    );
}