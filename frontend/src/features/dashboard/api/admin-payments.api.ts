import { apiClient } from "@/lib/api/axios";
import { normalizeApiError } from "@/lib/api/errors";

export interface AdminPayment {
  id: string;
  orderId: string;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface AdminPaymentsPage {
  items: AdminPayment[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

async function getPage(page: number): Promise<AdminPaymentsPage> {
  try {
    const { data } = await apiClient.get<AdminPaymentsPage>("/v1/admin/payments", {
      params: { page },
    });
    return data;
  } catch (error) {
    throw normalizeApiError(error);
  }
}

export const adminPaymentsApi = { getPage };
