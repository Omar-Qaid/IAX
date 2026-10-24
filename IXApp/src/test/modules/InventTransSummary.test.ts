import { describe, expect, it } from 'vitest';
import type { InventTransRecord } from '../../modules/finance/inventory/api/inventTransApi';
import { summarizeInventTransRows } from '../../modules/finance/inventory/components/summarizeInventTransRows';

const transaction = (overrides: Partial<InventTransRecord>): InventTransRecord => ({
  itemNumber: 'ITEM-A',
  currencyCode: 'SAR',
  quantity: 0,
  costAmount: 0,
  receiptStatus: '',
  issueStatus: '',
  ...overrides,
} as InventTransRecord);

describe('inventory transaction summary', () => {
  it('totals transactions for one item and currency', () => {
    expect(summarizeInventTransRows([
      transaction({ quantity: 10, costAmount: 100, receiptStatus: 'Received' }),
      transaction({ quantity: -3, costAmount: -30, issueStatus: 'Sold' }),
    ])).toEqual({ quantity: 7, costAmount: 70, receipts: 1, issues: 1 });
  });

  it('does not combine quantities of different items', () => {
    const summary = summarizeInventTransRows([
      transaction({ itemNumber: 'ITEM-A', quantity: 10, costAmount: 100 }),
      transaction({ itemNumber: 'ITEM-B', quantity: 5, costAmount: 50 }),
    ]);
    expect(summary.quantity).toBeNull();
    expect(summary.costAmount).toBe(150);
  });

  it('does not combine costs of different currencies', () => {
    const summary = summarizeInventTransRows([
      transaction({ quantity: 10, costAmount: 100, currencyCode: 'SAR' }),
      transaction({ quantity: 5, costAmount: 50, currencyCode: 'USD' }),
    ]);
    expect(summary.quantity).toBe(15);
    expect(summary.costAmount).toBeNull();
  });
});
