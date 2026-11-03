import axios from "axios";

import {
    clearAccessToken,
    setAccessToken,
} from "./access-token";

import type {
    RefreshTokenResponse,
} from "@/types/auth.types";

const API_BASE_URL =
    process.env.NEXT_PUBLIC_API_BASE_URL;

if (!API_BASE_URL) {
    throw new Error(
        "NEXT_PUBLIC_API_BASE_URL is not configured.",
    );
}

const sessionClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 15_000,
    headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
    },
    withCredentials: true,
});

let refreshPromise: Promise<RefreshTokenResponse> | null = null;

export async function refreshAccessToken(): Promise<RefreshTokenResponse> {
    if (!refreshPromise) {
        refreshPromise = sessionClient
            .post<RefreshTokenResponse>("/v1/auth/refresh")
            .then(({ data }) => {
                setAccessToken(data.accessToken);
                return data;
            })
            .finally(() => {
                refreshPromise = null;
            });
    }

    return refreshPromise;
}

export async function revokeSession(): Promise<void> {
    try {
        await sessionClient.post("/v1/auth/logout");
    } finally {
        clearAccessToken();
    }
}
