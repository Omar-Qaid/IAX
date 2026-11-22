import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { requireApiData } from '@core/api/apiResponseData';
import { createEntityApi } from '@core/api/createEntityApi';
import type { LookupOption } from '@shared/components/lookups/types';

interface JournalRecord {
  id: string;
  recId: number;
  dataAreaId: string;
  rowVersion?: string | null;
  createdBy?: string;
  createdAt?: string;
  lastModifiedBy?: string;
  lastModifiedAt?: string;
}
export interface PaymentJournal extends JournalRecord {
  journalNum: string;
  journalName: string;
  name: string;
  journalType: number;
  posted: number;
  postedDateTime: string;
  systemBlocked: number;
  currencyCode: string;
  offsetAccountType: number;
  offsetLedgerDimension: number;
  defaultDimension: number;
  fixedOffsetAccount: number;
  numOfLines: number;
  // Further LedgerJournalTable fields the API returns (header tabs show them).
  voucherAllocatedAtPosting?: number;
  ledgerJournalInclTax?: number;
  exchRate?: number;
  exchRateSecondary?: number;
  fixedExchRate?: number;
  reportingCurrencyExchRate?: number;
  reportingCurrencyExchRateSecondary?: number;
  reportingCurrencyFixedExchRate?: number;
  originalJournalNum?: string;
  journalTotalDebit?: number;
  journalTotalCredit?: number;
  journalBalance?: number;
  // Header tab fields (read-only through the API; the server keeps them on update).
  currentOperationsTax?: number;
  detailSummaryPosting?: number;
  linesLimitBeforeDistribution?: number;
  parentJournalNum?: string;
  documentNum?: string;
  taxObligationCompany?: number;
  bankTransSummarizationEnabled?: number;
  bankTransSummarizationCriteria?: number;
  sessionId?: number;
  systemBlockedReason?: number;
  workflowApprovalStatus?: number;
  originalCompany?: string;
  log?: string;
}
export interface PaymentJournalLine extends JournalRecord {
  journalNum: string;
  lineNum: number;
  voucher: string;
  transDate: string;
  company: string;
  accountType: number;
  ledgerDimension: number;
  ledgerDimensionName: string;
  defaultDimension: number;
  amountCurDebit: number;
  amountCurCredit: number;
  currencyCode: string;
  offsetAccountType: number;
  offsetLedgerDimension: number;
  offsetDefaultDimension: number;
  offsetCompany: string;
  txt: string;
  invoice: string;
  paymMode: string;
  paymReference: string;
  postingProfile: string;
  payment: string;
  documentNum: string;
  documentDate: string;
  due: string;
  settleVoucher: number;
  markedInvoiceRecId: number;
  markedInvoiceCompany: string;
  noEdit: number;
  // Further LedgerJournalTrans fields the API returns (line tabs show them, read-only).
  exchRate?: number;
  exchRateSecond?: number;
  reportingCurrencyExchRate?: number;
  reportingCurrencyExchRateSecondary?: number;
  taxGroup?: string;
  taxItemGroup?: string;
  taxCode?: string;
  cashDiscCode?: string;
  cashDiscBaseDate?: string;
  prepayment?: number;
  reverseEntry?: number;
  reverseDate?: string;
  qty?: number;
  releaseDate?: string;
  transfer?: number;
  transferred?: number;
  transferredBy?: string;
  transferredOn?: string;
  transferredTo?: string;
  taxWithholdGroup?: string;
  taxDirectionControl?: number;
  cashDiscAmount?: number;
  dateCashDisc?: string;
  vatNumJournal?: string;
  vendorVatDate?: string;
  approved?: number;
  approver?: number;
  paymSpec?: string;
  bankTransType?: string;
  bankDepositVoucher?: number;
  bankDepositNum?: string;
  foreignCompany?: string;
  foreignVoucher?: string;
  reasonRefRecId?: number;
  paymentStatus?: number;
}
export interface PaymentJournalName {
  recId: number;
  journalName: string;
  name: string;
  journalType: number;
  currencyCode: string;
  offsetAccountType: number;
  offsetLedgerDimension: number;
  defaultDimension: number;
  fixedOffsetAccount: number;
}
export interface OffsetAccount extends LookupOption {
  accountType: number;
}
export interface JournalValidation {
  valid: boolean;
  errors: string[];
  unverifiedChecks: string[];
  scope: string;
}
export interface JournalSimulation {
  wouldPost: boolean;
  refusal: string | null;
  totalDebit: number;
  totalCredit: number;
  scope: string;
}
const headerEndpoint = '/v1/LedgerJournalTable';
/** An API started before the scoped endpoints existed answers them with 404. */
// The API client rejects with an ApiError (`status`); raw axios errors carry `response.status`.
const isNotFound = (error: unknown) => {
  const value = error as { status?: number; response?: { status?: number } } | null;
  return value?.status === 404 || value?.response?.status === 404;
};
const lineEndpoint = '/v1/LedgerJournalTrans';
const record = <T extends JournalRecord>(dto: Omit<T, 'id'>): T =>
  ({ ...dto, id: String(dto.recId) }) as T;
// Keep every loaded DTO field (including concurrency tokens), not just visible columns.
const dto = <T extends JournalRecord>({ id: _id, ...value }: T): Omit<T, 'id'> => value;
const headers = createEntityApi<Omit<PaymentJournal, 'id'>, PaymentJournal>({
  endpoint: headerEndpoint,
  resourceName: 'payment journals',
  toRecord: record<PaymentJournal>,
  toDto: dto,
});
const lines = createEntityApi<Omit<PaymentJournalLine, 'id'>, PaymentJournalLine>({
  endpoint: lineEndpoint,
  resourceName: 'payment journal lines',
  toRecord: record<PaymentJournalLine>,
  toDto: dto,
});
/** D365 LedgerJournalType: 0 = Daily (general journal), 1 = customer payment, 2 = vendor payment. */
export type LedgerJournalTypeId = 0 | 1 | 2;
const journalTypeNames: Record<LedgerJournalTypeId, string> = {
  0: 'general journal',
  1: 'customer payment journal',
  2: 'vendor payment journal',
};

/** The journal API of one journal type: lists, reads and names only journals of that type. */
function createLedgerJournalApi(journalType: LedgerJournalTypeId) {
  return {
    headers,
    lines,
    journalType,
    /** Journals of this type only, filtered on the server. */
    async list(signal?: AbortSignal) {
      const response = await apiClient
        .get<ApiResponse<Omit<PaymentJournal, 'id'>[]>>(
          `${headerEndpoint}/by-type/${journalType}`,
          { signal }
        )
        .catch(async (error: unknown) => {
          // Older API: load every journal and filter here.
          if (isNotFound(error))
            return apiClient.get<ApiResponse<Omit<PaymentJournal, 'id'>[]>>(headerEndpoint, {
              signal,
            });
          throw error;
        });
      // Keep the type guard: the page must never show other journal types.
      return requireApiData(response.data, 'payment journals')
        .filter((row) => row.journalType === journalType)
        .map(record<PaymentJournal>);
    },
    async get(recId: string, signal?: AbortSignal) {
      const response = await apiClient.get<ApiResponse<Omit<PaymentJournal, 'id'>>>(
        `${headerEndpoint}/${encodeURIComponent(recId)}`,
        { signal }
      );
      const row = record<PaymentJournal>(requireApiData(response.data, 'payment journal'));
      if (row.journalType !== journalType)
        throw new Error(`This journal is not a ${journalTypeNames[journalType]}.`);
      return row;
    },
    /** Only this journal's lines (server-filtered), not every journal line of the company. */
    async journalLines(journalNum: string, signal?: AbortSignal) {
      try {
        const response = await apiClient.get<ApiResponse<Omit<PaymentJournalLine, 'id'>[]>>(
          `${lineEndpoint}/by-journal/${encodeURIComponent(journalNum)}`,
          { signal }
        );
        return requireApiData(response.data, 'payment journal lines').map(
          record<PaymentJournalLine>
        );
      } catch (error) {
        // Older API: load every line and keep this journal's.
        if (!isNotFound(error)) throw error;
        return (await lines.list(signal)).filter((row) => row.journalNum === journalNum);
      }
    },
    async names(signal?: AbortSignal) {
      const response = await apiClient.get<ApiResponse<PaymentJournalName[]>>(
        '/v1/LedgerJournalName',
        { signal }
      );
      return requireApiData(response.data, 'journal names').filter(
        (row) => row.journalType === journalType
      );
    },
    async offsets(signal?: AbortSignal) {
      try {
        const response = await apiClient.get<ApiResponse<OffsetAccount[]>>(
          `${headerEndpoint}/offset-accounts`,
          { signal }
        );
        return requireApiData(response.data, 'offset accounts');
      } catch (error) {
        // An older API reads "offset-accounts" as a journal id: "LedgerJournalTable not found".
        if (isNotFound(error))
          throw new Error('Offset accounts are not available: restart the API to load them.', {
            cause: error,
          });
        throw error;
      }
    },
    async validate(recId: number): Promise<JournalValidation> {
      return (await apiClient.get<JournalValidation>(`${headerEndpoint}/${recId}/validate`)).data;
    },
    async simulate(recId: number): Promise<JournalSimulation> {
      return (await apiClient.post<JournalSimulation>(`${headerEndpoint}/${recId}/simulate`)).data;
    },
    async post(recId: number): Promise<void> {
      await apiClient.post(`${headerEndpoint}/${recId}/post`);
    },
  };
}
export type LedgerJournalApi = ReturnType<typeof createLedgerJournalApi>;
const journalApis = new Map<LedgerJournalTypeId, LedgerJournalApi>();
/** One shared API object per journal type (pages and tests see the same instance). */
export function ledgerJournalApiFor(journalType: LedgerJournalTypeId): LedgerJournalApi {
  let api = journalApis.get(journalType);
  if (!api) {
    api = createLedgerJournalApi(journalType);
    journalApis.set(journalType, api);
  }
  return api;
}
export const ledgerJournalCustPaymApi = ledgerJournalApiFor(1);
export function newPaymentJournal(
  company: string,
  journalType: LedgerJournalTypeId = 1
): PaymentJournal {
  return {
    id: '',
    recId: 0,
    dataAreaId: company,
    journalNum: '',
    journalName: '',
    name: '',
    journalType,
    posted: 0,
    postedDateTime: '0001-01-01T00:00:00',
    systemBlocked: 0,
    currencyCode: '',
    offsetAccountType: 0,
    offsetLedgerDimension: 0,
    defaultDimension: 0,
    fixedOffsetAccount: 0,
    numOfLines: 0,
  };
}
/** A new line; `accountType` is the D365 LedgerJournalACType (0 ledger, 1 customer, 2 vendor). */
export function newPaymentJournalLine(header: PaymentJournal, accountType = 1): PaymentJournalLine {
  const now = new Date();
  const date = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  return {
    id: '',
    recId: 0,
    dataAreaId: header.dataAreaId,
    journalNum: header.journalNum,
    lineNum: 0,
    voucher: '',
    transDate: date,
    company: header.dataAreaId,
    accountType,
    ledgerDimension: 0,
    ledgerDimensionName: '',
    defaultDimension: header.defaultDimension,
    amountCurDebit: 0,
    amountCurCredit: 0,
    currencyCode: header.currencyCode,
    offsetAccountType: header.offsetAccountType,
    offsetLedgerDimension: header.offsetLedgerDimension,
    offsetDefaultDimension: 0,
    offsetCompany: header.dataAreaId,
    txt: header.name,
    invoice: '',
    paymMode: '',
    paymReference: '',
    postingProfile: '',
    payment: '',
    documentNum: '',
    documentDate: date,
    due: date,
    settleVoucher: 0,
    markedInvoiceRecId: 0,
    markedInvoiceCompany: '',
    noEdit: 0,
  };
}
/** Same amount to the cent (avoids floating point noise when matching invoices). */
export const sameAmount = (a: number, b: number) => Math.round(a * 100) === Math.round(b * 100);

export function paymentJournalTotals(rows: PaymentJournalLine[]) {
  // Each entered side with an offset contributes an equal opposite accounting side.
  const debit = rows.reduce(
    (sum, row) => sum + row.amountCurDebit + (row.offsetLedgerDimension ? row.amountCurCredit : 0),
    0
  );
  const credit = rows.reduce(
    (sum, row) => sum + row.amountCurCredit + (row.offsetLedgerDimension ? row.amountCurDebit : 0),
    0
  );
  return { debit, credit, balance: debit - credit };
}
