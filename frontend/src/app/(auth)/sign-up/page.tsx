import type { Metadata } from "next";

import { SignUpForm } from "../_components/sign-up-form";

export const metadata: Metadata = {
    title: "Create Account",
    description:
        "Create your Arovia account for a simpler healthcare and wellness experience.",
};

export default function SignUpPage() {
    return <SignUpForm />;
}