import { describe, expect, it } from 'vitest';
import { localizedName } from '@shared/utilities/localizedName';

describe('localizedName', () => {
  const value = { name: 'Payment Request', nameAlias: 'طلب صرف' };

  it('uses Name for LTR', () => {
    expect(localizedName(value, false)).toBe('Payment Request');
  });

  it('uses NameAlias for RTL', () => {
    expect(localizedName(value, true)).toBe('طلب صرف');
  });

  it('falls back to Name when the RTL alias is empty', () => {
    expect(localizedName({ name: 'Payment Request', nameAlias: ' ' }, true)).toBe(
      'Payment Request'
    );
  });
});
