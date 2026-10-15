import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventItemGroupPosting {
  id: string;
  accountType: number;
  ledgerDimension: string;
  mainAccount: string;
}

export interface InventItemGroupRecord {
  id: string;
  itemGroupId: string;
  name: string;
  postings: InventItemGroupPosting[];
}

const endpoint = '/v1/InventItemGroup';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'Item group request failed.', 400);
  }
  return response.data;
};
export const inventItemGroupApi = {
  async list(signal?: AbortSignal): Promise<InventItemGroupRecord[]> {
    return requireData(
      (await apiClient.get<ApiResponse<InventItemGroupRecord[]>>(endpoint, { signal })).data
    );
  },
  async ledgerAccounts(signal?: AbortSignal): Promise<InventItemGroupPosting[]> {
    return requireData(
      (
        await apiClient.get<ApiResponse<InventItemGroupPosting[]>>(`${endpoint}/ledger-accounts`, {
          signal,
        })
      ).data
    );
  },
  async create(record: InventItemGroupRecord): Promise<InventItemGroupRecord> {
    return requireData(
      (await apiClient.post<ApiResponse<InventItemGroupRecord>>(endpoint, record)).data
    );
  },
  async update(record: InventItemGroupRecord): Promise<InventItemGroupRecord> {
    return requireData(
      (
        await apiClient.put<ApiResponse<InventItemGroupRecord>>(
          `${endpoint}/${encodeURIComponent(record.id)}`,
          record
        )
      ).data
    );
  },
  async delete(record: InventItemGroupRecord): Promise<void> {
    requireData(
      (await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(record.id)}`))
        .data
    );
  },
};
