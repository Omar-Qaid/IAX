import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';

interface CustomerInvoiceActions {
  onNewFreeTextInvoice?: () => void;
  onFreeTextInvoices?: () => void;
  onInvoiceJournal?: () => void;
}

/**
 * The customer's Invoice tab, laid out as D365 does: New, Set up, Journals,
 * Related information and Prepayment. Destinations the page supplies are live;
 * the remaining reference commands stay disabled with an explanatory tooltip.
 */
export function buildCustomerInvoiceRibbonGroups(
  tr: (key: string, fallback: string) => string,
  actions: CustomerInvoiceActions = {}
): ActionPaneRibbonGroup[] {
  const unavailable = tr('customerInvoiceRibbon.unavailable', 'This action is not available yet.');
  const groups: ActionPaneRibbonGroup[] = [];

  if (actions.onNewFreeTextInvoice)
    groups.push({
      id: 'new',
      label: tr('customerInvoiceRibbon.new', 'New'),
      columns: 1,
      actions: [
        {
          id: 'free-text-invoice',
          label: tr('customerInvoiceRibbon.freeTextInvoice', 'Free text invoice'),
          onClick: actions.onNewFreeTextInvoice,
        },
      ],
    });

  groups.push({
    id: 'setup',
    label: tr('customerInvoiceRibbon.setupGroup', 'Set up'),
    columns: 1,
    actions: [
      {
        id: 'recurring-invoices',
        label: tr('customerInvoiceRibbon.recurringInvoices', 'Recurring invoices'),
        disabled: true,
        tooltip: unavailable,
      },
    ],
  });

  groups.push({
    id: 'journals',
    label: tr('customerInvoiceRibbon.journals', 'Journals'),
    columns: 1,
    actions: [
      {
        id: 'invoice-journal',
        label: tr('customerInvoiceRibbon.invoiceJournal', 'Invoice journal'),
        disabled: !actions.onInvoiceJournal,
        tooltip: actions.onInvoiceJournal ? undefined : unavailable,
        onClick: actions.onInvoiceJournal,
      },
      {
        id: 'bill-of-exchange',
        label: tr('customerInvoiceRibbon.billOfExchange', 'Bill of exchange'),
        disabled: true,
        tooltip: unavailable,
      },
    ],
  });

  groups.push({
    id: 'related-information',
    label: tr('customerInvoiceRibbon.relatedInformation', 'Related information'),
    columns: 1,
    actions: [
      ...(actions.onFreeTextInvoices
        ? [
            {
              id: 'free-text-invoices',
              label: tr('customerInvoiceRibbon.freeTextInvoices', 'Free text invoices'),
              onClick: actions.onFreeTextInvoices,
            },
          ]
        : []),
      {
        id: 'period-statistics',
        label: tr('customerInvoiceRibbon.periodStatistics', 'Period statistics'),
        disabled: true,
        tooltip: unavailable,
      },
      {
        id: 'cod-outstanding',
        label: tr('customerInvoiceRibbon.codOutstanding', 'COD outstanding'),
        disabled: true,
        tooltip: unavailable,
      },
      {
        id: 'bill-of-exchange-statistics',
        label: tr('customerInvoiceRibbon.billOfExchangeStatistics', 'Bill of exchange statistics'),
        disabled: true,
        tooltip: unavailable,
      },
    ],
  });

  groups.push({
    id: 'prepayment',
    label: tr('customerInvoiceRibbon.prepayment', 'Prepayment'),
    columns: 1,
    actions: [
      {
        id: 'apply-prepayment',
        label: tr('customerInvoiceRibbon.apply', 'Apply'),
        disabled: true,
        tooltip: unavailable,
      },
    ],
  });

  return groups;
}
