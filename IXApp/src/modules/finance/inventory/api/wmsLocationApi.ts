import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WMSLocationRecord {
  id: string;
  recId: number;
  inventLocationId: string;
  warehouseName: string;
  wmsLocationId: string;
  inputLocation: string;
  checkText: string;
  locationType: number;
  sortCode: number;
}

const endpoint = '/v1/WMSLocation';
const data = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'Inventory location request failed.', 500);
  return response.data;
};
const body = ({ id: _id, recId: _recId, warehouseName: _warehouseName, ...value }: WMSLocationRecord) => value;

export const wmsLocationApi = {
  async list(signal?: AbortSignal): Promise<WMSLocationRecord[]> {
    return data((await apiClient.get<ApiResponse<WMSLocationRecord[]>>(endpoint, { signal })).data);
  },
  async create(value: WMSLocationRecord): Promise<WMSLocationRecord> {
    return data((await apiClient.post<ApiResponse<WMSLocationRecord>>(endpoint, body(value))).data);
  },
  async update(value: WMSLocationRecord): Promise<WMSLocationRecord> {
    return data((await apiClient.put<ApiResponse<WMSLocationRecord>>(`${endpoint}/${value.recId}`, body(value))).data);
  },
  async delete(value: WMSLocationRecord): Promise<void> {
    data((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${value.recId}`)).data);
  },
};
