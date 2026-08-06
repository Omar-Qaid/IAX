import { describe, expect, it } from 'vitest';
import { deepEqual } from '@shared/utils/deepEqual';

describe('deepEqual', () => {
  it('compares nested form values independently of object key order', () => {
    expect(deepEqual({ name: 'Contoso', options: { enabled: true, limit: 2 } }, {
      options: { limit: 2, enabled: true },
      name: 'Contoso',
    })).toBe(true);
  });

  it('supports dates and detects changed array values', () => {
    expect(deepEqual({ date: new Date('2026-01-01'), values: [1, 2] }, {
      date: new Date('2026-01-01'),
      values: [1, 3],
    })).toBe(false);
  });
});
