"use client";

import { useEffect, useMemo, useRef, useState } from "react";

import { useRouter, useSearchParams } from "next/navigation";

import { motion } from "framer-motion";

import {
    AlertCircle,
    ArrowRight,
    CheckCircle2,
    Loader2,
    Mail,
    ShieldCheck,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Logo } from "@/components/brand/logo";

import { verifyEmailTokenSchema } from "@/lib/validation/verify-email.validations";
import { forgotPasswordSchema } from "@/lib/validation/forgot-password.validations";

import type {
    VerifyEmailRequest,
    VerifyEmailResponse,
} from "@/types/auth.types";
import { ApiError } from "@/lib/api/errors";
import {
    useResendEmailVerification,
    useVerifyEmail,
} from "@/features/auth/hooks/use-email-verification";

type VerificationState =
    | "checking"
    | "check-inbox"
    | "success"
    | "invalid-link"
    | "failed";

interface VerificationResult {
    token: string;
    state: "success" | "failed";
    email?: string;
    errorMessage?: string;
}

export function VerifyEmailPage() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const { mutateAsync: verifyEmail } = useVerifyEmail();
    const resendMutation = useResendEmailVerification();

    const [verificationResult, setVerificationResult] =
        useState<VerificationResult | null>(null);
    const [resendMessage, setResendMessage] = useState<string | null>(
        null,
    );
    const [resendError, setResendError] = useState<string | null>(
        null,
    );

    const pendingVerification = useRef<{
        token: string;
        promise: Promise<VerifyEmailResponse>;
    } | null>(null);

    const token = useMemo(
        () => searchParams.get("token"),
        [searchParams],
    );
    const email = useMemo(
        () => searchParams.get("email"),
        [searchParams],
    );
    const validEmail = useMemo(
        () => forgotPasswordSchema.safeParse({ email }),
        [email],
    );
    const validToken = useMemo(
        () => verifyEmailTokenSchema.safeParse({ token }),
        [token],
    );
    const tokenValue = validToken.success ? validToken.data.token : null;
    const verificationState: VerificationState = !token
        ? validEmail.success
            ? "check-inbox"
            : "invalid-link"
        : !validToken.success
          ? "invalid-link"
          : verificationResult?.token === validToken.data.token
            ? verificationResult.state
            : "checking";
    const userEmail =
        verificationResult?.token === tokenValue
            ? verificationResult.email ?? null
            : validEmail.success
              ? validEmail.data.email
              : null;
    const errorMessage =
        verificationResult?.token === tokenValue
            ? verificationResult.errorMessage ?? null
            : null;

    useEffect(() => {
        let active = true;

        if (!token || !validToken.success) {
            pendingVerification.current = null;
            return;
        }

        const request: VerifyEmailRequest = {
            token: validToken.data.token,
        };

        let verificationPromise =
            pendingVerification.current?.token === request.token
                ? pendingVerification.current.promise
                : null;

        if (!verificationPromise) {
            verificationPromise = verifyEmail(request);
            pendingVerification.current = {
                token: request.token,
                promise: verificationPromise,
            };
        }

        void verificationPromise
            .then((data) => {
                if (!active) {
                    return;
                }

                setVerificationResult({
                    token: request.token,
                    state: data.isEmailVerified ? "success" : "failed",
                    email: data.email,
                });
            })
            .catch((error: unknown) => {
                if (!active) {
                    return;
                }

                setVerificationResult({
                    token: request.token,
                    state: "failed",
                    errorMessage:
                        error instanceof ApiError
                            ? error.message
                            : "We couldn't verify your email. Please try again.",
                });
            });

        return () => {
            active = false;
        };
    }, [token, validToken, verifyEmail]);

    const resendVerification = async () => {
        if (!validEmail.success) {
            return;
        }

        setResendError(null);
        setResendMessage(null);

        try {
            await resendMutation.mutateAsync({
                email: validEmail.data.email,
            });
            setResendMessage(
                "If this account still needs verification, a new email is on its way.",
            );
        } catch (error) {
            setResendError(
                error instanceof ApiError
                    ? error.message
                    : "We couldn't resend the verification email. Please try again.",
            );
        }
    };

    if (verificationState === "invalid-link") {
        return (
            <VerificationShell>
                <VerificationIcon
                    type="error"
                    icon={<AlertCircle className="size-7" />}
                />

                <VerificationContent
                    eyebrow="Verification unavailable"
                    title="This verification link isn&apos;t valid"
                    description="The link may be incomplete or malformed. Please return to sign in and request a new verification email."
                />

                <div className="mt-8 flex flex-col gap-3">
                    <Button
                        type="button"
                        onClick={() => router.push("/sign-in")}
                        className="h-11 w-full rounded-xl"
                    >
                        Back to sign in
                    </Button>

                    <Button
                        type="button"
                        variant="ghost"
                        onClick={() => router.push("/")}
                        className="h-11 w-full rounded-xl"
                    >
                        Return home
                    </Button>
                </div>
            </VerificationShell>
        );
    }

    if (verificationState === "success") {
        return (
            <VerificationShell>
                <VerificationIcon
                    type="success"
                    icon={<CheckCircle2 className="size-7" />}
                />

                <VerificationContent
                    eyebrow="Email verified"
                    title="You're all set"
                    description={
                        userEmail
                            ? `Your email address ${userEmail} has been successfully verified.`
                            : "Your email address has been successfully verified."
                    }
                />

                <div className="mt-8">
                    <Button
                        type="button"
                        onClick={() => router.push("/sign-in")}
                        className="group flex h-11 w-full items-center justify-center rounded-xl"
                    >
                        Continue to sign in

                        <ArrowRight className="ml-2 size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
                    </Button>
                </div>

                <VerificationFooterNote>
                    Your Arovia account is now ready to use.
                </VerificationFooterNote>
            </VerificationShell>
        );
    }

    if (verificationState === "check-inbox") {
        return (
            <VerificationShell>
                <VerificationIcon
                    type="success"
                    icon={<Mail className="size-7" />}
                />

                <VerificationContent
                    eyebrow="One more step"
                    title="Check your inbox"
                    description={
                        userEmail
                            ? `We sent a verification link to ${userEmail}. Open it to finish setting up your account.`
                            : "Open the verification link in your email to finish setting up your account."
                    }
                />

                <div className="mt-8 flex w-full flex-col gap-3">
                    <Button
                        type="button"
                        onClick={() => router.push("/sign-in")}
                        className="h-11 w-full rounded-xl"
                    >
                        Back to sign in
                    </Button>

                    <Button
                        type="button"
                        variant="outline"
                        disabled={!validEmail.success || resendMutation.isPending}
                        onClick={() => void resendVerification()}
                        className="h-11 w-full rounded-xl"
                    >
                        {resendMutation.isPending
                            ? "Sending..."
                            : resendMessage
                              ? "Send another email"
                              : "Resend verification email"}
                    </Button>

                    {resendMessage && (
                        <p role="status" className="text-center text-sm text-muted-foreground">
                            {resendMessage}
                        </p>
                    )}

                    {resendError && (
                        <p role="alert" className="text-center text-sm text-destructive">
                            {resendError}
                        </p>
                    )}
                </div>
            </VerificationShell>
        );
    }

    if (verificationState === "failed") {
        return (
            <VerificationShell>
                <VerificationIcon
                    type="error"
                    icon={<AlertCircle className="size-7" />}
                />

                <VerificationContent
                    eyebrow="Verification failed"
                    title="We couldn't verify your email"
                    description={
                        errorMessage ??
                        "The verification link may have expired or already been used. Please request a new verification email and try again."
                    }
                />

                <div className="mt-8 flex flex-col gap-3">
                    {validEmail.success && (
                        <Button
                            type="button"
                            variant="outline"
                            disabled={resendMutation.isPending}
                            onClick={() => void resendVerification()}
                            className="h-11 w-full rounded-xl"
                        >
                            {resendMutation.isPending
                                ? "Sending..."
                                : "Resend verification email"}
                        </Button>
                    )}

                    <Button
                        type="button"
                        onClick={() => router.push("/sign-in")}
                        className="h-11 w-full rounded-xl"
                    >
                        Back to sign in
                    </Button>

                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => router.push("/contact")}
                        className="h-11 w-full rounded-xl"
                    >
                        Contact support
                    </Button>
                </div>

                {resendMessage && (
                    <p role="status" className="mt-3 text-center text-sm text-muted-foreground">
                        {resendMessage}
                    </p>
                )}

                {resendError && (
                    <p role="alert" className="mt-3 text-center text-sm text-destructive">
                        {resendError}
                    </p>
                )}

                <VerificationFooterNote>
                    Don&apos;t panic. We will resolve your issue.
                </VerificationFooterNote>
            </VerificationShell>
        );
    }

    return (
        <VerificationShell>
            <VerificationIcon
                type="loading"
                icon={
                    <Loader2 className="size-7 animate-spin" />
                }
            />

            <VerificationContent
                eyebrow="Verifying your email"
                title="Just a moment"
                description="We're securely verifying your email address and activating your Arovia account."
            />

            <div className="mt-8 rounded-2xl border border-border/70 bg-muted/30 p-4">
                <div className="flex items-start gap-3">
                    <ShieldCheck className="mt-0.5 size-4 shrink-0 text-primary" />

                    <div className="flex min-w-0 flex-col gap-1">
                        <p className="text-sm font-medium text-foreground">
                            Secure verification
                        </p>

                        <p className="text-xs leading-5 text-muted-foreground">
                            Your verification link is being processed
                            securely. Your verification token is never
                            displayed in the interface.
                        </p>
                    </div>
                </div>
            </div>

            <VerificationFooterNote>
                This normally takes only a few seconds.
            </VerificationFooterNote>
        </VerificationShell>
    );
}

function VerificationShell({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <main className="relative min-h-[calc(100dvh-4rem)] overflow-hidden bg-background mt-12">
            <div
                aria-hidden="true"
                className="pointer-events-none absolute inset-0"
            >
                <div className="absolute left-1/2 top-0 size-[28rem] -translate-x-1/2 rounded-full bg-primary/5 blur-3xl sm:size-[34rem]" />

                <div className="absolute -bottom-40 -left-40 size-80 rounded-full bg-primary/5 blur-3xl sm:size-96" />

                <div className="absolute -right-40 top-1/3 size-80 rounded-full bg-blue-400/5 blur-3xl sm:size-96" />
            </div>

            <div className="relative container mx-auto flex min-h-[calc(100dvh-4rem)] items-center justify-center px-4 py-10 sm:px-6 sm:py-14 lg:px-8 lg:py-16">
                <div className="flex w-full max-w-md flex-col">
                    {/* Logo */}
                    <motion.div
                        initial={{
                            opacity: 0,
                            y: -8,
                        }}
                        animate={{
                            opacity: 1,
                            y: 0,
                        }}
                        transition={{
                            duration: 0.45,
                            ease: "easeOut",
                        }}
                        className="mb-7 flex justify-center"
                    >
                        <Logo
                            imageSize={36}
                            textClassName="text-2xl"
                        />
                    </motion.div>

                    {/* Card */}
                    <motion.div
                        initial={{
                            opacity: 0,
                            y: 18,
                        }}
                        animate={{
                            opacity: 1,
                            y: 0,
                        }}
                        transition={{
                            duration: 0.55,
                            delay: 0.05,
                            ease: "easeOut",
                        }}
                        className="rounded-3xl border border-border/70 bg-card/90 p-6 shadow-sm backdrop-blur-xl sm:p-9"
                    >
                        <div className="flex flex-col">
                            {children}
                        </div>
                    </motion.div>

                    {/* Security label */}
                    <motion.div
                        initial={{
                            opacity: 0,
                        }}
                        animate={{
                            opacity: 1,
                        }}
                        transition={{
                            duration: 0.45,
                            delay: 0.2,
                        }}
                        className="mt-5 flex items-center justify-center gap-2 text-xs text-muted-foreground"
                    >
                        <ShieldCheck className="size-3.5" />

                        <span>
                            Secure account verification
                        </span>
                    </motion.div>
                </div>
            </div>
        </main>
    );
}

function VerificationIcon({
    type,
    icon,
}: {
    type: "success" | "error" | "loading";
    icon: React.ReactNode;
}) {
    const iconStyles = {
        success: "bg-primary/10 text-primary",
        error: "bg-destructive/10 text-destructive",
        loading: "bg-primary/10 text-primary",
    };

    return (
        <motion.div
            initial={{
                scale: 0.9,
                opacity: 0,
            }}
            animate={{
                scale: 1,
                opacity: 1,
            }}
            transition={{
                duration: 0.4,
                ease: "easeOut",
            }}
            className={`mx-auto flex size-16 items-center justify-center rounded-2xl ${iconStyles[type]}`}
        >
            {icon}
        </motion.div>
    );
}

function VerificationContent({
    eyebrow,
    title,
    description,
}: {
    eyebrow: string;
    title: string;
    description: string;
}) {
    return (
        <div className="mt-7 flex flex-col items-center gap-3 text-center">
            <p className="text-sm font-medium tracking-normal text-primary">
                {eyebrow}
            </p>

            <h1 className="text-3xl font-semibold tracking-normal text-foreground sm:text-4xl">
                {title}
            </h1>

            <p className="max-w-md tracking-normal text-sm text-muted-foreground sm:text-base">
                {description}
            </p>
        </div>
    );
}

function VerificationFooterNote({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <p className="mt-7 text-center text-xs text-muted-foreground">
            {children}
        </p>
    );
}
