import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
interface CustomerSellActions {
  onNewSalesOrder?: () => void;
  onReturnOrder?: () => void;
  onOrderLines?: () => void;
  onSalesOrders?: () => void;
  onPackingSlips?: () => void;
  onTradeAgreementJournals?: () => void;
  onPriceDiscountGroups?: () => void;
  onJournalNames?: () => void;
}
/** Available destinations are supplied by the customer page; unavailable entries stay disabled. */
export function buildCustomerSellRibbonGroups(
  tr: (key: string, fallback: string) => string,
  actions: CustomerSellActions
): ActionPaneRibbonGroup[] {
  const groups: ActionPaneRibbonGroup[] = [];
  if (actions.onNewSalesOrder || actions.onReturnOrder) {
    const newActions = [];
    if (actions.onNewSalesOrder)
      newActions.push({
        id: 'new-sales-order',
        label: tr('customerSellRibbon.salesOrder', 'Sales order'),
        onClick: actions.onNewSalesOrder,
      });
    if (actions.onReturnOrder)
      newActions.push({
        id: 'new-return-order',
        label: tr('customerSellRibbon.returnOrder', 'Return order'),
        onClick: actions.onReturnOrder,
      });
    groups.push({
      id: 'new',
      label: tr('customerSellRibbon.new', 'New'),
      actions: newActions,
    });
  }
  if (actions.onSalesOrders || actions.onOrderLines || actions.onPackingSlips)
    groups.push({
      id: 'related-information',
      label: tr('customerSellRibbon.relatedInformation', 'Related information'),
      actions: [
        ...(actions.onSalesOrders || actions.onOrderLines
          ? [
              {
                id: 'orders',
                label: tr('customerSellRibbon.orders', 'Orders'),
                items: [
                  ...(actions.onSalesOrders
                    ? [
                        {
                          id: 'sales-orders',
                          label: tr('customerSellRibbon.allSalesOrders', 'All sales orders'),
                          onClick: actions.onSalesOrders,
                        },
                      ]
                    : []),
                  ...(actions.onOrderLines
                    ? [
                        {
                          id: 'order-lines',
                          label: tr('custSalesLines.orderLines', 'Order lines'),
                          onClick: actions.onOrderLines,
                        },
                      ]
                    : []),
                  {
                    id: 'confirmation-journal',
                    label: tr('customerSellRibbon.confirmationJournal', 'Confirmation journal'),
                    disabled: true,
                    tooltip: tr(
                      'customerSellRibbon.confirmationJournalUnavailable',
                      'Open a sales order to view its confirmation journal.'
                    ),
                    onClick: () => {},
                  },
                  {
                    id: 'backorder-lines',
                    label: tr('customerSellRibbon.backorderLines', 'Backorder lines'),
                    disabled: true,
                    tooltip: tr(
                      'customerSellRibbon.backorderLinesUnavailable',
                      'Backorder lines are not available yet.'
                    ),
                    onClick: () => {},
                  },
                ],
              },
            ]
          : []),
        ...(actions.onPackingSlips
          ? [
              {
                id: 'packing-slips',
                label: tr('customerSellRibbon.packingSlips', 'Packing slips'),
                onClick: actions.onPackingSlips,
              },
            ]
          : []),
      ],
    });
  if (actions.onTradeAgreementJournals)
    groups.push({
      id: 'trade-agreements',
      label: tr('customerSellRibbon.tradeAgreements', 'Trade agreements'),
      actions: [
        {
          id: 'trade-agreement-journals',
          label: tr('customerSellRibbon.journals', 'Trade agreement journals'),
          onClick: actions.onTradeAgreementJournals,
        },
      ],
    });
  const setupItems = [
    ...(actions.onPriceDiscountGroups
      ? [
          {
            id: 'price-discount-groups',
            label: tr('customerSellRibbon.priceDiscountGroups', 'Price/discount groups'),
            onClick: actions.onPriceDiscountGroups,
          },
        ]
      : []),
    ...(actions.onJournalNames
      ? [
          {
            id: 'journal-names',
            label: tr('customerSellRibbon.journalNames', 'Trade agreement journal names'),
            onClick: actions.onJournalNames,
          },
        ]
      : []),
  ];
  if (setupItems.length)
    groups.push({
      id: 'setup',
      label: tr('customerSellRibbon.setupGroup', 'Set up'),
      actions: [
        { id: 'setup-menu', label: tr('customerSellRibbon.setup', 'Setup'), items: setupItems },
      ],
    });
  return groups;
}
