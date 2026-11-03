import type { Metadata } from "next";

import { SignInForm } from "../_components/sign-in-form";

export const metadata: Metadata = {
    title: "Sign In",
    description:
        "Sign in to your Arovia account to manage your healthcare and wellness experience.",
};

export default function SignInPage() {
    return <SignInForm />;
}