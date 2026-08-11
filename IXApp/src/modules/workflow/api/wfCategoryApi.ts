import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfCategoryDto {
  recId: number;
  code: string | null;
  name: string | null;
  nameAR: string | null;
  description: string | null;
  descriptionAR: string | null;
  sysField: boolean;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export interface WfCategoryRecord extends WfCategoryDto {
  id: string;
}

const endpoint = '/v1/WfCategory';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || 'The workflow-category response did not contain data.',
      500
    );
  }
  return response.data;
};

const toRecord = (dto: WfCategoryDto): WfCategoryRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfCategoryRecord): WfCategoryDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAR: record.nameAR?.trim() || null,
  description: record.description?.trim() || null,
  descriptionAR: record.descriptionAR?.trim() || null,
});

export const wfCategoryApi = {
  async list(signal?: AbortSignal): Promise<WfCategoryRecord[]> {
    const response = await apiClient.get<ApiResponse<WfCategoryDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfCategoryRecord): Promise<WfCategoryRecord> {
    const response = await apiClient.post<ApiResponse<WfCategoryDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfCategoryRecord): Promise<WfCategoryRecord> {
    const response = await apiClient.put<ApiResponse<WfCategoryDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: WfCategoryRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
