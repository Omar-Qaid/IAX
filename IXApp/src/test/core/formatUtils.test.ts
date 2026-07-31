import { describe, it, expect } from 'vitest';
import { formatCurrency, formatNumber } from '@core/utilities/formatUtils';

describe('formatUtils', () => {
  it('formats currency correctly', () => {
    const formatted = formatCurrency(1234.56, 'USD');
    expect(formatted).toContain('1,234.56');
  });

  it('formats numbers correctly', () => {
    const formatted = formatNumber(1000, 0);
    expect(formatted).toBe('1,000');
  });
});
