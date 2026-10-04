import { describe, expect, it } from 'vitest';
import {
  salesLineNetAmount,
  synchronizeSalesLineDiscount,
} from '../../modules/finance/accounts-receivable/utils/salesLineDiscount';

const line = { quantity: 10, unitPrice: 100, lineDiscount: 0, lineDiscountPercent: 0 };

describe('sales line discount', () => {
  it('calculates the discount amount from the percentage', () => {
    expect(synchronizeSalesLineDiscount(line, 'lineDiscountPercent', 15)).toMatchObject({
      lineDiscount: 150,
      lineDiscountPercent: 15,
    });
  });

  it('calculates the percentage from the discount amount', () => {
    expect(synchronizeSalesLineDiscount(line, 'lineDiscount', 125)).toMatchObject({
      lineDiscount: 125,
      lineDiscountPercent: 12.5,
    });
  });

  it('recalculates the amount and net value when quantity changes', () => {
    const discounted = synchronizeSalesLineDiscount(line, 'lineDiscountPercent', 10);
    const resized = synchronizeSalesLineDiscount(discounted, 'quantity', 20);
    expect(resized.lineDiscount).toBe(200);
    expect(salesLineNetAmount(resized)).toBe(1800);
  });
});
