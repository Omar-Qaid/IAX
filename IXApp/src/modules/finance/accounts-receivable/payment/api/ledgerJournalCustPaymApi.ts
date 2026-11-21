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
export const ledgerJournalCustPaymApi = {
  headers,
  lines,
  async list(signal?: AbortSignal) {
    return (await headers.list(signal)).filter((row) => row.journalType === 1);
  },
  async get(recId: string, signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<Omit<PaymentJournal, 'id'>>>(
      `${headerEndpoint}/${encodeURIComponent(recId)}`,
      { signal }
    );
    const row = record<PaymentJournal>(requireApiData(response.data, 'payment journal'));
    if (row.journalType !== 1) throw new Error('This journal is not a customer payment journal.');
    return row;
  },
  async journalLines(journalNum: string, signal?: AbortSignal) {
    return (await lines.list(signal)).filter((row) => row.journalNum === journalNum);
  },
  async names(signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<PaymentJournalName[]>>(
      '/v1/LedgerJournalName',
      { signal }
    );
    return requireApiData(response.data, 'journal names').filter((row) => row.journalType === 1);
  },
  async offsets(signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<OffsetAccount[]>>(
      `${headerEndpoint}/offset-accounts`,
      { signal }
    );
    return requireApiData(response.data, 'offset accounts');
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
export function newPaymentJournal(company: string): PaymentJournal {
  return {
    id: '',
    recId: 0,
    dataAreaId: company,
    journalNum: '',
    journalName: '',
    name: '',
    journalType: 1,
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
export function newPaymentJournalLine(header: PaymentJournal): PaymentJournalLine {
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
    accountType: 1,
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
