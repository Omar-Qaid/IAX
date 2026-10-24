import type { InventTransRecord } from '../api/inventTransApi';

export function summarizeInventTransRows(rows: InventTransRecord[]) {
  const items = new Set(rows.map((row) => row.itemNumber));
  const currencies = new Set(rows.map((row) => row.currencyCode));
  return {
    quantity: items.size <= 1 ? rows.reduce((total, row) => total + row.quantity, 0) : null,
    costAmount: currencies.size <= 1 ? rows.reduce((total, row) => total + row.costAmount, 0) : null,
    receipts: rows.filter((row) => Boolean(row.receiptStatus)).length,
    issues: rows.filter((row) => Boolean(row.issueStatus)).length,
  };
}
