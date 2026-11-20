import type { FreeTextInvoiceLine } from '../api/freeTextInvoiceApi';

export type Line = FreeTextInvoiceLine & { id: string; ledgerDisplay?: string };
export const isValidInvoiceLine = (line: Line) =>
  Boolean(line.description.trim()) &&
  Number.isFinite(line.quantity) &&
  line.quantity > 0 &&
  Number.isFinite(line.unitPrice) &&
  line.unitPrice >= 0 &&
  Number.isSafeInteger(line.ledgerDimension) &&
  line.ledgerDimension > 0;
export const newLine = (): Line => ({
  id: crypto.randomUUID(),
  recId: 0,
  lineNum: 1,
  description: '',
  quantity: 1,
  unitPrice: 0,
  netAmount: 0,
  taxAmount: 0,
  grossAmount: 0,
  ledgerDimension: 0,
  defaultDimension: 0,
  taxGroup: '',
  taxItemGroup: '',
});
