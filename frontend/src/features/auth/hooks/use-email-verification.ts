"use client";

import { useMutation } from "@tanstack/react-query";

import {
    resendEmailVerification,
    verifyEmail,
} from "../api/auth.api";

export function useVerifyEmail() {
    return useMutation({ mutationFn: verifyEmail });
}

export function useResendEmailVerification() {
    return useMutation({ mutationFn: resendEmailVerification });
}
