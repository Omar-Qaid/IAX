import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TaxAuthorityDto {
  recId: number;
  taxAuthority: string;
  name: string;
  taxAuthorityId: string;
  accountNum: string;
  phone: string;
  mobile: string;
  fax: string;
  sms: string;
  telex: string;
  extension: string;
  pager: string;
  email: string;
  url: string;
  address: string;
  roundOff: number;
  roundOffType: number;
  taxReportLayout: number;
  useDefaultLayout: number;
  separateTaxSummary: number;
  printBlankPage: number;
}

export interface TaxAuthorityRecord extends TaxAuthorityDto { id: string }

const endpoint = '/v1/TaxAuthorityAddress';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The tax authority response did not contain data.', 500);
  }
  return response.data;
};
const toRecord = (dto: TaxAuthorityDto): TaxAuthorityRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: TaxAuthorityRecord): TaxAuthorityDto => ({
  ...record,
  taxAuthority: record.taxAuthority.trim().toUpperCase(),
  taxAuthorityId: record.taxAuthorityId.trim().toUpperCase(),
  name: record.name.trim(),
});

export const taxAuthorityApi = {
  async list(signal?: AbortSignal): Promise<TaxAuthorityRecord[]> {
    const response = await apiClient.get<ApiResponse<TaxAuthorityDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: TaxAuthorityRecord): Promise<TaxAuthorityRecord> {
    const response = await apiClient.post<ApiResponse<TaxAuthorityDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: TaxAuthorityRecord): Promise<TaxAuthorityRecord> {
    const response = await apiClient.put<ApiResponse<TaxAuthorityDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: TaxAuthorityRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
