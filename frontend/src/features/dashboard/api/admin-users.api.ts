import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

export type AdminUserRole =
  | "Customer"
  | "Admin"
  | "Pharmacist"
  | "WarehouseStaff";

export interface AdminUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: AdminUserRole;
  isEmailVerified: boolean;
}

export interface AdminUsersResponse {
  users: AdminUser[];
  totalCount: number;
}

async function listUsers(): Promise<AdminUsersResponse> {
  try {
    const { data } = await apiClient.get<AdminUsersResponse>("/v1/admin/users");
    return data;
  } catch (error) {
    throw normalizeApiError(error);
  }
}

async function changeUserRole(userId: string, role: AdminUserRole): Promise<void> {
  try {
    await apiClient.post<void>(
      `/v1/admin/users/${encodeURIComponent(userId)}/role`,
      { role },
    );
  } catch (error) {
    throw normalizeApiError(error);
  }
}

export const adminUsersApi = { listUsers, changeUserRole };
