import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface HcmReferenceDto {
  recId: number;
  code?: string | null;
  name?: string | null;
  nameAlias?: string | null;
  description?: string | null;
  isActive?: boolean;
  dataAreaId?: string | null;
  recVersion?: number | null;
  rowVersion?: string | null;
}

export interface HcmReferenceRecord {
  id: string;
  recId: number;
  code: string;
  name: string;
  nameAlias?: string | null;
  description?: string | null;
  isActive: boolean;
  dataAreaId: string;
  recVersion: number;
  rowVersion: string | null;
}

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The API response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: HcmReferenceDto): HcmReferenceRecord => {
  const recId = dto.recId ?? 0;
  return {
    id: String(recId),
    recId,
    code: dto.code ?? '',
    name: dto.name ?? '',
    nameAlias: dto.nameAlias ?? null,
    description: dto.description ?? null,
    isActive: dto.isActive ?? true,
    dataAreaId: dto.dataAreaId ?? 'dat',
    recVersion: dto.recVersion ?? 1,
    rowVersion: dto.rowVersion ?? null,
  };
};

const toDto = (record: HcmReferenceRecord): HcmReferenceDto => ({
  recId: record.recId,
  code: record.code,
  name: record.name,
  nameAlias: record.nameAlias,
  description: record.description,
  isActive: record.isActive,
  dataAreaId: record.dataAreaId,
  recVersion: record.recVersion,
  rowVersion: record.rowVersion,
});

export function createHcmReferenceApi(endpoint: string) {
  const baseUrl = endpoint.startsWith('/') ? `/v1${endpoint}` : `/v1/${endpoint}`;
  return {
    async list(signal?: AbortSignal): Promise<HcmReferenceRecord[]> {
      const response = await apiClient.get<ApiResponse<HcmReferenceDto[]>>(baseUrl, { signal });
      return requireData(response.data).map(toRecord);
    },
    async create(record: HcmReferenceRecord): Promise<HcmReferenceRecord> {
      const response = await apiClient.post<ApiResponse<HcmReferenceDto>>(baseUrl, toDto(record));
      return toRecord(requireData(response.data));
    },
    async update(record: HcmReferenceRecord): Promise<HcmReferenceRecord> {
      const response = await apiClient.put<ApiResponse<HcmReferenceDto>>(
        `${baseUrl}/${record.recId}`,
        toDto(record)
      );
      return toRecord(requireData(response.data));
    },
    async delete(record: HcmReferenceRecord): Promise<void> {
      const response = await apiClient.delete<ApiResponse<boolean>>(`${baseUrl}/${record.recId}`);
      requireData(response.data);
    },
  };
}

export const hcmNationalityApi = createHcmReferenceApi('/HcmNationality');
export const hcmOccupationApi = createHcmReferenceApi('/HcmOccupation');
export const hcmDepartmentApi = createHcmReferenceApi('/HcmDepartment');
