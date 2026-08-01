import { describe, it, expect } from 'vitest';
import { MOCK_CUSTOMERS } from '@mocks/data/customers';
import { MOCK_CURRENCIES } from '@mocks/data/currencies';

describe('Mock Services and Data Layer Architecture', () => {
  it('provides typed mock customer dataset', () => {
    expect(Array.isArray(MOCK_CUSTOMERS)).toBe(true);
    expect(MOCK_CUSTOMERS.length).toBeGreaterThan(0);
    expect(MOCK_CUSTOMERS[0].accountNumber).toBeDefined();
  });

  it('provides typed mock currencies dataset', () => {
    expect(Array.isArray(MOCK_CURRENCIES)).toBe(true);
    expect(MOCK_CURRENCIES.length).toBeGreaterThan(0);
  });
});
