import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { FetchRowsParams } from '@shared/components/data-grid/types';

export interface WfCategoryDto {
  recId: number;
  code: string | null;
  name: string | null;
  description: string | null;
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
  description: record.description?.trim() || null,
});

export const wfCategoryApi = {
  async list(signal?: AbortSignal): Promise<WfCategoryRecord[]> {
    const response = await apiClient.get<ApiResponse<WfCategoryDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async listPage({
    page,
    pageSize,
    sort,
    filters,
    globalSearch,
    signal,
  }: FetchRowsParams): Promise<{ rows: WfCategoryRecord[]; totalCount: number }> {
    const params = new URLSearchParams({
      PageNumber: String(page + 1),
      PageSize: String(pageSize),
    });
    const activeSort = sort.find((item) => item.sort != null);
    if (activeSort) {
      params.set('SortField', activeSort.field);
      params.set('SortOrder', activeSort.sort ?? 'asc');
    }
    if (globalSearch.trim()) params.set('SearchTerm', globalSearch.trim());
    filters.forEach((filter, index) => {
      params.set(`Filters[${index}].Field`, filter.field);
      params.set(`Filters[${index}].Operator`, filter.operator);
      params.set(`Filters[${index}].Value`, String(filter.value ?? ''));
    });

    const response = await apiClient.get<ApiResponse<WfCategoryDto[]>>(`${endpoint}/paged`, {
      params,
      signal,
    });
    const rows = requireData(response.data).map(toRecord);
    return { rows, totalCount: response.data.pagination?.totalRecords ?? rows.length };
  },
  async getById(recId: number, signal?: AbortSignal): Promise<WfCategoryRecord> {
    const response = await apiClient.get<ApiResponse<WfCategoryDto>>(`${endpoint}/${recId}`, {
      signal,
    });
    return toRecord(requireData(response.data));
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
