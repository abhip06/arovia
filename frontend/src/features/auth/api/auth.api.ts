import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

import type {
    SignInRequest,
    SignInResponse,
    ResendEmailVerificationRequest,
    RequestPasswordResetRequest,
    ResetPasswordRequest,
    SignUpResponse,
    SignUpRequest,
    VerifyEmailRequest,
    VerifyEmailResponse,
} from "@/types/auth.types";

export async function signIn(
    request: SignInRequest,
): Promise<SignInResponse> {
    try {
        const response = await apiClient.post<SignInResponse>(
            "/v1/auth/login",
            request,
            { skipAuthRefresh: true },
        );

        return response.data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function signUp(
    request: SignUpRequest,
): Promise<SignUpResponse> {
    try {
        const response = await apiClient.post<SignUpResponse>(
            "/v1/auth/register",
            request,
            { skipAuthRefresh: true },
        );

        return response.data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function verifyEmail(
    request: VerifyEmailRequest,
): Promise<VerifyEmailResponse> {
    try {
        const response =
            await apiClient.post<VerifyEmailResponse>(
                "/v1/auth/email-verification/verify",
                request,
                { skipAuthRefresh: true },
            );

        return response.data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function resendEmailVerification(
    request: ResendEmailVerificationRequest,
): Promise<void> {
    try {
        await apiClient.post(
            "/v1/auth/email-verification/resend",
            request,
            { skipAuthRefresh: true },
        );
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function requestPasswordReset(
    request: RequestPasswordResetRequest,
): Promise<void> {
    try {
        await apiClient.post(
            "/v1/auth/password/reset/request",
            request,
            { skipAuthRefresh: true },
        );
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function resetPassword(
    request: ResetPasswordRequest,
): Promise<void> {
    try {
        await apiClient.post(
            "/v1/auth/password/reset",
            request,
            { skipAuthRefresh: true },
        );
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export function getGoogleLoginUrl() {
    return apiClient.getUri({ url: "/v1/auth/google" });
}
