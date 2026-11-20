import { describe, expect, it, vi } from 'vitest';
import { buildCustomerSellRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerSellRibbon';
const tr = (_: string, fallback: string) => fallback;
describe('customer Sell ribbon', () => {
  it('keeps New order separate from existing Orders and omits destinations without permission', () => {
    const create = vi.fn(),
      orders = vi.fn();
    const groups = buildCustomerSellRibbonGroups(tr, {
      onNewSalesOrder: create,
      onSalesOrders: orders,
    });
    groups.find((group) => group.id === 'new')!.actions[0].onClick!();
    expect(create).toHaveBeenCalledOnce();
    expect(orders).not.toHaveBeenCalled();
    groups.find((group) => group.id === 'related-information')!.actions[0].items![0].onClick();
    expect(orders).toHaveBeenCalledOnce();
    expect(groups.map((group) => group.id)).toEqual(['new', 'related-information']);
    expect(buildCustomerSellRibbonGroups(tr, {})).toEqual([]);
  });
});

it('offers order lines independently of all sales orders', () => {
  const lines = vi.fn();
  const groups = buildCustomerSellRibbonGroups(tr, { onOrderLines: lines });
  const items = groups[0].actions[0].items!;
  expect(items.map((item) => item.id)).toEqual([
    'order-lines',
    'confirmation-journal',
    'backorder-lines',
  ]);
  expect(items.slice(1).every((item) => item.disabled)).toBe(true);
  items[0].onClick();
  expect(lines).toHaveBeenCalledOnce();
});
