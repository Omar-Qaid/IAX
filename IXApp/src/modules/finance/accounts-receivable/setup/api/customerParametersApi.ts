import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';
import { requireApiData } from '@core/api/apiResponseData';
type ParameterValues = Record<string, string | number | boolean>;

export const parameterTextFields = [
  'custPostingProfile',
  'prepaymentPostingProfile',
  'paymTermId',
  'paymMode',
  'dlvMode',
  'dlvReasonId',
  'currencyCode',
  'taxGroup',
  'invoiceJournalName',
  'packingSlipJournalName',
  'custNumSeqGroup',
  'priceDiscJournalNamePrice',
  'priceDiscJournalNameLineDisc',
  'priceDiscJournalNameMultilineDisc',
  'priceDiscJournalNameTotalDisc',
] as const;
export const parameterBooleanFields = [
  'taxOnPrepayment',
  'priceDiscSearchPrice',
  'priceDiscSearchLineDisc',
  'priceDiscSearchMultilineDisc',
  'priceDiscSearchTotalDisc',
  'priceDiscMandatory',
] as const;
export interface CustomerParametersRecord extends Record<string, unknown> {
  recId: number;
  key: number;
  dataAreaId: string;
  rowVersion: string | null;
  recVersion: number;
}
const endpoint = '/v1/CustParameters';
export function parameterValues(record: CustomerParametersRecord): ParameterValues {
  return {
    ...Object.fromEntries(parameterTextFields.map((field) => [field, String(record[field] ?? '')])),
    ...Object.fromEntries(
      parameterBooleanFields.map((field) => [
        field,
        record[field] === 1 || record[field] === 'Yes' || record[field] === true,
      ])
    ),
    creditLimit: Number(record.creditLimit ?? 0),
  };
}
export const customerParametersApi = {
  async get(company: string, signal?: AbortSignal): Promise<CustomerParametersRecord | null> {
    const response = await apiClient.get<ApiResponse<CustomerParametersRecord[]>>(endpoint, {
      signal,
      headers: { 'X-Company': company },
    });
    const rows = requireApiData(response.data, 'customer parameters');
    if (!Array.isArray(rows)) throw new ApiError('Invalid customer parameters response.', 500);
    const matches = rows.filter(
      (row) => row.key === 0 && row.dataAreaId?.toLowerCase() === company.toLowerCase()
    );
    if (matches.length > 1)
      throw new ApiError('Multiple customer parameter records exist for this company.', 500);
    const record = matches[0] ?? null;
    if (record && (!Number.isSafeInteger(record.recId) || record.recId <= 0))
      throw new ApiError('Invalid customer parameters record.', 500);
    return record;
  },
  async save(
    record: CustomerParametersRecord,
    values: ParameterValues
  ): Promise<CustomerParametersRecord> {
    // Preserve fields not exposed by this form, including identity and concurrency metadata.
    const payload = { ...record };
    for (const field of parameterTextFields) payload[field] = String(values[field] ?? '');
    for (const field of parameterBooleanFields) payload[field] = values[field] ? 1 : 0;
    payload.creditLimit = Number(values.creditLimit);
    const response = await apiClient.put<ApiResponse<CustomerParametersRecord>>(
      `${endpoint}/${record.recId}`,
      payload,
      { headers: { 'X-Company': record.dataAreaId } }
    );
    const saved = requireApiData(response.data, 'customer parameters');
    if (
      saved.recId !== record.recId ||
      saved.key !== record.key ||
      saved.dataAreaId?.toLowerCase() !== record.dataAreaId.toLowerCase()
    )
      throw new ApiError('Invalid customer parameters save response.', 500);
    return saved;
  },
};
