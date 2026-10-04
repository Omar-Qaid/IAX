import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export type ChargeDocumentType = 'sales' | 'purchase';
export type ChargeLevel = 'header' | 'line';

export interface MarkupTransRecord {
  id: string;
  recId: number;
  markupCode: string;
  lineNum: number;
  transDate: string;
  txt: string;
  voucher: string;
  markupCategory: number;
  moduleType: number;
  transRecId: number;
  transTableId: number;
  currencyCode: string;
  value: number;
  calculatedAmount: number;
  keep: number;
  mcrBrokerContractFee: number;
  taxGroup: string;
  taxItemGroup: string;
  intercompanyRefRecId: number;
  intercompanyMarkupValue: number;
}

type MarkupTransDto = Omit<MarkupTransRecord, 'id'>;

export interface MarkupCodeOption {
  markupCode: string;
  txt: string;
  taxItemGroup: string;
  mcrBrokerContractFee: number;
}

const requireData = <T>(response: ApiResponse<T>, fallback: string): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || fallback, 500);
  return response.data;
};
const toRecord = (dto: MarkupTransDto): MarkupTransRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: MarkupTransRecord): MarkupTransDto => record;

export const markupTransApi = {
  async list(documentType: ChargeDocumentType, documentRecId: number, level: ChargeLevel, signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<MarkupTransDto[]>>(
      `/v1/MarkupTrans/document/${documentType}/${documentRecId}`,
      { params: { level }, signal }
    );
    return requireData(response.data, 'The document charges could not be loaded.').map(toRecord);
  },
  async codes(documentType: ChargeDocumentType, signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<MarkupCodeOption[]>>(
      `/v1/MarkupTrans/codes/${documentType}`,
      { signal }
    );
    return requireData(response.data, 'The charges codes could not be loaded.');
  },
  async create(documentType: ChargeDocumentType, documentRecId: number, level: ChargeLevel, record: MarkupTransRecord) {
    const response = await apiClient.post<ApiResponse<MarkupTransDto>>(
      `/v1/MarkupTrans/document/${documentType}/${documentRecId}`,
      toDto(record),
      { params: { level } }
    );
    return toRecord(requireData(response.data, 'The charge could not be created.'));
  },
  async update(record: MarkupTransRecord) {
    const response = await apiClient.put<ApiResponse<MarkupTransDto>>(
      `/v1/MarkupTrans/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data, 'The charge could not be saved.'));
  },
  async delete(record: MarkupTransRecord) {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/v1/MarkupTrans/${record.recId}`);
    requireData(response.data, 'The charge could not be deleted.');
  },
};
