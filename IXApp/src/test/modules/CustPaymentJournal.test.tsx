import { expect, it, vi } from 'vitest';
import { buildCustomerCollectRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerCollectRibbon';

it('enables the payment journal ribbon action only when a handler is supplied', () => {
  const withoutHandler = buildCustomerCollectRibbonGroups((_key, fallback) => fallback);
  const withHandler = buildCustomerCollectRibbonGroups((_key, fallback) => fallback, {
    onPaymentJournal: vi.fn(),
  });
  const find = (groups: ReturnType<typeof buildCustomerCollectRibbonGroups>) =>
    groups.flatMap((group) => group.actions).find((action) => action.id === 'paymentJournal');

  expect(find(withoutHandler)?.disabled).toBe(true);
  expect(find(withHandler)?.disabled).toBe(false);
  expect(find(withHandler)?.onClick).toBeTypeOf('function');
});
