import { describe, expect, it } from 'vitest';
import { settingsQueryKeys } from '@modules/administration/queries/settingsQueryKeys';
import {
  customerGroupQueryKeys,
  customerQueryKeys,
  salesOrderQueryKeys,
} from '@modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys';
import { currencyQueryKeys } from '@modules/finance/foundation/queries/currencyQueryKeys';

describe('module-owned query keys', () => {
  it('keeps administration query keys scoped and stable', () => {
    expect(settingsQueryKeys.global()).toEqual(['settings', 'global']);
    expect(settingsQueryKeys.user()).toEqual(['settings', 'user']);
  });

  it('keeps accounts-receivable query keys scoped and parameterized', () => {
    expect(customerQueryKeys.list({ page: 2 })).toEqual(['customers', 'list', { page: 2 }]);
    expect(customerGroupQueryKeys.detail('retail')).toEqual(['customerGroups', 'detail', 'retail']);
    expect(salesOrderQueryKeys.detail('SO-001')).toEqual(['salesOrders', 'detail', 'SO-001']);
  });

  it('keeps foundation query keys inside the foundation module', () => {
    expect(currencyQueryKeys.list({ active: true })).toEqual([
      'currencies',
      'list',
      { active: true },
    ]);
  });
});
