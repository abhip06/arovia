import type { Metadata } from "next";

import { ForgotPasswordForm } from "../_components/forgot-password-form";

export const metadata: Metadata = {
    title: "Forgot Password",
    description:
        "Reset your Arovia account password securely.",
};

export default function ForgotPasswordPage() {
    return <ForgotPasswordForm />;
}