import { createEntityApi } from '@core/api/createEntityApi';
export type CustomerPaymentSetupKind = 'terms' | 'methods';
export interface CustomerPaymentSetupRecord {
  id: string;
  recId: number;
  code: string;
  description: string;
  days: number;
  months: number;
  source: Record<string, unknown>;
}
interface Dto extends Record<string, unknown> {
  recId: number;
}
export const customerPaymentSetupApi = (kind: CustomerPaymentSetupKind) => {
  const term = kind === 'terms';
  return createEntityApi<Dto, CustomerPaymentSetupRecord>({
    endpoint: term ? '/v1/PaymTerm' : '/v1/CustPaymMode',
    resourceName: term ? 'payment terms' : 'customer payment methods',
    toRecord: (dto) => ({
      id: String(dto.recId),
      recId: dto.recId,
      code: String(dto[term ? 'paymTermId' : 'paymMode'] ?? ''),
      description: String(dto[term ? 'description' : 'name'] ?? ''),
      days: Number(dto[term ? 'numOfDays' : 'discGraceDays'] ?? 0),
      months: Number(dto.numOfMonths ?? 0),
      source: dto,
    }),
    toDto: (record) => ({
      ...record.source,
      recId: record.recId,
      [term ? 'paymTermId' : 'paymMode']: record.code.trim(),
      [term ? 'description' : 'name']: record.description.trim(),
      [term ? 'numOfDays' : 'discGraceDays']: record.days,
      ...(term ? { numOfMonths: record.months } : {}),
    }),
  });
};
