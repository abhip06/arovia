import axios, { AxiosError, InternalAxiosRequestConfig } from "axios";
import { clearAccessToken, getAccessToken } from "../auth/access-token";
import { refreshAccessToken } from "../auth/session";

declare module "axios" {
    // The generic defaults must match Axios's declarations during module augmentation.
    // eslint-disable-next-line @typescript-eslint/no-unused-vars, @typescript-eslint/no-explicit-any
    interface AxiosRequestConfig<D = any> {
        skipAuthRefresh?: boolean;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars, @typescript-eslint/no-explicit-any
    interface InternalAxiosRequestConfig<D = any> {
        skipAuthRefresh?: boolean;
        _retry?: boolean;
    }
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

if (!API_BASE_URL) {
    throw new Error(
        "NEXT_PUBLIC_API_BASE_URL is not configured.",
    );
}

export const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 15_000,
    headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
    },
    withCredentials: true,
});

apiClient.interceptors.request.use((config) => {
    const accessToken = getAccessToken();

    if (accessToken && !config.skipAuthRefresh) {
        config.headers.Authorization = `Bearer ${accessToken}`;
    }

    return config;
});

let refreshPromise: Promise<void> | null = null;

async function ensureFreshAccessToken() {
    if (!refreshPromise) {
        refreshPromise = refreshAccessToken()
            .then(() => undefined)
            .finally(() => {
                refreshPromise = null;
            });
    }

    return refreshPromise;
}

apiClient.interceptors.response.use(
    (response) => response,

    async (error: AxiosError) => {
        const originalRequest =
            error.config as InternalAxiosRequestConfig | undefined;

        if (error.response?.status !== 401 || !originalRequest) {
            return Promise.reject(error);
        }

        if (originalRequest.skipAuthRefresh) {
            return Promise.reject(error);
        }

        if (originalRequest._retry) {
            clearAccessToken();

            if (typeof window !== "undefined") {
                window.dispatchEvent(
                    new Event("auth:session-expired"),
                );
            }

            return Promise.reject(error);
        }

        originalRequest._retry = true;

        try {
            await ensureFreshAccessToken();

            const accessToken = getAccessToken();

            if (!accessToken) {
                clearAccessToken();
                return Promise.reject(error);
            }

            originalRequest.headers.Authorization =
                `Bearer ${accessToken}`;

            return apiClient(originalRequest);
        } catch (refreshError) {
            clearAccessToken();

            if (typeof window !== "undefined") {
                window.dispatchEvent(
                    new Event("auth:session-expired"),
                );
            }

            return Promise.reject(refreshError);
        }
    },
);
