import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
interface CustomerSellActions {
  onNewSalesOrder?: () => void;
  onSalesOrders?: () => void;
  onPackingSlips?: () => void;
  onTradeAgreementJournals?: () => void;
  onPriceDiscountGroups?: () => void;
  onJournalNames?: () => void;
}
/** Only actions with an implemented basic ERP destination are supplied by the customer page. */
export function buildCustomerSellRibbonGroups(
  tr: (key: string, fallback: string) => string,
  actions: CustomerSellActions
): ActionPaneRibbonGroup[] {
  const groups: ActionPaneRibbonGroup[] = [];
  if (actions.onNewSalesOrder)
    groups.push({
      id: 'new',
      label: tr('customerSellRibbon.new', 'New'),
      actions: [
        {
          id: 'new-sales-order',
          label: tr('customerSellRibbon.salesOrder', 'Sales order'),
          onClick: actions.onNewSalesOrder,
        },
      ],
    });
  if (actions.onSalesOrders || actions.onPackingSlips)
    groups.push({
      id: 'related-information',
      label: tr('customerSellRibbon.relatedInformation', 'Related information'),
      actions: [
        ...(actions.onSalesOrders
          ? [
              {
                id: 'orders',
                label: tr('customerSellRibbon.orders', 'Orders'),
                items: [
                  {
                    id: 'sales-orders',
                    label: tr('pages.salesOrders.title', 'Sales orders'),
                    onClick: actions.onSalesOrders,
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
