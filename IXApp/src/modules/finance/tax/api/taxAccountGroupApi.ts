import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TaxAccountGroupRecord {
  id: string;
  recId: number;
  taxAccountGroup: string;
  name: string;
  [field: string]: string | number | null;
}
export interface TaxLedgerAccount {
  [key: string]: unknown;
  id: number;
  code: string;
  name: string;
}
const endpoint = '/v1/TaxLedgerAccountGroup';
function data<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The tax posting group request failed.', 500);
  return response.data;
}
const record = (dto: Omit<TaxAccountGroupRecord, 'id'>): TaxAccountGroupRecord =>
  ({ ...dto, id: String(dto.recId) }) as TaxAccountGroupRecord;
const payload = ({ id: _id, ...value }: TaxAccountGroupRecord) => ({
  ...value,
  taxAccountGroup: value.taxAccountGroup.trim(),
  name: value.name.trim(),
});
export const taxAccountGroupApi = {
  async list(signal?: AbortSignal) {
    return data(
      (await apiClient.get<ApiResponse<TaxAccountGroupRecord[]>>(endpoint, { signal })).data
    ).map(record);
  },
  async accounts(signal?: AbortSignal) {
    return data(
      (
        await apiClient.get<ApiResponse<TaxLedgerAccount[]>>(`${endpoint}/ledger-accounts`, {
          signal,
        })
      ).data
    );
  },
  async create(value: TaxAccountGroupRecord) {
    return record(
      data(
        (await apiClient.post<ApiResponse<TaxAccountGroupRecord>>(endpoint, payload(value))).data
      )
    );
  },
  async update(value: TaxAccountGroupRecord) {
    return record(
      data(
        (
          await apiClient.put<ApiResponse<TaxAccountGroupRecord>>(
            `${endpoint}/${value.recId}`,
            payload(value)
          )
        ).data
      )
    );
  },
  async delete(value: TaxAccountGroupRecord) {
    data((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${value.recId}`)).data);
  },
};
