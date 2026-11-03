"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";

import { motion } from "framer-motion";
import {
    Eye,
    EyeOff,
    Loader2,
    ShieldCheck,
} from "lucide-react";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Logo } from "@/components/brand/logo";

import {
    signInSchema,
    type SignInFormValues,
} from "@/lib/validation/sign-in.validations";

import { ApiError } from "@/lib/api/errors";
import type { SignInRequest } from "@/types/auth.types";
import { useLogin } from "@/features/auth/hooks/use-login";
import { useGoogleLogin } from "@/features/auth/hooks/use-google-login";

function getSignInDestination() {
    const returnUrl = new URLSearchParams(window.location.search).get("returnUrl");

    if (!returnUrl || !returnUrl.startsWith("/") || returnUrl.startsWith("//") || returnUrl.includes("\\")) {
        return "/";
    }

    return returnUrl;
}

export function SignInForm() {
    const router = useRouter();

    const [showPassword, setShowPassword] = useState(false);

    const loginMutation = useLogin();
    const googleLoginMutation = useGoogleLogin();

    const {
        register,
        handleSubmit,
        formState: {
            errors,
            isSubmitting,
            isValid,
        },
    } = useForm<SignInFormValues>({
        resolver: zodResolver(signInSchema),
        mode: "onChange",
        defaultValues: {
            email: "",
            password: "",
        },
    });

    const isLoading =
        isSubmitting ||
        loginMutation.isPending ||
        googleLoginMutation.isPending;

    const onSubmit = async (values: SignInFormValues) => {
        try {
            const payload: SignInRequest = {
                email: values.email.trim(),
                password: values.password,
            };

            await loginMutation.mutateAsync(payload);

            toast.success("Logged in successfully.");

            router.replace(getSignInDestination());
        } catch (error) {
            if (error instanceof ApiError) {
                toast.error(error.message);
            } else {
                toast.error("Unable to sign in. Please try again.");
            }
        }
    };

    const handleGoogleSignIn = () => {
        if (isLoading) {
            return;
        }

        googleLoginMutation.startGoogleLogin({
            onSuccess: () => {
                toast.success("Signed in with Google.");
                router.replace(getSignInDestination());
            },
            onError: (error) => {
                toast.error(
                    error instanceof Error
                        ? error.message
                        : "Unable to sign in with Google. Please try again.",
                );
            },
        });
    };

    return (
        <main className="min-h-[calc(100vh-4rem)] bg-background mt-16">
            <div className="container mx-auto grid min-h-[calc(100vh-4rem)] lg:grid-cols-2">
                {/* Brand / Visual Side */}
                <motion.section
                    initial={{ opacity: 0, x: -24 }}
                    animate={{ opacity: 1, x: 0 }}
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
                            <h1 className="mt-7 text-balance text-4xl font-semibold leading-[1.12] tracking-normal xl:text-5xl">
                                Your health.
                                <br />
                                Your care.
                                <br />
                                <span className="text-primary">
                                    Your Arovia.
                                </span>
                            </h1>

                            <p className="mt-6 max-w-md text-base leading-7 text-muted-foreground">
                                A simpler way to discover medicines, wellness
                                essentials and healthcare products for
                                everyday life.
                            </p>

                            <div className="mt-8 space-y-3">
                                <div className="flex items-center gap-3 text-sm leading-6 text-muted-foreground">
                                    <ShieldCheck className="size-4 shrink-0 text-primary" />

                                    <span>
                                        A thoughtfully designed healthcare
                                        experience
                                    </span>
                                </div>

                                <div className="flex items-center gap-3 text-sm leading-6 text-muted-foreground">
                                    <ShieldCheck className="size-4 shrink-0 text-primary" />

                                    <span>
                                        Simple and convenient access to care
                                        essentials
                                    </span>
                                </div>
                            </div>
                        </div>

                        <p className="text-xs leading-5 text-muted-foreground">
                            Better Health. Made Simple.
                        </p>
                    </div>
                </motion.section>

                {/* Sign In Panel */}
                <motion.section
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
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
                            <Logo
                                imageSize={34}
                                textClassName="text-2xl"
                            />
                        </div>

                        {/* Heading */}
                        <div>
                            <p className="text-sm font-medium leading-6 text-primary">
                                Welcome back
                            </p>

                            <h2 className="mt-2 text-3xl font-semibold leading-tight tracking-normal sm:text-4xl">
                                Sign in to Arovia
                            </h2>

                            <p className="mt-3 text-sm leading-6 text-muted-foreground">
                                Access your account, orders and healthcare
                                preferences.
                            </p>
                        </div>

                        {/* Google */}
                        <Button
                            type="button"
                            variant="outline"
                            disabled={isLoading}
                            className="mt-8 h-12 w-full rounded-xl font-medium"
                            onClick={handleGoogleSignIn}
                        >
                            {googleLoginMutation.isPending ? (
                                <>
                                    <Loader2 className="mr-2 size-4 animate-spin" />
                                    Connecting to Google...
                                </>
                            ) : (
                                <>
                                    <GoogleIcon />
                                    Continue with Google
                                </>
                            )}
                        </Button>

                        {/* Divider */}
                        <div className="my-7 flex items-center gap-4">
                            <Separator className="flex-1" />

                            <span className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                                or
                            </span>

                            <Separator className="flex-1" />
                        </div>

                        {/* Form */}
                        <form
                            onSubmit={handleSubmit(onSubmit)}
                            className="space-y-5"
                            noValidate
                        >
                            {/* Email */}
                            <div className="space-y-2.5">
                                <Label
                                    htmlFor="email"
                                    className="block leading-6"
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
                                    {...register("email")}
                                />

                                {errors.email && (
                                    <p
                                        id="email-error"
                                        role="alert"
                                        className="text-xs leading-5 text-destructive"
                                    >
                                        {errors.email.message}
                                    </p>
                                )}
                            </div>

                            {/* Password */}
                            <div className="space-y-2.5">
                                <div className="flex items-center justify-between gap-4">
                                    <Label
                                        htmlFor="password"
                                        className="block leading-6"
                                    >
                                        Password
                                    </Label>

                                    <Link
                                        href="/forgot-password"
                                        className="text-xs font-medium leading-5 text-primary transition-colors hover:text-primary/80"
                                    >
                                        Forgot password?
                                    </Link>
                                </div>

                                <div className="relative">
                                    <Input
                                        id="password"
                                        type={
                                            showPassword
                                                ? "text"
                                                : "password"
                                        }
                                        autoComplete="current-password"
                                        placeholder="Enter your password"
                                        aria-invalid={Boolean(
                                            errors.password,
                                        )}
                                        aria-describedby={
                                            errors.password
                                                ? "password-error"
                                                : undefined
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
                                                (current) => !current,
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
                                        {errors.password.message}
                                    </p>
                                )}
                            </div>

                            {/* Submit */}
                            <Button
                                type="submit"
                                disabled={!isValid || isLoading}
                                className="h-12 w-full rounded-xl text-sm font-semibold transition-colors"
                            >
                                {isLoading ? (
                                    <>
                                        <Loader2 className="mr-2 size-4 animate-spin" />
                                        Signing in...
                                    </>
                                ) : (
                                    "Sign In"
                                )}
                            </Button>
                        </form>

                        {/* Sign Up */}
                        <p className="mt-8 text-center text-sm leading-6 text-muted-foreground">
                            Don&apos;t have an account?{" "}
                            <Link
                                href="/sign-up"
                                className="font-semibold text-primary transition-colors hover:text-primary/80"
                            >
                                Create an account
                            </Link>
                        </p>
                    </div>
                </motion.section>
            </div>
        </main>
    );
}

function GoogleIcon() {
    return (
        <svg
            viewBox="0 0 24 24"
            aria-hidden="true"
            className="size-5"
        >
            <path
                fill="#4285F4"
                d="M21.35 12.27c0-.78-.07-1.53-.22-2.25H12v4.26h5.22a4.46 4.46 0 0 1-1.94 2.93v2.45h3.14c1.84-1.7 2.93-4.21 2.93-7.39Z"
            />

            <path
                fill="#34A853"
                d="M12 21.7c2.64 0 4.85-.87 6.47-2.34l-3.14-2.45c-.87.58-1.98.92-3.33.92-2.56 0-4.73-1.73-5.51-4.06H3.25v2.53A9.77 9.77 0 0 0 12 21.7Z"
            />

            <path
                fill="#FBBC05"
                d="M6.49 13.77A5.87 5.87 0 0 1 6.18 12c0-.62.11-1.22.31-1.77V7.7H3.25A9.77 9.77 0 0 0 2.23 12c0 1.57.38 3.05 1.02 4.3l3.24-2.53Z"
            />

            <path
                fill="#EA4335"
                d="M12 6.17c1.44 0 2.73.5 3.75 1.49l2.81-2.81C16.84 3.27 14.63 2.3 12 2.3a9.77 9.77 0 0 0-8.75 5.4l3.24 2.53c.78-2.33 2.95-4.06 5.51-4.06Z"
            />
        </svg>
    );
}
