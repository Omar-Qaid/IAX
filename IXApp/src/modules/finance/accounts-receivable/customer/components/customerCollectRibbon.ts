import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';

/** Collect actions use implemented customer-scoped screens; other reference commands stay disabled. */
export function buildCustomerCollectRibbonGroups(
  tr: (key: string, fallback: string) => string,
  actions: { onSettleTransactions?: () => void } = {}
): ActionPaneRibbonGroup[] {
  return [
    {
      id: 'new',
      label: tr('customerCollectRibbon.groups.new', 'New'),
      columns: 1,
      actions: [
        {
          id: 'paymentJournal',
          label: tr('customerCollectRibbon.actions.paymentJournal', 'Payment journal'),
          disabled: true,
        },
        {
          id: 'collectionLetters',
          label: tr('customerCollectRibbon.actions.collectionLetters', 'Collection letters'),
          disabled: true,
        },
      ],
    },
    {
      id: 'collectionLetters',
      label: tr('customerCollectRibbon.groups.collectionLetters', 'Collection letters'),
      columns: 1,
      actions: [
        {
          id: 'reviewCollectionLetters',
          label: tr(
            'customerCollectRibbon.actions.reviewCollectionLetters',
            'Review and process collection letters'
          ),
          disabled: true,
        },
        {
          id: 'collectionLetterJournal',
          label: tr(
            'customerCollectRibbon.actions.collectionLetterJournal',
            'Collection letter journal'
          ),
          disabled: true,
        },
      ],
    },
    {
      id: 'interestNoteAdjustment',
      label: tr('customerCollectRibbon.groups.interestNoteAdjustment', 'Interest note adjustment'),
      columns: 2,
      actions: [
        {
          id: 'interestNote',
          label: tr('customerCollectRibbon.actions.interestNote', 'Interest note'),
          disabled: true,
          items: [],
        },
        {
          id: 'transactionInterest',
          label: tr('customerCollectRibbon.actions.transactionInterest', 'Transaction interest'),
          disabled: true,
          items: [],
        },
        {
          id: 'fee',
          label: tr('customerCollectRibbon.actions.fee', 'Fee'),
          disabled: true,
          items: [],
        },
        {
          id: 'history',
          label: tr('customerCollectRibbon.actions.history', 'History'),
          disabled: true,
        },
      ],
    },
    {
      id: 'interest',
      label: tr('customerCollectRibbon.groups.interest', 'Interest'),
      columns: 1,
      actions: [
        {
          id: 'calculateInterest',
          label: tr('customerCollectRibbon.actions.calculateInterest', 'Calculate interest'),
          disabled: true,
        },
        {
          id: 'printPostInterestNotes',
          label: tr(
            'customerCollectRibbon.actions.printPostInterestNotes',
            'Print/post interest notes'
          ),
          disabled: true,
        },
        {
          id: 'interestJournal',
          label: tr('customerCollectRibbon.actions.interestJournal', 'Interest journal'),
          disabled: true,
        },
      ],
    },
    {
      id: 'customerBalances',
      label: tr('customerCollectRibbon.groups.customerBalances', 'Customer balances'),
      columns: 2,
      actions: [
        {
          id: 'statements',
          label: tr('customerCollectRibbon.actions.statements', 'Statements'),
          disabled: true,
        },
        {
          id: 'writeOff',
          label: tr('customerCollectRibbon.actions.writeOff', 'Write off'),
          disabled: true,
        },
        {
          id: 'reimburseCustomer',
          label: tr('customerCollectRibbon.actions.reimburseCustomer', 'Reimburse customer'),
          disabled: true,
        },
        {
          id: 'collections',
          label: tr('customerCollectRibbon.actions.collections', 'Collections'),
          disabled: true,
        },
        {
          id: 'collectionsHistory',
          label: tr('customerCollectRibbon.actions.collectionsHistory', 'Collections history'),
          disabled: true,
          items: [],
        },
      ],
    },
    {
      id: 'settle',
      label: tr('customerCollectRibbon.groups.settle', 'Settle'),
      columns: 1,
      actions: [
        {
          id: 'settleTransactions',
          label: tr('customerCollectRibbon.actions.settleTransactions', 'Settle transactions'),
          disabled: !actions.onSettleTransactions,
          onClick: actions.onSettleTransactions,
        },
        {
          id: 'undoSettlement',
          label: tr('customerCollectRibbon.actions.undoSettlement', 'Undo settlement'),
          disabled: true,
        },
      ],
    },
    {
      id: 'deductions',
      label: tr('customerCollectRibbon.groups.deductions', 'Deductions'),
      columns: 1,
      actions: [
        {
          id: 'viewDeductions',
          label: tr('customerCollectRibbon.actions.viewDeductions', 'View deductions'),
          disabled: true,
        },
        {
          id: 'createDeductions',
          label: tr('customerCollectRibbon.actions.createDeductions', 'Create deductions'),
          disabled: true,
        },
      ],
    },
  ];
}
