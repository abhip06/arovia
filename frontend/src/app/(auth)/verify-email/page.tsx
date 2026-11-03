import type { Metadata } from "next";
import { Suspense } from "react";

import { VerifyEmailPage } from "../_components/verify-email-page";

export const metadata: Metadata = {
    title: "Verify Email",
    description:
        "Verify your email address to secure your Arovia account.",
};

function VerifyEmailFallback() {
    return (
        <main className="min-h-[calc(100vh-4rem)] bg-background">
            <div className="container mx-auto flex min-h-[calc(100vh-4rem)] items-center justify-center px-4 py-12 lg:px-6">
                <div className="w-full max-w-lg">
                    <div className="animate-pulse rounded-3xl border border-border/70 bg-card p-8 shadow-sm sm:p-10">
                        <div className="mx-auto size-14 rounded-2xl bg-muted" />

                        <div className="mx-auto mt-7 h-8 w-56 rounded-lg bg-muted" />

                        <div className="mx-auto mt-4 h-4 w-full max-w-md rounded bg-muted" />
                        <div className="mx-auto mt-2 h-4 w-4/5 rounded bg-muted" />

                        <div className="mx-auto mt-8 h-11 w-full rounded-xl bg-muted" />
                    </div>
                </div>
            </div>
        </main>
    );
}

export default function VerifyEmail() {
    return (
        <Suspense fallback={<VerifyEmailFallback />}>
            <VerifyEmailPage />
        </Suspense>
    );
}