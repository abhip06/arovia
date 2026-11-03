import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

import type { CurrentUser, UpdateCurrentUserRequest } from "@/types/user.types";

export async function getCurrentUser(): Promise<CurrentUser> {
    try {
        const response = await apiClient.get<CurrentUser>(
            "/v1/users/me",
        );

        return response.data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}

export async function updateCurrentUser(
    request: UpdateCurrentUserRequest
): Promise<CurrentUser> {
    try {
        const response = await apiClient.put<CurrentUser>(
            "/v1/users/me",
            request
        );

        return response.data;
    } catch (error) {
        throw normalizeApiError(error);
    }
}