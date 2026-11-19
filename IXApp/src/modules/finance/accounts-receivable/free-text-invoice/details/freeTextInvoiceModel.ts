import type { FreeTextInvoiceRecord } from '../api/freeTextInvoiceApi';
import { newLine, type Line } from '../lines/freeTextInvoiceLine';

export type Invoice = Omit<FreeTextInvoiceRecord, 'lines'> & { lines: Line[] };
export const asInvoice = (record: FreeTextInvoiceRecord): Invoice => ({
  ...record,
  lines: record.lines.map((line) => ({ ...line, id: String(line.recId) })),
});
const localDate = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};
export const newInvoice = (customerAccount: string): Invoice => ({
  id: 'new',
  recId: 0,
  invoiceId: '',
  customerAccount,
  invoiceAccount: '',
  description: '',
  invoiceDate: localDate(),
  dueDate: '',
  currencyCode: '',
  paymentTerms: '',
  paymentMethod: '',
  postingProfile: '',
  defaultDimension: 0,
  taxGroup: '',
  posted: false,
  netAmount: 0,
  taxAmount: 0,
  invoiceAmount: 0,
  lines: [newLine()],
});
