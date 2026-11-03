"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useState } from "react";

import { motion } from "framer-motion";

import {
    CheckCircle2,
    Eye,
    EyeOff,
    HeartPulse,
    ShieldCheck,
    CircleAlert,
} from "lucide-react";

import { useForm, useWatch } from "react-hook-form";

import { zodResolver } from "@hookform/resolvers/zod";

import {
    resetPasswordSchema,
    type ResetPasswordFormValues,
} from "@/lib/validation/reset-password.validations";

import type { ResetPasswordRequest } from "@/types/auth.types";
import { ApiError } from "@/lib/api/errors";
import { useResetPassword } from "@/features/auth/hooks/use-password-recovery";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface ResetPasswordFormProps {
    token?: string;
}

export function ResetPasswordForm({
    token = "",
}: ResetPasswordFormProps) {
    const queryToken = useSearchParams().get("token") ?? "";
    const resetToken = token || queryToken;
    const [showPassword, setShowPassword] =
        useState(false);

    const [isSuccess, setIsSuccess] =
        useState(false);

    const [isTokenInvalid, setIsTokenInvalid] =
        useState(false);
    const resetMutation = useResetPassword();

    const {
        register,
        handleSubmit,
        control,
        clearErrors,
        setError,
        formState: {
            errors,
            isSubmitting,
            isValid,
        },
    } = useForm<ResetPasswordFormValues>({
        resolver: zodResolver(resetPasswordSchema),
        mode: "onChange",
        defaultValues: {
            password: "",
        },
    });

    const password = useWatch({
        control,
        name: "password",
        defaultValue: "",
    });

    const hasMinimumLength =
        password.length >= 8;

    const hasUppercase =
        /[A-Z]/.test(password);

    const hasLowercase =
        /[a-z]/.test(password);

    const hasNumber =
        /\d/.test(password);

    const handleSubmitPassword = async (
        values: ResetPasswordFormValues,
    ) => {
        clearErrors("root");
        if (!resetToken) {
            setIsTokenInvalid(true);
            return;
        }

        const request: ResetPasswordRequest = {
            token: resetToken,
            newPassword: values.password,
        };

        try {
            await resetMutation.mutateAsync(request);
            setIsSuccess(true);
        } catch (error) {
            if (
                error instanceof ApiError &&
                [400, 401, 404, 410].includes(error.status ?? 0)
            ) {
                setIsTokenInvalid(true);
                return;
            }

            setError("root", {
                message:
                    error instanceof ApiError
                        ? error.message
                        : "We couldn't update your password. Please try again.",
            });
        }
    };

    const isLoading = isSubmitting || resetMutation.isPending;

    if (isTokenInvalid) {
        return (
            <InvalidTokenState />
        );
    }

    if (isSuccess) {
        return (
            <PasswordResetSuccess />
        );
    }

    return (
        <main className="min-h-[calc(100vh-4rem)] bg-background">
            <div className="container mx-auto grid min-h-[calc(100vh-4rem)] lg:grid-cols-2">
                {/* Brand / Visual Side */}
                <motion.section
                    initial={{
                        opacity: 0,
                        x: -24,
                    }}
                    animate={{
                        opacity: 1,
                        x: 0,
                    }}
                    transition={{
                        duration: 0.6,
                        ease: "easeOut",
                    }}
                    className="relative hidden overflow-hidden border-r lg:flex"
                >
                    <div className="absolute inset-0 bg-linear-to-br from-primary/10 via-background to-secondary/30" />

                    <div className="absolute -left-20 top-20 size-72 rounded-full bg-primary/10 blur-3xl" />

                    <div className="absolute -bottom-24 right-0 size-80 rounded-full bg-blue-400/10 blur-3xl" />

                    <div className="relative flex w-full flex-col justify-between p-10 xl:p-14">
                        <div className="max-w-lg">
                            <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                <HeartPulse className="size-6" />
                            </div>

                            <h1 className="mt-7 text-balance text-4xl font-semibold leading-[1.12] xl:text-5xl">
                                A fresh start
                                <br />
                                for your
                                <br />
                                <span className="text-primary">
                                    Arovia account.
                                </span>
                            </h1>

                            <p className="mt-6 max-w-md text-base leading-7 text-muted-foreground">
                                Create a new password and get back to a
                                simpler healthcare experience.
                            </p>

                            <div className="mt-8 space-y-3">
                                <Benefit text="Create a strong new password" />

                                <Benefit text="Keep your account protected" />

                                <Benefit text="Continue your Arovia journey" />
                            </div>
                        </div>

                        <p className="text-xs leading-5 text-muted-foreground">
                            Better Health. Made Simple.
                        </p>
                    </div>
                </motion.section>

                {/* Reset Panel */}
                <motion.section
                    initial={{
                        opacity: 0,
                        y: 20,
                    }}
                    animate={{
                        opacity: 1,
                        y: 0,
                    }}
                    transition={{
                        duration: 0.55,
                        delay: 0.08,
                        ease: "easeOut",
                    }}
                    className="flex items-center justify-center px-4 py-10 sm:px-6 lg:px-10"
                >
                    <div className="w-full max-w-md">
                        {/* Mobile Logo */}
                        <div className="mb-10 lg:hidden">
                            <Link
                                href="/"
                                aria-label="Arovia home"
                                className="text-2xl font-bold tracking-tight"
                            >
                                <span className="text-primary">
                                    Aro
                                </span>

                                <span className="text-foreground">
                                    via
                                </span>
                            </Link>
                        </div>

                        {/* Icon */}
                        <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                            <ShieldCheck className="size-6" />
                        </div>

                        {/* Heading */}
                        <div className="mt-7">
                            <p className="text-sm font-medium leading-6 text-primary">
                                Account recovery
                            </p>

                            <h2 className="mt-2 text-3xl font-semibold leading-tight sm:text-4xl">
                                Create a new password
                            </h2>

                            <p className="mt-4 text-sm leading-6 text-muted-foreground">
                                Choose a strong password to keep your Arovia
                                account secure.
                            </p>
                        </div>

                        {/* Form */}
                        <form
                            onSubmit={handleSubmit(
                                handleSubmitPassword,
                            )}
                            className="mt-8 space-y-5"
                            noValidate
                        >
                            <div className="space-y-2">
                                <Label
                                    htmlFor="password"
                                    className="leading-6"
                                >
                                    New password
                                </Label>

                                <div className="relative">
                                    <Input
                                        id="password"
                                        type={
                                            showPassword
                                                ? "text"
                                                : "password"
                                        }
                                        autoComplete="new-password"
                                        placeholder="Create a new password"
                                        aria-invalid={Boolean(
                                            errors.password,
                                        )}
                                        aria-describedby={
                                            errors.password
                                                ? "password-error"
                                                : "password-requirements"
                                        }
                                        className="h-12 rounded-xl pr-12"
                                        {...register("password")}
                                    />

                                    <button
                                        type="button"
                                        aria-label={
                                            showPassword
                                                ? "Hide password"
                                                : "Show password"
                                        }
                                        aria-pressed={showPassword}
                                        onClick={() =>
                                            setShowPassword(
                                                (current) =>
                                                    !current,
                                            )
                                        }
                                        className="absolute right-1 top-1/2 flex size-10 -translate-y-1/2 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-accent hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                                    >
                                        {showPassword ? (
                                            <EyeOff className="size-4" />
                                        ) : (
                                            <Eye className="size-4" />
                                        )}
                                    </button>
                                </div>

                                {errors.password && (
                                    <p
                                        id="password-error"
                                        role="alert"
                                        className="text-xs leading-5 text-destructive"
                                    >
                                        {
                                            errors.password
                                                .message
                                        }
                                    </p>
                                )}
                            </div>

                            {/* Password Requirements */}
                            <div
                                id="password-requirements"
                                className="rounded-xl border bg-muted/30 p-4"
                            >
                                <p className="text-xs font-medium leading-5 text-foreground">
                                    Password requirements
                                </p>

                                <div className="mt-3 grid gap-2 sm:grid-cols-2">
                                    <PasswordRequirement
                                        text="8+ characters"
                                        valid={
                                            hasMinimumLength
                                        }
                                    />

                                    <PasswordRequirement
                                        text="One uppercase letter"
                                        valid={
                                            hasUppercase
                                        }
                                    />

                                    <PasswordRequirement
                                        text="One lowercase letter"
                                        valid={
                                            hasLowercase
                                        }
                                    />

                                    <PasswordRequirement
                                        text="One number"
                                        valid={
                                            hasNumber
                                        }
                                    />
                                </div>
                            </div>

                            {errors.root?.message && (
                                <p
                                    role="alert"
                                    className="text-sm text-destructive"
                                >
                                    {errors.root.message}
                                </p>
                            )}

                            {/* Submit */}
                            <Button
                                type="submit"
                                disabled={
                                    !isValid ||
                                    isLoading
                                }
                                className="h-12 w-full rounded-xl text-sm font-semibold transition-colors"
                            >
                                {isLoading
                                    ? "Updating password..."
                                    : "Reset Password"}
                            </Button>
                        </form>

                        {/* Back */}
                        <div className="mt-8 flex justify-center">
                            <Link
                                href="/sign-in"
                                className="text-sm font-medium leading-6 text-muted-foreground transition-colors hover:text-foreground"
                            >
                                Back to sign in
                            </Link>
                        </div>
                    </div>
                </motion.section>
            </div>
        </main>
    );
}

function PasswordRequirement({
    text,
    valid,
}: {
    text: string;
    valid: boolean;
}) {
    return (
        <div className="flex items-center gap-2 text-xs leading-5">
            <CheckCircle2
                className={
                    valid
                        ? "size-3.5 text-primary"
                        : "size-3.5 text-muted-foreground/40"
                }
            />

            <span
                className={
                    valid
                        ? "text-primary"
                        : "text-muted-foreground"
                }
            >
                {text}
            </span>
        </div>
    );
}

function Benefit({ text }: { text: string }) {
    return (
        <div className="flex items-center gap-3 text-sm leading-6 text-muted-foreground">
            <CheckCircle2 className="size-4 shrink-0 text-primary" />

            <span>{text}</span>
        </div>
    );
}

function PasswordResetSuccess() {
    return (
        <main className="flex min-h-[calc(100vh-4rem)] items-center justify-center bg-background px-4 py-12">
            <motion.div
                initial={{
                    opacity: 0,
                    scale: 0.97,
                }}
                animate={{
                    opacity: 1,
                    scale: 1,
                }}
                transition={{
                    duration: 0.4,
                }}
                className="w-full max-w-md text-center"
            >
                <div className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                    <CheckCircle2 className="size-6" />
                </div>

                <p className="mt-7 text-sm font-medium leading-6 text-primary">
                    Password updated
                </p>

                <h1 className="mt-2 text-3xl font-semibold leading-tight sm:text-4xl">
                    You&apos;re good to go
                </h1>

                <p className="mt-4 text-sm leading-6 text-muted-foreground">
                    Your Arovia password has been updated successfully.
                    You can now sign in with your new password.
                </p>

                <Button
                    className="mt-8 h-12 w-full rounded-xl"
                >
                    <Link href="/sign-in">
                        Continue to Sign In
                    </Link>
                </Button>
            </motion.div>
        </main>
    );
}

function InvalidTokenState() {
    return (
        <main className="flex min-h-[calc(100vh-4rem)] items-center justify-center bg-background px-4 py-12">
            <motion.div
                initial={{
                    opacity: 0,
                    y: 12,
                }}
                animate={{
                    opacity: 1,
                    y: 0,
                }}
                className="w-full max-w-md text-center"
            >
                <div className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
                    <CircleAlert className="size-6" />
                </div>

                <p className="mt-7 text-sm font-medium leading-6 text-destructive">
                    Reset link unavailable
                </p>

                <h1 className="mt-2 text-3xl font-semibold leading-tight">
                    This reset link is no longer valid
                </h1>

                <p className="mt-4 text-sm leading-6 text-muted-foreground">
                    The password reset link may have expired or already
                    been used. Request a new reset link to continue.
                </p>

                <div className="mt-8 space-y-3">
                    <Button
                        className="h-12 w-full rounded-xl"
                    >
                        <Link href="/forgot-password">
                            Request New Reset Link
                        </Link>
                    </Button>

                    <Button
                        variant="ghost"
                        className="h-12 w-full rounded-xl"
                    >
                        <Link href="/sign-in">
                            Back to Sign In
                        </Link>
                    </Button>
                </div>
            </motion.div>
        </main>
    );
}
