import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TaxItemGroupLineDto {
  recId: number;
  taxItemGroup: string;
  taxCode: string;
  taxExemptCode: string;
  taxCodeName: string | null;
  taxValue: number | null;
}

export interface TaxItemGroupLine extends TaxItemGroupLineDto { id: string }

export interface TaxItemGroupDto {
  recId: number;
  taxItemGroup: string;
  name: string;
  source: number;
  euSalesListType: number;
  lines: TaxItemGroupLineDto[];
}

export interface TaxItemGroupRecord extends Omit<TaxItemGroupDto, 'lines'> {
  id: string;
  lines: TaxItemGroupLine[];
}

const endpoint = '/v1/TaxItemGroup';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The item sales tax group response did not contain data.', 500);
  }
  return response.data;
};
const toLine = (line: TaxItemGroupLineDto): TaxItemGroupLine => ({
  ...line,
  id: line.recId ? String(line.recId) : `new-${crypto.randomUUID()}`,
});
const toRecord = (dto: TaxItemGroupDto): TaxItemGroupRecord => ({
  ...dto,
  id: String(dto.recId),
  lines: (dto.lines ?? []).map(toLine),
});
const toDto = ({ id: _id, lines, ...record }: TaxItemGroupRecord): TaxItemGroupDto => ({
  ...record,
  taxItemGroup: record.taxItemGroup.trim().toUpperCase(),
  name: record.name.trim(),
  lines: lines.map(({ id: _lineId, ...line }) => ({
    ...line,
    taxItemGroup: record.taxItemGroup.trim().toUpperCase(),
    taxCode: line.taxCode.trim().toUpperCase(),
    taxExemptCode: line.taxExemptCode.trim().toUpperCase() || 'NONE',
  })),
});

export const taxItemGroupApi = {
  async list(signal?: AbortSignal): Promise<TaxItemGroupRecord[]> {
    const response = await apiClient.get<ApiResponse<TaxItemGroupDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: TaxItemGroupRecord): Promise<TaxItemGroupRecord> {
    const response = await apiClient.post<ApiResponse<TaxItemGroupDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: TaxItemGroupRecord): Promise<TaxItemGroupRecord> {
    const response = await apiClient.put<ApiResponse<TaxItemGroupDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: TaxItemGroupRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
