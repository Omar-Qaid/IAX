import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface EcoResProductRecord {
  id: string;
  productNumber: string;
  name: string;
  nameAlias: string;
  description: string;
  searchName: string;
  productType: string;
  productSubtype: string;
}

export interface EcoResProductInput {
  productNumber: string;
  name: string;
  nameAlias: string;
  description: string;
  searchName: string;
  productType: 'Item' | 'Service';
}

const endpoint = '/v1/EcoResProduct';
const data = <T>(response: ApiResponse<T>, fallback: string): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || fallback, 400);
  return response.data;
};

export const ecoResProductApi = {
  async list(signal?: AbortSignal): Promise<EcoResProductRecord[]> {
    const response = await apiClient.get<ApiResponse<EcoResProductRecord[]>>(endpoint, { signal });
    return data(response.data, 'The products response did not contain data.');
  },
  async create(input: EcoResProductInput): Promise<EcoResProductRecord> {
    const response = await apiClient.post<ApiResponse<EcoResProductRecord>>(endpoint, input);
    return data(response.data, 'The product could not be created.');
  },
  async update(id: string, input: EcoResProductInput): Promise<EcoResProductRecord> {
    const response = await apiClient.put<ApiResponse<EcoResProductRecord>>(`${endpoint}/${encodeURIComponent(id)}`, input);
    return data(response.data, 'The product could not be updated.');
  },
  async delete(id: string): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(id)}`);
    data(response.data, 'The product could not be deleted.');
  },
};
