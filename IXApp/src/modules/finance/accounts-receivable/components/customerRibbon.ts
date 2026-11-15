import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';

interface CustomerRibbonContext {
  saved: boolean;
  hasParty: boolean;
  canReadBalances: boolean;
  onContacts: () => void;
  onViewContacts?: () => void;
  onNewContact?: () => void;
  onBalance: () => void;
  onTransactions?: () => void;
  onOpenTransactions?: () => void;
  onAttachments: () => void;
}

/** Only basic ERP actions with a confirmed, implemented destination belong in this tab. */
export function buildCustomerRibbonGroups(
  tr: (key: string, fallback: string) => string,
  context: CustomerRibbonContext
): ActionPaneRibbonGroup[] {
  const groups: ActionPaneRibbonGroup[] = [
    {
      id: 'accounts',
      label: tr('customerRibbon.groups.accounts', 'Accounts'),
      actions: [
        {
          id: 'contacts',
          label: context.onViewContacts
            ? tr('contactPerson.contacts', 'Contacts')
            : tr('customerDetails.sections.contacts', 'Contact information'),
          disabled: !context.saved || !context.hasParty,
          onClick: context.onContacts,
          items: context.onViewContacts
            ? [
                {
                  id: 'view-contacts',
                  label: tr('contactPerson.view', 'View contacts'),
                  onClick: context.onViewContacts,
                },
                ...(context.onNewContact
                  ? [
                      {
                        id: 'new-contact',
                        label: tr('contactPerson.new', 'New contact'),
                        onClick: context.onNewContact,
                      },
                    ]
                  : []),
                {
                  id: 'communication',
                  label: tr('customerDetails.sections.contacts', 'Contact information'),
                  onClick: context.onContacts,
                },
              ]
            : undefined,
        },
      ],
    },
  ];
  if (context.canReadBalances && context.onOpenTransactions)
    groups.push({
      id: 'transactions',
      label: tr('customerRibbon.groups.transactions', 'Transactions'),
      actions: [
        {
          id: 'transactions',
          label: tr('customerRibbon.groups.transactions', 'Transactions'),
          disabled: !context.saved,
          onClick: context.onTransactions,
        },
        {
          id: 'open-transactions',
          label: tr('partyBalance.openTransactions', 'Open transactions'),
          disabled: !context.saved,
          onClick: context.onOpenTransactions,
        },
      ],
    });
  if (context.canReadBalances)
    groups.push({
      id: 'balance',
      label: tr('customerRibbon.groups.balance', 'Balance'),
      actions: [
        {
          id: 'balance',
          label: tr('customerRibbon.actions.balance', 'Balance'),
          disabled: !context.saved,
          onClick: context.onBalance,
        },
      ],
    });
  groups.push({
    id: 'attachments',
    label: tr('customerRibbon.groups.attachments', 'Attachments'),
    actions: [
      {
        id: 'attachments',
        label: tr('customerRibbon.actions.attachments', 'Attachments'),
        disabled: !context.saved,
        onClick: context.onAttachments,
      },
    ],
  });
  return groups;
}
