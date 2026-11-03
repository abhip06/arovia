import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

export type CurrentLocationSource = "user" | "ip";

export interface CurrentLocationResponse {
    pincode: string;
    city: string | null;
    state: string | null;
    country: string | null;
    source: CurrentLocationSource;
    isConfirmed: boolean;
}

function parseCurrentLocation(
    value: unknown,
): CurrentLocationResponse {
    if (!value || typeof value !== "object") {
        throw new Error("Invalid current-location response.");
    }

    const data = value as Record<string, unknown>;

    if (
        typeof data.pincode !== "string" ||
        !/^\d{6}$/.test(data.pincode) ||
        (data.source !== "user" && data.source !== "ip") ||
        typeof data.isConfirmed !== "boolean"
    ) {
        throw new Error("Invalid current-location response.");
    }

    const readText = (key: string): string | null => {
        const item = data[key];

        return typeof item === "string" && item.trim()
            ? item.trim()
            : null;
    };

    return {
        pincode: data.pincode,
        city: readText("city"),
        state: readText("state"),
        country: readText("country"),
        source: data.source,
        isConfirmed: data.isConfirmed,
    };
}

async function getCurrentLocation(): Promise<CurrentLocationResponse | null> {
    let response;

    try {
        response = await apiClient.get<unknown>(
            "/v1/location/current",
            {
                validateStatus: (status) =>
                    status === 200 || status === 204,
            },
        );
    } catch (error) {
        throw normalizeApiError(error);
    }

    if (response.status === 204) {
        return null;
    }

    return parseCurrentLocation(response.data);
}

export const locationApi = {
    getCurrentLocation,
};