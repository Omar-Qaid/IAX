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
    // Company-scoped; every key of one order sits under order(company, id).
    expect(salesOrderQueryKeys.detail('DAT', '1')).toEqual([
      'salesOrders',
      'DAT',
      'order',
      '1',
      'header',
    ]);
    expect(salesOrderQueryKeys.lines('DAT', '1').slice(0, 4)).toEqual([
      ...salesOrderQueryKeys.order('DAT', '1'),
    ]);
    expect(salesOrderQueryKeys.list('DAT')).not.toEqual(salesOrderQueryKeys.list('USMF'));
  });

  it('keeps foundation query keys inside the foundation module', () => {
    expect(currencyQueryKeys.list({ active: true })).toEqual([
      'currencies',
      'list',
      { active: true },
    ]);
  });
});
