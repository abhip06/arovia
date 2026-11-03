import axios from "axios";

export interface ApiErrorResponse {
    title?: string;
    detail?: string;
    message?: string;
    errors?: Record<string, string[]>;
}

export class ApiError extends Error {
    readonly status?: number;
    readonly data?: ApiErrorResponse;

    constructor(
        message: string,
        status?: number,
        data?: ApiErrorResponse,
    ) {
        super(message);

        this.name = "ApiError";
        this.status = status;
        this.data = data;
    }
}

export function normalizeApiError(error: unknown): ApiError {
    if (!axios.isAxiosError<ApiErrorResponse>(error)) {
        return new ApiError("An unexpected error occurred.");
    }

    const status = error.response?.status;
    const data = error.response?.data;

    const message =
        data?.detail ??
        data?.message ??
        error.message ??
        "An unexpected error occurred.";

    return new ApiError(message, status, data);
}