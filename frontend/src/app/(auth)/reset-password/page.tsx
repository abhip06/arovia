import type { Metadata } from "next";
import { Suspense } from "react";

import { ResetPasswordForm } from "../_components/reset-password-form";

export const metadata: Metadata = {
    title: "Reset Password",
    description:
        "Create a new password for your Arovia account.",
};

export default function ResetPasswordPage() {
    return (
        <Suspense
            fallback={
                <main className="flex min-h-[calc(100vh-4rem)] items-center justify-center bg-background px-4 py-12">
                    <div
                        aria-label="Loading password reset"
                        className="size-9 animate-spin rounded-full border-2 border-primary/20 border-t-primary"
                    />
                </main>
            }
        >
            <ResetPasswordForm />
        </Suspense>
    );
}
