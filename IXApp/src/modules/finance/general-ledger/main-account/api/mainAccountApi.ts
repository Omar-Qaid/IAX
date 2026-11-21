import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

/** `MainAccount`, as the chart of accounts form reads and writes it. */
export interface MainAccountRecord {
  id: string;
  recId: number;
  mainAccountId: string;
  name: string;
  type: number;
  currencyCode: string;
  accountCategoryRef: number;
  ledgerChartOfAccounts: number;
  offsetLedgerDimension: number;
  openingAccount: number;
  postingType: number;
  debitCreditProposal: number;
  debitCreditCheck: number;
  debitCreditBalanceDemand: number;
  mandatoryPaymentReference: number;
  validateCurrency: number;
  validatePosting: number;
  validateUser: number;
  userId: string;
  activeFrom: string | null;
  activeTo: string | null;
  suspended: number;
  closeType: number;
  closing: number;
  monetary: number;
  exchangeAdjustmentRateType: number;
  reportingExchangeAdjustmentRateType: number;
  reportingAccountType: number;
  consolidationMainAccount: string;
  groupLevel01: string;
  groupLevel02: string;
  groupLevel03: string;
}

const endpoint = '/v1/MainAccount';

function unwrap<T>(response: ApiResponse<T>, fallback: string): T {
  if (!response.success || response.data == null) throw new ApiError(response.message || fallback, 500);
  return response.data;
}

/** The server keys records by RecId; the pattern wants a string id. */
const toRecord = (row: MainAccountRecord): MainAccountRecord => ({
  ...row,
  id: String(row.recId ?? row.id ?? ''),
});

/** `LedgerChartOfAccounts`, named in the form's title. */
export interface LedgerChartOfAccountsRecord {
  recId: number;
  name: string;
  description: string;
}

export const mainAccountApi = {
  async charts(signal?: AbortSignal): Promise<LedgerChartOfAccountsRecord[]> {
    const { data } = await apiClient.get<ApiResponse<LedgerChartOfAccountsRecord[]>>(
      '/v1/LedgerChartOfAccounts',
      { signal }
    );
    return unwrap(data, 'Unable to load the chart of accounts.');
  },
  async list(signal?: AbortSignal): Promise<MainAccountRecord[]> {
    const { data } = await apiClient.get<ApiResponse<MainAccountRecord[]>>(endpoint, { signal });
    return unwrap(data, 'Unable to load main accounts.').map(toRecord);
  },
  async update(id: string, record: Partial<MainAccountRecord>): Promise<MainAccountRecord> {
    const { data } = await apiClient.put<ApiResponse<MainAccountRecord>>(
      `${endpoint}/${encodeURIComponent(id)}`,
      record
    );
    return toRecord(unwrap(data, 'The main account could not be saved.'));
  },
  async create(record: Partial<MainAccountRecord>): Promise<MainAccountRecord> {
    const { data } = await apiClient.post<ApiResponse<MainAccountRecord>>(endpoint, record);
    return toRecord(unwrap(data, 'The main account could not be created.'));
  },
  async remove(id: string): Promise<void> {
    const { data } = await apiClient.delete<ApiResponse<boolean>>(
      `${endpoint}/${encodeURIComponent(id)}`
    );
    if (!data.success) throw new ApiError(data.message || 'The main account could not be deleted.', 500);
  },
};
