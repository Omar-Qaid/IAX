import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventPostingRule {
  id: string;
  inventAccountType: number;
  itemCode: number;
  itemRelation: string;
  categoryRelation: string;
  custVendCode: number;
  custVendRelation: string;
  taxGroupId: string;
  ledgerDimension: string;
  mainAccount: string;
}
export interface PostingLookup {
  id: string;
  label: string;
}
export interface InventPostingLookups {
  items: PostingLookup[];
  itemGroups: PostingLookup[];
  customers: PostingLookup[];
  customerGroups: PostingLookup[];
  vendors: PostingLookup[];
  vendorGroups: PostingLookup[];
  categories: PostingLookup[];
  taxGroups: PostingLookup[];
  ledgerAccounts: PostingLookup[];
}
const endpoint = '/v1/InventPosting';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'Inventory posting request failed.', 400);
  return response.data;
};
export const inventPostingApi = {
  async list(accountType: number, signal?: AbortSignal): Promise<InventPostingRule[]> {
    return requireData(
      (
        await apiClient.get<ApiResponse<InventPostingRule[]>>(endpoint, {
          params: { accountType },
          signal,
        })
      ).data
    );
  },
  async lookups(signal?: AbortSignal): Promise<InventPostingLookups> {
    return requireData(
      (await apiClient.get<ApiResponse<InventPostingLookups>>(`${endpoint}/lookups`, { signal }))
        .data
    );
  },
  async create(rule: InventPostingRule): Promise<InventPostingRule> {
    return requireData((await apiClient.post<ApiResponse<InventPostingRule>>(endpoint, rule)).data);
  },
  async update(rule: InventPostingRule): Promise<InventPostingRule> {
    return requireData(
      (
        await apiClient.put<ApiResponse<InventPostingRule>>(
          `${endpoint}/${encodeURIComponent(rule.id)}`,
          rule
        )
      ).data
    );
  },
  async delete(rule: InventPostingRule): Promise<void> {
    requireData(
      (await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(rule.id)}`))
        .data
    );
  },
};
