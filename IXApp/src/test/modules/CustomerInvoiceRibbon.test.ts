import { describe, expect, it, vi } from 'vitest';
import { buildCustomerInvoiceRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerInvoiceRibbon';

const tr = (_: string, fallback: string) => fallback;

describe('customer Invoice ribbon', () => {
  it('lays the tab out the way D365 does', () => {
    const groups = buildCustomerInvoiceRibbonGroups(tr, {
      onNewFreeTextInvoice: vi.fn(),
      onFreeTextInvoices: vi.fn(),
      onInvoiceJournal: vi.fn(),
    });
    expect(groups.map((group) => group.id)).toEqual([
      'new',
      'setup',
      'journals',
      'related-information',
      'prepayment',
    ]);
  });

  it('runs every wired destination and leaves the rest disabled with a tooltip', () => {
    const newInvoice = vi.fn();
    const invoices = vi.fn();
    const journal = vi.fn();
    const groups = buildCustomerInvoiceRibbonGroups(tr, {
      onNewFreeTextInvoice: newInvoice,
      onFreeTextInvoices: invoices,
      onInvoiceJournal: journal,
    });
    const action = (groupId: string, actionId: string) =>
      groups.find((group) => group.id === groupId)!.actions.find((item) => item.id === actionId)!;

    action('new', 'free-text-invoice').onClick!();
    expect(newInvoice).toHaveBeenCalledOnce();
    action('related-information', 'free-text-invoices').onClick!();
    expect(invoices).toHaveBeenCalledOnce();
    action('journals', 'invoice-journal').onClick!();
    expect(journal).toHaveBeenCalledOnce();

    for (const [groupId, actionId] of [
      ['setup', 'recurring-invoices'],
      ['journals', 'bill-of-exchange'],
      ['related-information', 'period-statistics'],
      ['related-information', 'cod-outstanding'],
      ['related-information', 'bill-of-exchange-statistics'],
      ['prepayment', 'apply-prepayment'],
    ] as const) {
      const entry = action(groupId, actionId);
      expect(entry.disabled).toBe(true);
      expect(entry.tooltip).toBeTruthy();
      expect(entry.onClick).toBeUndefined();
    }
  });

  it('hides New and disables the journal when the user lacks those permissions', () => {
    const groups = buildCustomerInvoiceRibbonGroups(tr, {});
    expect(groups.map((group) => group.id)).toEqual([
      'setup',
      'journals',
      'related-information',
      'prepayment',
    ]);
    const journal = groups
      .find((group) => group.id === 'journals')!
      .actions.find((item) => item.id === 'invoice-journal')!;
    expect(journal.disabled).toBe(true);
    expect(journal.onClick).toBeUndefined();
    expect(
      groups.find((group) => group.id === 'related-information')!.actions.map((item) => item.id)
    ).toEqual(['period-statistics', 'cod-outstanding', 'bill-of-exchange-statistics']);
  });
});
