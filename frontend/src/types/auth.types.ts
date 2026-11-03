export type AuthProvider = "password" | "google";

export type VerificationChannel = "email";

export interface SignInRequest {
    email: string;
    password: string;
}

export interface SignInResponse {
    accessToken: string;
    expiresIn: number;
    userId: string;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
}

export interface SignUpRequest {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
}

export interface SignUpResponse {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
    isEmailVerified: boolean;
}

export interface ResendEmailVerificationRequest {
    email: string;
}

export interface VerifyEmailRequest {
    token: string;
}

export interface VerifyEmailResponse {
    id: string;
    email: string;
    isEmailVerified: boolean;
}

export interface VerificationChallenge {
    channel: VerificationChannel;
    destination: string;
    expiresAt: string;
    resendAvailableAt: string;
}

export interface AuthResponse {
    requiresEmailVerification: boolean;
    verification?: VerificationChallenge;
}

export interface AuthUser {
    id: string;
    firstName: string;
    lastName: string;
    email?: string;
    phoneNumber?: string;
}

export interface RefreshTokenResponse {
    accessToken: string;
    expiresIn: number;
}

export interface RequestPasswordResetRequest {
    email: string;
}

export interface ResetPasswordRequest {
    token: string;
    newPassword: string;
}
