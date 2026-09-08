import { apiClient } from './apiClient';
import type { ApiResponse } from './apiResponse';
import { requireApiData } from './apiResponseData';

export interface EntityApiConfig<TDto, TRecord extends { recId: number }> {
  endpoint: string;
  resourceName: string;
  toRecord: (dto: TDto) => TRecord;
  toDto: (record: TRecord) => TDto;
}

/** Shared transport; field normalization stays in the domain adapter. */
export function createEntityApi<TDto, TRecord extends { recId: number }>({
  endpoint,
  resourceName,
  toRecord,
  toDto,
}: EntityApiConfig<TDto, TRecord>) {
  return {
    async list(signal?: AbortSignal): Promise<TRecord[]> {
      const response = await apiClient.get<ApiResponse<TDto[]>>(endpoint, { signal });
      return requireApiData(response.data, resourceName).map(toRecord);
    },
    async create(record: TRecord): Promise<TRecord> {
      const response = await apiClient.post<ApiResponse<TDto>>(endpoint, toDto(record));
      return toRecord(requireApiData(response.data, resourceName));
    },
    async update(record: TRecord): Promise<TRecord> {
      const response = await apiClient.put<ApiResponse<TDto>>(
        `${endpoint}/${record.recId}`,
        toDto(record)
      );
      return toRecord(requireApiData(response.data, resourceName));
    },
    async delete(record: TRecord): Promise<void> {
      const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
      requireApiData(response.data, resourceName);
    },
  };
}
