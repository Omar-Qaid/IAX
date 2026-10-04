import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface MarkupTableDto {
  recId: number;
  markupCode: string;
  txt: string;
  moduleType: number;
  taxItemGroup: string;
  taxRateType: number;
  taxWithholdItemGroup: number;
  custType: number;
  custPosting: number;
  customerLedgerDimension: number | null;
  vendType: number;
  vendPosting: number;
  vendorLedgerDimension: number | null;
  maxAmount: number;
  useInMatching: number;
  includeIntoIntrastatInvoiceValue: number;
  includeIntoIntrastatStatisticalValue: number;
  isShipping: number;
  refundable: number;
  mcrProrate: number;
  mcrBrokerContractFee: number;
}

export interface MarkupTableRecord extends MarkupTableDto { id: string }

const endpoint = '/v1/MarkupTable';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The charges-code response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: MarkupTableDto): MarkupTableRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: MarkupTableRecord): MarkupTableDto => ({
  ...record,
  markupCode: record.markupCode.trim(),
  txt: record.txt.trim(),
  taxItemGroup: record.taxItemGroup.trim().toUpperCase(),
});

export const markupTableApi = {
  async list(signal?: AbortSignal): Promise<MarkupTableRecord[]> {
    const response = await apiClient.get<ApiResponse<MarkupTableDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: MarkupTableRecord): Promise<MarkupTableRecord> {
    const response = await apiClient.post<ApiResponse<MarkupTableDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: MarkupTableRecord): Promise<MarkupTableRecord> {
    const response = await apiClient.put<ApiResponse<MarkupTableDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: MarkupTableRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
