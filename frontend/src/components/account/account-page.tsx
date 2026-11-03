"use client";

import { useEffect, useState, useSyncExternalStore, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import { useTheme } from "next-themes";
import { motion } from "framer-motion";
import { toast } from "sonner";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import {
    ArrowRight,
    BadgeCheck,
    BriefcaseBusiness,
    CalendarDays,
    ChevronRight,
    CircleHelp,
    CreditCard,
    House,
    KeyRound,
    LoaderCircle,
    MapPin,
    Moon,
    Plus,
    Settings2,
    ShieldCheck,
    Smartphone,
    Sun,
    Trash2,
    UserRound,
    type LucideIcon,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { useAuth } from "@/features/auth/context/auth-context";
import { useAddresses } from "@/features/addresses/hooks/use-addresses";
import { AddressFormDialog } from "@/features/addresses/components/address-form-dialog";
import type { Address } from "@/types/address.types";
import { UpdateCurrentUserRequest } from "@/types/user.types";
import { useUpdateCurrentUser } from "@/features/auth/hooks/use-current-user";

const EMPTY_ADDRESSES: Address[] = [];
const subscribe = () => () => {};
const getClientSnapshot = () => true;
const getServerSnapshot = () => false;

export type ProfileGender = "Male" | "Female" | "Other";

type ProfileFormValues = {
    firstName: string;
    lastName: string;
    phone: string;
    dateOfBirth: string;
    gender: "" | ProfileGender;
};

const profileSchema = z.object({
    firstName: z.string().trim().min(2, "Enter your first name.").max(100),
    lastName: z.string().trim().min(1, "Enter your last name.").max(100),
    phone: z
        .string()
        .trim()
        .max(20, "Phone number is too long.")
        .refine((value) => {
            if (!value) return true;
            if (!/^[+0-9().\s-]+$/.test(value)) return false;
            const digits = value.replace(/\D/g, "");
            return digits.length >= 7 && digits.length <= 15;
        }, "Enter a valid phone number."),
    dateOfBirth: z.string().refine((value) => {
        if (!value) return true;
        if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return false;

        const parsed = new Date(`${value}T00:00:00.000Z`);
        if (Number.isNaN(parsed.getTime()) || parsed.toISOString().slice(0, 10) !== value) {
            return false;
        }

        return value <= getLocalDateString();
    }, "Enter a valid date that is not in the future."),
    gender: z.enum(["", "Male", "Female", "Other"]),
});

const passwordSchema = z
    .object({
        currentPassword: z.string().min(1, "Enter your current password."),
        newPassword: z.string().min(8, "Use at least 8 characters.").max(128),
        confirmPassword: z.string().min(1, "Confirm your new password."),
    })
    .refine((values) => values.newPassword === values.confirmPassword, {
        message: "Passwords do not match.",
        path: ["confirmPassword"],
    })
    .refine((values) => values.currentPassword !== values.newPassword, {
        message: "Choose a password different from your current one.",
        path: ["newPassword"],
    });

const navigationItems = [
    { id: "personal-info", label: "Personal information", icon: UserRound },
    { id: "addresses", label: "Address book", icon: MapPin },
    { id: "security", label: "Sign-in & security", icon: ShieldCheck },
    { id: "preferences", label: "Preferences", icon: Settings2 },
] satisfies { id: string; label: string; icon: LucideIcon }[];

export interface ChangePasswordInput {
    currentPassword: string;
    newPassword: string;
}

export function AccountPage() {
    const router = useRouter();
    const {
        authStatus,
        isLoading: isAuthLoading,
        user,
        setAuthenticatedUser,
    } = useAuth();
    const isAuthenticated = !isAuthLoading && Boolean(authStatus && user);
    const addressesQuery = useAddresses(isAuthenticated);
    const addresses = addressesQuery.data ?? EMPTY_ADDRESSES;

    const mounted = useSyncExternalStore(subscribe, getClientSnapshot, getServerSnapshot);
    const [activeSection, setActiveSection] = useState("personal-info");
    const [isProfileDialogOpen, setIsProfileDialogOpen] = useState(false);
    const [isPasswordDialogOpen, setIsPasswordDialogOpen] = useState(false);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isAddressDialogOpen, setIsAddressDialogOpen] = useState(false);
    const { theme, setTheme } = useTheme();

    const fullName = [user?.firstName, user?.lastName].filter(Boolean).join(" ") || "Your account";
    const initials = [user?.firstName?.[0], user?.lastName?.[0]]
        .filter(Boolean)
        .join("")
        .toUpperCase() || "A";
    const phone = user?.phone?.trim() || null;
    const dateOfBirth = toDateInputValue(user?.dateOfBirth);
    const gender = user?.gender?.trim() || null;
    const primaryAddress = addresses.find((address) => address.isPrimary);

    const openSection = (id: string) => {
        setActiveSection(id);
        document.getElementById(id)?.scrollIntoView({
            behavior: "smooth",
            block: "start",
        });
    };

    const updateUserMutation = useUpdateCurrentUser();

    const handleUpdateProfile = async (input: UpdateCurrentUserRequest) => {
        try {
            const updatedUser = await updateUserMutation.mutateAsync(input);

            // Keep the account page in sync after the server confirms the update.
            if (user) {
                setAuthenticatedUser({
                    ...user,
                    firstName: updatedUser.firstName,
                    lastName: updatedUser.lastName,
                    phone: updatedUser.phone,
                    dateOfBirth: updatedUser.dateOfBirth,
                    gender: updatedUser.gender,
                });
            }

        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : "Could not update account data.",
            );
        }
    };

    const handleChangePassword = async (): Promise<void> => {
        throw new Error("Changing passwords is not implemented yet.");
    };

    const handleDeleteAccount = async (): Promise<void> => {
        throw new Error("Account deletion is not implemented yet.");
    };

    if (isAuthLoading) {
        return <AccountPageSkeleton />;
    }

    if (!authStatus || !user) {
        return (
            <main className="flex min-h-[70vh] items-center justify-center px-4 py-16">
                <motion.div
                    initial={{ opacity: 0, y: 12 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="w-full max-w-md rounded-3xl border border-border/70 bg-card p-8 text-center shadow-sm"
                >
                    <div className="mx-auto flex size-14 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                        <UserRound className="size-6" />
                    </div>
                    <h1 className="mt-5 text-2xl font-semibold tracking-tight">
                        Your account, all in one place
                    </h1>
                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                        Sign in to manage your personal details, delivery addresses, and account security.
                    </p>
                    <Button className="mt-6 h-11 w-full rounded-xl" onClick={() => router.push("/sign-in")}>
                        Sign in to Arovia <ArrowRight className="ml-2 size-4" />
                    </Button>
                </motion.div>
            </main>
        );
    }

    return (
        <main className="min-h-screen bg-background my-32">
            <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                <motion.header
                    initial={{ opacity: 0, y: 12 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.35, ease: "easeOut" }}
                    className="relative isolate overflow-hidden rounded-[2rem] border border-primary/10 bg-gradient-to-br from-primary/[0.09] via-card to-sky-500/[0.06] p-6 sm:p-9"
                >
                    <div aria-hidden="true" className="pointer-events-none absolute -right-12 -top-20 -z-10 size-64 rounded-full bg-primary/10 blur-3xl" />
                    <div aria-hidden="true" className="pointer-events-none absolute -bottom-28 right-1/3 -z-10 size-56 rounded-full bg-sky-500/10 blur-3xl" />
                    <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex items-center gap-4 sm:gap-5">
                            <div className="flex size-16 shrink-0 items-center justify-center rounded-2xl border border-white/50 bg-background/80 text-xl font-semibold text-primary shadow-sm backdrop-blur sm:size-[76px] sm:text-2xl">
                                {initials}
                            </div>
                            <div className="min-w-0">
                                <div className="mb-2 flex flex-wrap items-center gap-2">
                                    <Badge variant="secondary" className="rounded-full border border-primary/10 bg-primary/10 text-primary">
                                        <BadgeCheck className="mr-1 size-3.5" /> My account
                                    </Badge>
                                </div>
                                <h1 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
                                    Welcome back, {user.firstName || "there"}
                                </h1>
                                <p className="mt-1 max-w-xl text-sm leading-6 text-muted-foreground">
                                    Manage your details and keep your Arovia experience up to date.
                                </p>
                            </div>
                        </div>
                        <div className="flex flex-wrap gap-3">
                            <Button variant="outline" className="rounded-xl bg-background/70 cursor-pointer" onClick={() => router.push("/orders")}>
                                <CreditCard className="mr-2 size-4" /> Your orders
                            </Button>
                            <Button className="rounded-xl cursor-pointer" onClick={() => setIsProfileDialogOpen(true)}>
                                Edit profile <ChevronRight className="ml-1 size-4" />
                            </Button>
                        </div>
                    </div>
                </motion.header>

                <div className="mt-7 grid gap-7 lg:grid-cols-[240px_minmax(0,1fr)] lg:items-start">
                    <aside className="lg:sticky lg:top-24">
                        <div className="rounded-2xl border border-border/70 bg-card p-3 shadow-sm">
                            <p className="px-3 pb-3 pt-2 text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
                                Account settings
                            </p>
                            <nav aria-label="Account sections" className="flex gap-1 overflow-x-auto lg:flex-col lg:overflow-visible">
                                {navigationItems.map((item) => {
                                    const Icon = item.icon;
                                    const isActive = activeSection === item.id;
                                    return (
                                        <button
                                            key={item.id}
                                            type="button"
                                            onClick={() => openSection(item.id)}
                                            className={[
                                                "flex shrink-0 items-center gap-3 rounded-xl px-3 py-3 text-left text-sm transition-colors cursor-pointer",
                                                "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                                                isActive
                                                    ? "bg-primary/10 font-medium text-primary"
                                                    : "text-muted-foreground hover:bg-muted/70 hover:text-foreground",
                                            ].join(" ")}
                                        >
                                            <Icon className="size-4 shrink-0" />
                                            <span>{item.label}</span>
                                            {isActive && <ChevronRight className="ml-auto hidden size-4 lg:block" />}
                                        </button>
                                    );
                                })}
                            </nav>
                            <div className="mx-2 my-3 border-t border-border/70" />
                            <button
                                type="button"
                                onClick={() => router.push("/contact")}
                                className="flex w-full items-center gap-3 rounded-xl px-3 py-3 text-sm text-muted-foreground transition-colors hover:bg-muted/70 hover:text-foreground cursor-pointer"
                            >
                                <CircleHelp className="size-4" /> Help &amp; support <ArrowRight className="ml-auto size-4" />
                            </button>
                        </div>

                        <div className="mt-4 rounded-2xl border border-emerald-500/15 bg-emerald-500/4 p-4">
                            <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
                                <ShieldCheck className="size-4 text-emerald-600 dark:text-emerald-400" /> Your privacy matters
                            </div>
                            <p className="mt-2 text-xs leading-5 text-muted-foreground">
                                Your personal information is used to support your account and healthcare orders.
                            </p>
                        </div>
                    </aside>

                    <div className="min-w-0 space-y-7">
                        <section id="personal-info" className="scroll-mt-28">
                            <SectionHeading
                                eyebrow="Your details"
                                title="Personal information"
                                description="The information associated with your Arovia account."
                                action={
                                    <Button variant="ghost" size="sm" className="rounded-lg" onClick={() => setIsProfileDialogOpen(true)}>
                                        Edit details <ChevronRight className="ml-1 size-4" />
                                    </Button>
                                }
                            />
                            <div className="mt-4 grid gap-4 sm:grid-cols-2">
                                <InfoCard icon={UserRound} label="Full name" value={fullName} />
                                <InfoCard
                                    icon={Smartphone}
                                    label="Email address"
                                    value={user.email || "Not available"}
                                    helper="Used for account sign-in and important updates."
                                />
                                {phone && <InfoCard icon={Smartphone} label="Phone number" value={phone} />}
                                {dateOfBirth && (
                                    <InfoCard
                                        icon={CalendarDays}
                                        label="Date of birth"
                                        value={formatDateOfBirth(dateOfBirth)}
                                    />
                                )}
                                {gender && <InfoCard icon={UserRound} label="Gender" value={formatGender(gender)} />}
                                <InfoCard
                                    icon={ShieldCheck}
                                    label="Account status"
                                    value="Active account"
                                    helper="Your Arovia account is ready to use."
                                />
                                <InfoCard
                                    icon={MapPin}
                                    label="Default delivery location"
                                    value={primaryAddress ? `${primaryAddress.city}, ${primaryAddress.state}` : "No primary address"}
                                    helper={primaryAddress ? `Pincode ${primaryAddress.postalCode}` : "Add an address to make checkout faster."}
                                />
                            </div>
                        </section>

                        <section id="addresses" className="scroll-mt-28">
                            <SectionHeading
                                eyebrow="Delivery details"
                                title="Address book"
                                description="Choose and manage where your orders should arrive."
                                action={
                                    <Button variant="outline" size="sm" className="rounded-xl cursor-pointer" onClick={() => setIsAddressDialogOpen(true)}>
                                        <Plus className="mr-1.5 size-4" /> Add address
                                    </Button>
                                }
                            />
                            <div className="mt-4 rounded-2xl border border-border/70 bg-card p-4 shadow-sm sm:p-5">
                                {addressesQuery.isPending ? (
                                    <InlineLoading label="Loading your saved addresses…" />
                                ) : addressesQuery.isError ? (
                                    <InlineError
                                        message="We couldn't load your saved addresses."
                                        onRetry={() => void addressesQuery.refetch()}
                                    />
                                ) : addresses.length === 0 ? (
                                    <div className="flex flex-col items-center rounded-xl border border-dashed border-border px-5 py-10 text-center">
                                        <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                            <MapPin className="size-5" />
                                        </div>
                                        <h3 className="mt-4 text-sm font-semibold">Your address book is empty</h3>
                                        <p className="mt-1 max-w-sm text-sm leading-6 text-muted-foreground">
                                            Add your home or work address to make your next checkout quicker.
                                        </p>
                                        <Button className="mt-4 rounded-xl cursor-pointer" onClick={() => setIsAddressDialogOpen(true)}>
                                            <Plus className="mr-2 size-4" /> Add your first address
                                        </Button>
                                    </div>
                                ) : (
                                    <div className="divide-y divide-border/70">
                                        {addresses.map((address) => <AddressRow key={address.id} address={address} />)}
                                        <div className="pt-4">
                                            <Button
                                                variant="ghost"
                                                className="w-full justify-between rounded-xl text-muted-foreground hover:text-foreground"
                                                onClick={() => router.push("/addresses")}
                                            >
                                                Manage all addresses <ArrowRight className="size-4" />
                                            </Button>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </section>

                        <section id="security" className="scroll-mt-28">
                            <SectionHeading
                                eyebrow="Stay protected"
                                title="Sign-in & security"
                                description="Keep your account credentials private and up to date."
                            />
                            <div className="mt-4 overflow-hidden rounded-2xl border border-border/70 bg-card shadow-sm">
                                <ActionRow
                                    icon={KeyRound}
                                    title="Password"
                                    description="Change your password periodically to help protect your account."
                                    actionLabel="Change password"
                                    onAction={() => setIsPasswordDialogOpen(true)}
                                />
                                <div className="mx-5 border-t border-border/70 sm:mx-6" />
                                <ActionRow
                                    icon={ShieldCheck}
                                    title="Account protection"
                                    description="Never share your password or one-time verification codes with anyone."
                                    actionLabel="Security tips"
                                    onAction={() => toast.info("Use a unique password and never share verification codes.")}
                                />
                            </div>
                        </section>

                        <section id="preferences" className="scroll-mt-28">
                            <SectionHeading
                                eyebrow="Make it yours"
                                title="Preferences"
                                description="Choose how Arovia looks on your device."
                            />
                            <div className="mt-4 rounded-2xl border border-border/70 bg-card p-5 shadow-sm sm:p-6">
                                <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                                    <div className="flex items-start gap-3">
                                        <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                            <Sun className="size-5" />
                                        </div>
                                        <div>
                                            <h3 className="text-sm font-semibold">Appearance</h3>
                                            <p className="mt-1 text-sm text-muted-foreground">Select a theme that feels comfortable to you.</p>
                                        </div>
                                    </div>
                                    <div className="inline-flex w-full rounded-xl border border-border/70 bg-muted/50 p-1 sm:w-auto" role="group" aria-label="Theme preference">
                                        <ThemeButton icon={Sun} label="Light" value="light" currentTheme={mounted ? theme : undefined} onSelect={setTheme} />
                                        <ThemeButton icon={Moon} label="Dark" value="dark" currentTheme={mounted ? theme : undefined} onSelect={setTheme} />
                                        <ThemeButton icon={Settings2} label="System" value="system" currentTheme={mounted ? theme : undefined} onSelect={setTheme} />
                                    </div>
                                </div>
                            </div>
                        </section>

                        <section className="rounded-2xl border border-destructive/20 bg-destructive/2.5 p-5 sm:p-6">
                            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                                <div className="flex items-start gap-3">
                                    <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-destructive/10 text-destructive">
                                        <Trash2 className="size-5" />
                                    </div>
                                    <div>
                                        <h2 className="text-sm font-semibold">Delete account</h2>
                                        <p className="mt-1 max-w-xl text-sm leading-6 text-muted-foreground">
                                            Permanently remove your account. This action may affect your saved addresses, account access, and order history.
                                        </p>
                                    </div>
                                </div>
                                <Button
                                    variant="outline"
                                    className="shrink-0 rounded-xl border-destructive/30 text-destructive hover:bg-destructive/10 hover:text-destructive"
                                    onClick={() => setIsDeleteDialogOpen(true)}
                                >
                                    Delete account <ChevronRight className="ml-1 size-4" />
                                </Button>
                            </div>
                        </section>
                    </div>
                </div>
            </div>

            <ProfileDialog
                open={isProfileDialogOpen}
                onOpenChange={setIsProfileDialogOpen}
                firstName={user.firstName ?? ""}
                lastName={user.lastName ?? ""}
                phone={phone ?? ""}
                dateOfBirth={dateOfBirth ?? ""}
                gender={gender ?? ""}
                onSave={handleUpdateProfile}
            />
            <ChangePasswordDialog
                open={isPasswordDialogOpen}
                onOpenChange={setIsPasswordDialogOpen}
                onSave={handleChangePassword}
            />
            <DeleteAccountDialog
                open={isDeleteDialogOpen}
                onOpenChange={setIsDeleteDialogOpen}
                onDelete={handleDeleteAccount}
            />
            <AddressFormDialog
                open={isAddressDialogOpen}
                onOpenChange={setIsAddressDialogOpen}
                onCreated={() => {
                    void addressesQuery.refetch();
                }}
            />
        </main>
    );
}

function getLocalDateString() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function toDateInputValue(value?: string | null) {
    if (!value) return "";
    const date = value.slice(0, 10);
    return /^\d{4}-\d{2}-\d{2}$/.test(date) ? date : "";
}

function formatDateOfBirth(value: string) {
    const dateValue = toDateInputValue(value);
    if (!dateValue) return value;

    const date = new Date(`${dateValue}T00:00:00.000Z`);
    if (Number.isNaN(date.getTime())) return value;

    return new Intl.DateTimeFormat("en-IN", {
        day: "numeric",
        month: "long",
        year: "numeric",
        timeZone: "UTC",
    }).format(date);
}

function formatGender(value: string) {
    switch (value.trim().toLowerCase()) {
        case "male":
            return "Male";
        case "female":
            return "Female";
        case "other":
            return "Other";
        default:
            return value;
    }
}

function normalizeGender(value: string): ProfileFormValues["gender"] {
    switch (value.trim().toLowerCase()) {
        case "male":
            return "Male";
        case "female":
            return "Female";
        case "other":
            return "Other";
        default:
            return "";
    }
}

function SectionHeading({
    eyebrow,
    title,
    description,
    action,
}: {
    eyebrow: string;
    title: string;
    description: string;
    action?: ReactNode;
}) {
    return (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
                <p className="text-xs font-semibold uppercase tracking-[0.12em] text-primary">{eyebrow}</p>
                <h2 className="mt-1.5 text-xl font-semibold tracking-tight text-foreground sm:text-2xl">{title}</h2>
                <p className="mt-1.5 text-sm leading-6 text-muted-foreground">{description}</p>
            </div>
            {action}
        </div>
    );
}

function InfoCard({
    icon: Icon,
    label,
    value,
    helper,
}: {
    icon: LucideIcon;
    label: string;
    value: string;
    helper?: string;
}) {
    return (
        <div className="rounded-2xl border border-border/70 bg-card p-4 transition-colors hover:border-primary/20 sm:p-5">
            <div className="flex items-center gap-2 text-muted-foreground">
                <Icon className="size-4" />
                <p className="text-xs font-medium">{label}</p>
            </div>
            <p className="mt-3 wrap-break-word text-sm font-semibold text-foreground">{value}</p>
            {helper && <p className="mt-1.5 text-xs leading-5 text-muted-foreground">{helper}</p>}
        </div>
    );
}

function AddressRow({ address }: { address: Address }) {
    const normalizedLabel = address.label.trim().toLowerCase();
    const Icon = normalizedLabel === "home"
        ? House
        : normalizedLabel === "work"
            ? BriefcaseBusiness
            : MapPin;

    return (
        <div className="flex gap-3 py-4 first:pt-0 last:pb-0 sm:gap-4">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                <Icon className="size-4.5" />
            </div>
            <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2">
                    <h3 className="text-sm font-semibold">{address.label || "Address"}</h3>
                    {address.isPrimary && (
                        <Badge variant="secondary" className="rounded-full bg-primary/10 text-primary">
                            Primary
                        </Badge>
                    )}
                </div>
                <p className="mt-1 text-sm font-medium text-foreground">{address.recipientName}</p>
                <p className="mt-1 text-sm leading-6 text-muted-foreground">
                    {address.addressLine1}{address.addressLine2 ? `, ${address.addressLine2}` : ""}<br />
                    {address.city}, {address.state} {address.postalCode}<br />
                    {address.country}
                </p>
                {address.recipientPhone && <p className="mt-1 text-xs text-muted-foreground">{address.recipientPhone}</p>}
            </div>
        </div>
    );
}

function ActionRow({
    icon: Icon,
    title,
    description,
    actionLabel,
    onAction,
}: {
    icon: LucideIcon;
    title: string;
    description: string;
    actionLabel: string;
    onAction: () => void;
}) {
    return (
        <div className="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:gap-4 sm:p-6">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-muted text-foreground">
                <Icon className="size-5" />
            </div>
            <div className="min-w-0 flex-1">
                <h3 className="text-sm font-semibold">{title}</h3>
                <p className="mt-1 text-sm leading-6 text-muted-foreground">{description}</p>
            </div>
            <Button variant="outline" className="shrink-0 rounded-xl cursor-pointer" onClick={onAction}>
                {actionLabel}<ChevronRight className="ml-1 size-4" />
            </Button>
        </div>
    );
}

function ThemeButton({
    icon: Icon,
    label,
    value,
    currentTheme,
    onSelect,
}: {
    icon: LucideIcon;
    label: string;
    value: "light" | "dark" | "system";
    currentTheme?: string;
    onSelect: (theme: string) => void;
}) {
    const selected = currentTheme === value;

    return (
        <button
            type="button"
            aria-pressed={selected}
            onClick={() => onSelect(value)}
            className={[
                "flex flex-1 items-center justify-center gap-2 rounded-lg px-3 py-2 text-xs font-medium transition-colors sm:flex-none cursor-pointer",
                selected ? "bg-background text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
            ].join(" ")}
        >
            <Icon className="size-3.5" /> {label}
        </button>
    );
}

function ProfileDialog({
    open,
    onOpenChange,
    firstName,
    lastName,
    phone,
    dateOfBirth,
    gender,
    onSave,
}: {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    firstName: string;
    lastName: string;
    phone: string;
    dateOfBirth: string;
    gender: string;
    onSave?: (input: UpdateCurrentUserRequest) => Promise<void>;
}) {
    const [isSaving, setIsSaving] = useState(false);
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<ProfileFormValues>({
        resolver: zodResolver(profileSchema),
        defaultValues: {
            firstName,
            lastName,
            phone,
            dateOfBirth: toDateInputValue(dateOfBirth),
            gender: normalizeGender(gender),
        },
    });

    useEffect(() => {
        if (open) {
            reset({
                firstName,
                lastName,
                phone,
                dateOfBirth: toDateInputValue(dateOfBirth),
                gender: normalizeGender(gender),
            });
        }
    }, [open, firstName, lastName, phone, dateOfBirth, gender, reset]);

    const submit = handleSubmit(async (values) => {
        if (!onSave) {
            toast.info("Profile updates aren't connected yet.");
            return;
        }

        setIsSaving(true);
        try {
            await onSave({
                firstName: values.firstName.trim(),
                lastName: values.lastName.trim(),
                phone: values.phone.trim() || null,
                dateOfBirth: values.dateOfBirth || null,
                gender: values.gender || null,
            });
            toast.success("Your profile has been updated.");
            onOpenChange(false);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not update your profile.");
        } finally {
            setIsSaving(false);
        }
    });

    return (
        <Dialog open={open} onOpenChange={(next) => !isSaving && onOpenChange(next)}>
            <DialogContent className="sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle>Edit personal information</DialogTitle>
                    <DialogDescription>Keep your account details accurate. Optional details can be left blank.</DialogDescription>
                </DialogHeader>
                <form onSubmit={submit} className="mt-2 space-y-4">
                    <div className="grid gap-4 sm:grid-cols-2">
                        <FormField id="account-first-name" label="First name" error={errors.firstName?.message}>
                            <Input id="account-first-name" autoComplete="given-name" {...register("firstName")} />
                        </FormField>
                        <FormField id="account-last-name" label="Last name" error={errors.lastName?.message}>
                            <Input id="account-last-name" autoComplete="family-name" {...register("lastName")} />
                        </FormField>
                        <FormField id="account-phone" label="Phone number" error={errors.phone?.message}>
                            <Input
                                id="account-phone"
                                type="tel"
                                inputMode="tel"
                                autoComplete="tel"
                                placeholder="+91 98765 43210"
                                {...register("phone")}
                            />
                        </FormField>
                        <FormField id="account-date-of-birth" label="Date of birth" error={errors.dateOfBirth?.message}>
                            <Input
                                id="account-date-of-birth"
                                type="date"
                                max={getLocalDateString()}
                                {...register("dateOfBirth")}
                            />
                        </FormField>
                        <FormField id="account-gender" label="Gender" error={errors.gender?.message}>
                            <select
                                id="account-gender"
                                {...register("gender")}
                                className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-xs outline-none transition-[color,box-shadow] focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50"
                            >
                                <option value="">Prefer not to say</option>
                                <option value="Female">Female</option>
                                <option value="Male">Male</option>
                                <option value="Other">Other</option>
                            </select>
                        </FormField>
                    </div>
                    {!onSave && (
                        <p className="text-xs leading-5 text-muted-foreground">
                            The profile update mutation has not been passed to this page, so saving is disabled.
                        </p>
                    )}
                    <DialogFooter className="gap-2 sm:gap-2">
                        <Button type="button" variant="outline" disabled={isSaving} onClick={() => onOpenChange(false)}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={isSaving || !onSave}>
                            {isSaving && <LoaderCircle className="mr-2 size-4 animate-spin" />}
                            Save changes
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

function ChangePasswordDialog({
    open,
    onOpenChange,
    onSave,
}: {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSave?: (input: ChangePasswordInput) => Promise<void>;
}) {
    const [isSaving, setIsSaving] = useState(false);
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<z.infer<typeof passwordSchema>>({
        resolver: zodResolver(passwordSchema),
        defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
    });

    useEffect(() => {
        if (open) reset({ currentPassword: "", newPassword: "", confirmPassword: "" });
    }, [open, reset]);

    const submit = handleSubmit(async (values) => {
        if (!onSave) {
            toast.info("Password change isn't connected yet.");
            return;
        }

        setIsSaving(true);
        try {
            await onSave({ currentPassword: values.currentPassword, newPassword: values.newPassword });
            toast.success("Your password has been changed.");
            reset({ currentPassword: "", newPassword: "", confirmPassword: "" });
            onOpenChange(false);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not change your password.");
        } finally {
            setIsSaving(false);
        }
    });

    return (
        <Dialog open={open} onOpenChange={(next) => !isSaving && onOpenChange(next)}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>Change password</DialogTitle>
                    <DialogDescription>Use a strong, unique password that you don&apos;t use elsewhere.</DialogDescription>
                </DialogHeader>
                <form onSubmit={submit} className="mt-2 space-y-4">
                    <FormField id="current-password" label="Current password" error={errors.currentPassword?.message}>
                        <Input id="current-password" type="password" autoComplete="current-password" {...register("currentPassword")} />
                    </FormField>
                    <FormField id="new-password" label="New password" error={errors.newPassword?.message}>
                        <Input id="new-password" type="password" autoComplete="new-password" {...register("newPassword")} />
                    </FormField>
                    <FormField id="confirm-password" label="Confirm new password" error={errors.confirmPassword?.message}>
                        <Input id="confirm-password" type="password" autoComplete="new-password" {...register("confirmPassword")} />
                    </FormField>
                    {!onSave && (
                        <p className="text-xs leading-5 text-muted-foreground">
                            The password-change mutation has not been passed to this page, so saving is disabled.
                        </p>
                    )}
                    <DialogFooter className="gap-2 sm:gap-2">
                        <Button type="button" variant="outline" disabled={isSaving} onClick={() => onOpenChange(false)}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={isSaving || !onSave}>
                            {isSaving && <LoaderCircle className="mr-2 size-4 animate-spin" />}
                            Update password
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

function DeleteAccountDialog({
    open,
    onOpenChange,
    onDelete,
}: {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onDelete?: () => Promise<void>;
}) {
    const [confirmation, setConfirmation] = useState("");
    const [isDeleting, setIsDeleting] = useState(false);

    const canDelete = confirmation === "DELETE" && Boolean(onDelete) && !isDeleting;

    const handleDelete = async () => {
        if (!onDelete || confirmation !== "DELETE") return;
        setIsDeleting(true);
        try {
            await onDelete();
            toast.success("Your account deletion request has been processed.");
            onOpenChange(false);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not delete your account.");
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={(next) => { if (!isDeleting) { if (next) setConfirmation(""); onOpenChange(next); } }}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <div className="mb-2 flex size-11 items-center justify-center rounded-xl bg-destructive/10 text-destructive">
                        <Trash2 className="size-5" />
                    </div>
                    <DialogTitle>Delete your Arovia account?</DialogTitle>
                    <DialogDescription>
                        This is a serious action. You may lose access to saved addresses, account settings, and other account-related data. Review any outstanding orders before proceeding.
                    </DialogDescription>
                </DialogHeader>
                <div className="space-y-2 py-2">
                    <Label htmlFor="delete-account-confirmation">
                        Type <span className="font-semibold text-foreground">DELETE</span> to confirm
                    </Label>
                    <Input
                        id="delete-account-confirmation"
                        value={confirmation}
                        onChange={(event) => setConfirmation(event.target.value)}
                        autoComplete="off"
                        placeholder="DELETE"
                    />
                    {!onDelete && (
                        <p className="text-xs leading-5 text-muted-foreground">
                            The account-deletion mutation has not been passed to this page, so deletion is disabled.
                        </p>
                    )}
                </div>
                <DialogFooter className="gap-2 sm:gap-2">
                    <Button type="button" variant="outline" disabled={isDeleting} onClick={() => onOpenChange(false)}>
                        Keep my account
                    </Button>
                    <Button type="button" variant="destructive" disabled={!canDelete} onClick={() => void handleDelete()}>
                        {isDeleting && <LoaderCircle className="mr-2 size-4 animate-spin" />}
                        Delete account
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

function FormField({
    id,
    label,
    error,
    children,
}: {
    id: string;
    label: string;
    error?: string;
    children: ReactNode;
}) {
    return (
        <div className="space-y-1.5">
            <Label htmlFor={id}>{label}</Label>
            {children}
            {error && <p role="alert" className="text-xs text-destructive">{error}</p>}
        </div>
    );
}

function InlineLoading({ label }: { label: string }) {
    return (
        <div role="status" className="flex items-center justify-center gap-2 py-8 text-sm text-muted-foreground">
            <LoaderCircle className="size-4 animate-spin" /> {label}
        </div>
    );
}

function InlineError({ message, onRetry }: { message: string; onRetry: () => void }) {
    return (
        <div className="flex flex-col items-start gap-3 py-4 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-sm text-muted-foreground">{message}</p>
            <Button type="button" variant="outline" size="sm" onClick={onRetry}>Try again</Button>
        </div>
    );
}

function AccountPageSkeleton() {
    return (
        <main className="min-h-screen bg-background pb-16 pt-24">
            <div className="mx-auto max-w-7xl animate-pulse px-4 sm:px-6 lg:px-8">
                <div className="h-48 rounded-[2rem] border border-border/70 bg-muted/60" />
                <div className="mt-7 grid gap-7 lg:grid-cols-[240px_minmax(0,1fr)]">
                    <div className="h-72 rounded-2xl bg-muted/60" />
                    <div className="space-y-5">
                        <div className="h-36 rounded-2xl bg-muted/60" />
                        <div className="h-64 rounded-2xl bg-muted/60" />
                        <div className="h-48 rounded-2xl bg-muted/60" />
                    </div>
                </div>
            </div>
        </main>
    );
}
