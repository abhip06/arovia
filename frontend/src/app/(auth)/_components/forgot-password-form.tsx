"use client";

import Link from "next/link";

import { motion } from "framer-motion";

import {
    ArrowLeft,
    CheckCircle2,
    Mail,
    ShieldCheck,
} from "lucide-react";

import { useState } from "react";

import { useForm } from "react-hook-form";

import { zodResolver } from "@hookform/resolvers/zod";

import {
    forgotPasswordSchema,
    type ForgotPasswordFormValues,
} from "@/lib/validation/forgot-password.validations";

import type {
    RequestPasswordResetRequest,
} from "@/types/auth.types";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ApiError } from "@/lib/api/errors";
import { useRequestPasswordReset } from "@/features/auth/hooks/use-password-recovery";

export function ForgotPasswordForm() {
    const [isSuccess, setIsSuccess] =
        useState(false);
    const requestMutation = useRequestPasswordReset();

    const {
        register,
        handleSubmit,
        clearErrors,
        setError,
        formState: {
            errors,
            isSubmitting,
            isValid,
        },
    } = useForm<ForgotPasswordFormValues>({
        resolver: zodResolver(
            forgotPasswordSchema,
        ),
        mode: "onChange",
        defaultValues: {
            email: "",
        },
    });

    const onSubmit = async (
        values: ForgotPasswordFormValues,
    ) => {
        clearErrors("root");
        const request: RequestPasswordResetRequest = {
            email: values.email.trim(),
        };

        try {
            await requestMutation.mutateAsync(request);
            setIsSuccess(true);
        } catch (error) {
            setError("root", {
                message:
                    error instanceof ApiError
                        ? error.message
                        : "We couldn't send reset instructions. Please try again.",
            });
        }
    };

    const isLoading = isSubmitting || requestMutation.isPending;

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
                                <ShieldCheck className="size-6" />
                            </div>

                            <h1 className="mt-7 text-balance text-4xl font-semibold leading-[1.12] xl:text-5xl">
                                Your account.
                                <br />
                                Your security.
                                <br />
                                <span className="text-primary">
                                    Your peace of mind.
                                </span>
                            </h1>

                            <p className="mt-6 max-w-md text-base leading-7 text-muted-foreground">
                                We&apos;ll help you securely get back into your
                                Arovia account and continue your healthcare
                                journey.
                            </p>

                            <div className="mt-8 space-y-3">
                                <Benefit text="Secure password recovery" />

                                <Benefit text="Private account protection" />

                                <Benefit text="Simple and guided recovery" />
                            </div>
                        </div>

                        <p className="text-xs leading-5 text-muted-foreground">
                            Better Health. Made Simple.
                        </p>
                    </div>
                </motion.section>

                {/* Form Side */}
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

                        {!isSuccess ? (
                            <>
                                {/* Icon */}
                                <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                                    <Mail className="size-6" />
                                </div>

                                {/* Heading */}
                                <div className="mt-7">
                                    <p className="text-sm font-medium leading-6 text-primary">
                                        Account recovery
                                    </p>

                                    <h2 className="mt-2 text-3xl font-semibold leading-tight sm:text-4xl">
                                        Forgot your password?
                                    </h2>

                                    <p className="mt-4 text-sm leading-6 text-muted-foreground">
                                        Enter the email address associated
                                        with your Arovia account. If an account
                                        exists, we&apos;ll send password reset
                                        instructions.
                                    </p>
                                </div>

                                {/* Form */}
                                <form
                                    onSubmit={handleSubmit(
                                        onSubmit,
                                    )}
                                    className="mt-8 space-y-5"
                                    noValidate
                                >
                                    <div className="space-y-2">
                                        <Label
                                            htmlFor="email"
                                            className="leading-6"
                                        >
                                            Email
                                        </Label>

                                        <Input
                                            id="email"
                                            type="email"
                                            inputMode="email"
                                            autoComplete="email"
                                            placeholder="Enter your email address"
                                            aria-invalid={Boolean(
                                                errors.email,
                                            )}
                                            aria-describedby={
                                                errors.email
                                                    ? "email-error"
                                                    : undefined
                                            }
                                            className="h-12 rounded-xl"
                                            {...register(
                                                "email",
                                            )}
                                        />

                                        {errors.email && (
                                            <p
                                                id="email-error"
                                                role="alert"
                                                className="text-xs leading-5 text-destructive"
                                            >
                                                {
                                                    errors
                                                        .email
                                                        .message
                                                }
                                            </p>
                                        )}
                                    </div>

                                    <Button
                                        type="submit"
                                        disabled={
                                            !isValid ||
                                            isLoading
                                        }
                                        className="h-12 w-full rounded-xl text-sm font-semibold transition-colors"
                                    >
                                        {isLoading
                                            ? "Sending instructions..."
                                            : "Send Reset Instructions"}
                                    </Button>

                                    {errors.root?.message && (
                                        <p
                                            role="alert"
                                            className="text-sm text-destructive"
                                        >
                                            {errors.root.message}
                                        </p>
                                    )}
                                </form>

                                {/* Back to Sign In */}
                                <div className="mt-8 flex justify-center">
                                    <Link
                                        href="/sign-in"
                                        className="inline-flex items-center gap-2 text-sm font-medium leading-6 text-muted-foreground transition-colors hover:text-foreground"
                                    >
                                        <ArrowLeft className="size-4" />
                                        Back to sign in
                                    </Link>
                                </div>

                                {/* Security note */}
                                <div className="mt-10 flex items-start gap-3 rounded-xl border bg-muted/30 p-4">
                                    <ShieldCheck className="mt-0.5 size-4 shrink-0 text-primary" />

                                    <p className="text-xs leading-5 text-muted-foreground">
                                        For your security, Arovia will not
                                        reveal whether an email address is
                                        registered with an account.
                                    </p>
                                </div>
                            </>
                        ) : (
                            <SuccessState />
                        )}
                    </div>
                </motion.section>
            </div>
        </main>
    );
}

function SuccessState() {
    return (
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
                ease: "easeOut",
            }}
        >
            <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                <CheckCircle2 className="size-6" />
            </div>

            <div className="mt-7">
                <p className="text-sm font-medium leading-6 text-primary">
                    Check your inbox
                </p>

                <h2 className="mt-2 text-3xl font-semibold leading-tight sm:text-4xl">
                    Reset instructions sent
                </h2>

                <p className="mt-4 text-sm leading-6 text-muted-foreground">
                    If an Arovia account exists for that email address,
                    we&apos;ve sent instructions to reset your password.
                    Please check your inbox and spam folder.
                </p>
            </div>

            <div className="mt-8 rounded-xl border bg-muted/30 p-4">
                <div className="flex items-start gap-3">
                    <ShieldCheck className="mt-0.5 size-4 shrink-0 text-primary" />

                    <p className="text-xs leading-5 text-muted-foreground">
                        Password reset links should only be used by the
                        account owner. Never share a reset link with anyone
                        else.
                    </p>
                </div>
            </div>

            <Link
                href="/sign-in"
                className="mt-8 inline-flex h-12 w-full items-center justify-center rounded-xl bg-primary px-6 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90"
            >
                Back to Sign In
            </Link>
        </motion.div>
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
