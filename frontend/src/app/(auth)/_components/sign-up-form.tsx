"use client";

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { motion } from "framer-motion";

import {
    CheckCircle2,
    Eye,
    EyeOff,
    Loader2,
} from "lucide-react";

import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import {
    signUpSchema,
    type SignUpFormValues,
} from "@/lib/validation/sign-up.validations";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";

import type { SignUpRequest } from "@/types/auth.types";
import { ApiError } from "@/lib/api/errors";
import { useSignUp } from "@/features/auth/hooks/use-sign-up";
import { useGoogleLogin } from "@/features/auth/hooks/use-google-login";

export function SignUpForm() {
    const router = useRouter();
    const [showPassword, setShowPassword] = useState(false);
    const signUpMutation = useSignUp();
    const googleLoginMutation = useGoogleLogin();

    const {
        register,
        handleSubmit,
        control,
        setValue,
        formState: {
            errors,
            isSubmitting,
            isValid,
        },
    } = useForm<SignUpFormValues>({
        resolver: zodResolver(signUpSchema),
        mode: "onChange",
        defaultValues: {
            firstName: "",
            lastName: "",
            email: "",
            password: "",
            terms: false,
        },
    });

    const termsAccepted = useWatch({
        control,
        name: "terms",
        defaultValue: false,
    });

    const onSubmit = async (values: SignUpFormValues) => {
        const payload: SignUpRequest = {
            firstName: values.firstName.trim(),
            lastName: values.lastName.trim(),
            email: values.email.trim(),
            password: values.password,
        };

        try {
            await signUpMutation.mutateAsync(payload);
            router.push(
                `/verify-email?email=${encodeURIComponent(payload.email)}`,
            );
        } catch (error) {
            toast.error(
                error instanceof ApiError
                    ? error.message
                    : "Unable to create your account. Please try again.",
            );
        }
    };

    const handleGoogleSignUp = () => {
        if (isSubmitting || signUpMutation.isPending) {
            return;
        }

        googleLoginMutation.startGoogleLogin({
            onSuccess: () => {
                toast.success("Your account is ready.");
                router.push("/");
            },
            onError: (error) => {
                toast.error(
                    error instanceof Error
                        ? error.message
                        : "Unable to continue with Google. Please try again.",
                );
            },
        });
    };

    const isLoading =
        isSubmitting ||
        signUpMutation.isPending ||
        googleLoginMutation.isPending;

    return (
        <main className="min-h-[calc(100vh-4rem)] bg-background mt-16">
            <div className="container mx-auto grid min-h-[calc(100vh-4rem)] lg:grid-cols-2">
                {/* Brand Section */}
                <motion.section
                    initial={{ opacity: 0, x: -24 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{
                        duration: 0.6,
                        ease: "easeOut",
                    }}
                    className="relative hidden overflow-hidden border-r lg:flex"
                >
                    <div className="absolute inset-0 bg-gradient-to-br from-primary/10 via-background to-secondary/30" />

                    <div className="absolute -left-24 top-16 size-80 rounded-full bg-primary/10 blur-3xl" />

                    <div className="absolute -bottom-24 right-0 size-96 rounded-full bg-blue-400/10 blur-3xl" />

                    <div className="relative flex w-full flex-col justify-between p-10 xl:p-14">
                        <div className="max-w-lg">
                            <h1 className="mt-7 text-balance text-4xl font-semibold leading-[1.12] xl:text-5xl">
                                Start your simpler
                                <br />
                                healthcare journey
                                <br />
                                <span className="text-primary">
                                    with Arovia.
                                </span>
                            </h1>

                            <p className="mt-6 max-w-md text-base leading-7 text-muted-foreground">
                                Create your account and make it easier to
                                discover and manage the healthcare essentials
                                that matter to you and your family.
                            </p>

                            <div className="mt-8 space-y-3">
                                <Benefit text="Keep your healthcare experience organized" />
                                <Benefit text="Manage your orders and preferences" />
                                <Benefit text="Discover everyday wellness essentials" />
                            </div>
                        </div>

                        <p className="text-xs leading-5 text-muted-foreground">
                            Better Health. Made Simple.
                        </p>
                    </div>
                </motion.section>

                {/* Sign Up Panel */}
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
                        {/* Heading */}
                        <div>
                            <p className="text-sm font-medium leading-6 text-primary">
                                Get started
                            </p>

                            <h2 className="mt-2 text-3xl font-semibold leading-tight tracking-normal sm:text-4xl">
                                Create your Arovia account
                            </h2>

                            <p className="mt-3 text-sm leading-6 text-muted-foreground">
                                Join Arovia for a simpler way to manage your
                                everyday healthcare experience.
                            </p>
                        </div>

                        {/* Google Sign Up */}
                        <Button
                            type="button"
                            variant="outline"
                            disabled={isLoading}
                            className="mt-8 h-12 w-full rounded-xl font-medium cursor-pointer"
                            onClick={handleGoogleSignUp}
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
                            {/* Name */}
                            <div className="grid gap-5 sm:grid-cols-2">
                                {/* First Name */}
                                <div className="space-y-2.5">
                                    <Label
                                        htmlFor="firstName"
                                        className="block leading-6"
                                    >
                                        First name
                                    </Label>

                                    <Input
                                        id="firstName"
                                        type="text"
                                        autoComplete="given-name"
                                        placeholder="Enter first name"
                                        aria-invalid={Boolean(
                                            errors.firstName,
                                        )}
                                        aria-describedby={
                                            errors.firstName
                                                ? "first-name-error"
                                                : undefined
                                        }
                                        className="h-12 rounded-xl"
                                        {...register("firstName")}
                                    />

                                    {errors.firstName && (
                                        <p
                                            id="first-name-error"
                                            role="alert"
                                            className="text-xs leading-5 text-destructive"
                                        >
                                            {errors.firstName.message}
                                        </p>
                                    )}
                                </div>

                                {/* Last Name */}
                                <div className="space-y-2.5">
                                    <Label
                                        htmlFor="lastName"
                                        className="block leading-6"
                                    >
                                        Last name
                                    </Label>

                                    <Input
                                        id="lastName"
                                        type="text"
                                        autoComplete="family-name"
                                        placeholder="Enter last name"
                                        aria-invalid={Boolean(
                                            errors.lastName,
                                        )}
                                        aria-describedby={
                                            errors.lastName
                                                ? "last-name-error"
                                                : undefined
                                        }
                                        className="h-12 rounded-xl"
                                        {...register("lastName")}
                                    />

                                    {errors.lastName && (
                                        <p
                                            id="last-name-error"
                                            role="alert"
                                            className="text-xs leading-5 text-destructive"
                                        >
                                            {errors.lastName.message}
                                        </p>
                                    )}
                                </div>
                            </div>

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
                                    aria-invalid={Boolean(errors.email)}
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
                            <PasswordField
                                label="Password"
                                placeholder="Create a password"
                                type={
                                    showPassword
                                        ? "text"
                                        : "password"
                                }
                                error={errors.password?.message}
                                autoComplete="new-password"
                                showPassword={showPassword}
                                onToggle={() =>
                                    setShowPassword(
                                        (current) => !current,
                                    )
                                }
                                register={register}
                            />

                            {/* Password Requirements */}
                            <div className="rounded-xl border bg-muted/30 p-4">
                                <p className="text-xs font-medium leading-5 text-foreground">
                                    Password requirements
                                </p>

                                <div className="mt-3 grid gap-2 sm:grid-cols-2">
                                    <PasswordRequirement text="8+ characters" />
                                    <PasswordRequirement text="One uppercase letter" />
                                    <PasswordRequirement text="One lowercase letter" />
                                    <PasswordRequirement text="One number" />
                                </div>
                            </div>

                            {/* Terms */}
                            <div className="space-y-2">
                                <div className="flex items-start gap-3">
                                    <Checkbox
                                        id="terms"
                                        checked={termsAccepted}
                                        onCheckedChange={(checked) =>
                                            setValue(
                                                "terms",
                                                checked === true,
                                                {
                                                    shouldValidate: true,
                                                    shouldDirty: true,
                                                    shouldTouch: true,
                                                },
                                            )
                                        }
                                        aria-invalid={Boolean(
                                            errors.terms,
                                        )}
                                    />

                                    <Label
                                        htmlFor="terms"
                                        className="cursor-pointer text-xs font-normal leading-5 text-muted-foreground"
                                    >
                                        I agree to Arovia&apos;s{" "}
                                        <Link
                                            href="/service-terms"
                                            className="font-medium text-foreground underline underline-offset-2 transition-colors hover:text-primary"
                                        >
                                            Terms of Service
                                        </Link>{" "}
                                        and{" "}
                                        <Link
                                            href="/privacy-policy"
                                            className="font-medium text-foreground underline underline-offset-2 transition-colors hover:text-primary"
                                        >
                                            Privacy Policy
                                        </Link>
                                        .
                                    </Label>
                                </div>

                                {errors.terms && (
                                    <p
                                        role="alert"
                                        className="text-xs leading-5 text-destructive"
                                    >
                                        {errors.terms.message}
                                    </p>
                                )}
                            </div>

                            {/* Submit */}
                            <Button
                                type="submit"
                                disabled={
                                    !isValid ||
                                    isLoading
                                }
                                className="h-12 w-full rounded-xl text-sm font-semibold transition-colors cursor-pointer"
                            >
                                {isLoading ? (
                                    <>
                                        <Loader2 className="mr-2 size-4 animate-spin" />
                                        Creating account...
                                    </>
                                ) : (
                                    "Create Account"
                                )}
                            </Button>
                        </form>

                        {/* Sign In */}
                        <p className="mt-8 text-center text-sm leading-6 text-muted-foreground">
                            Already have an account?{" "}
                            <Link
                                href="/sign-in"
                                className="font-semibold text-primary transition-colors hover:text-primary/80"
                            >
                                Sign in
                            </Link>
                        </p>
                    </div>
                </motion.section>
            </div>
        </main>
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

function PasswordRequirement({
    text,
}: {
    text: string;
}) {
    return (
        <div className="flex items-center gap-2 text-xs leading-5 text-muted-foreground">
            <span className="size-1.5 shrink-0 rounded-full bg-primary/60" />
            <span>{text}</span>
        </div>
    );
}

function PasswordField({
    label,
    placeholder,
    type,
    error,
    autoComplete,
    showPassword,
    onToggle,
    register,
}: {
    label: string;
    placeholder: string;
    type: string;
    error?: string;
    autoComplete: string;
    showPassword: boolean;
    onToggle: () => void;
    register: ReturnType<
        typeof useForm<SignUpFormValues>
    >["register"];
}) {
    return (
        <div className="space-y-2.5">
            <Label
                htmlFor="password"
                className="block leading-6"
            >
                {label}
            </Label>

            <div className="relative">
                <Input
                    id="password"
                    type={type}
                    autoComplete={autoComplete}
                    placeholder={placeholder}
                    aria-invalid={Boolean(error)}
                    aria-describedby={
                        error
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
                    onClick={onToggle}
                    className="absolute right-1 top-1/2 flex size-10 -translate-y-1/2 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-accent hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                    {showPassword ? (
                        <EyeOff className="size-4" />
                    ) : (
                        <Eye className="size-4" />
                    )}
                </button>
            </div>

            {error && (
                <p
                    id="password-error"
                    role="alert"
                    className="text-xs leading-5 text-destructive"
                >
                    {error}
                </p>
            )}
        </div>
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
