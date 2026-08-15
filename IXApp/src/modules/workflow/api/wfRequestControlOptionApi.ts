import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfRequestControlOptionRecord {
  id: string;
  recId: number;
  requestControlId: number;
  value: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

type Dto = Omit<WfRequestControlOptionRecord, 'id'>;
const endpoint = '/v1/WfRequestControlsOption';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The request-control option response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: Dto): WfRequestControlOptionRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfRequestControlOptionRecord): Dto => record;

export const wfRequestControlOptionApi = {
  async list(signal?: AbortSignal): Promise<WfRequestControlOptionRecord[]> {
    const response = await apiClient.get<ApiResponse<Dto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfRequestControlOptionRecord): Promise<WfRequestControlOptionRecord> {
    const response = await apiClient.post<ApiResponse<Dto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfRequestControlOptionRecord): Promise<WfRequestControlOptionRecord> {
    const response = await apiClient.put<ApiResponse<Dto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfRequestControlOptionRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
