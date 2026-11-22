import type { LedgerJournalKind } from '../../general-ledger/journal/config/journalKinds';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';
import { customerJournalApi } from './api/customerJournalApi';

/** D365 customer payment journal (LedgerJournalType CustPayment): customer-specific behaviour. */
export const CUSTOMER_PAYMENT_JOURNAL: LedgerJournalKind = {
  journalType: 1,
  title: 'Customer payment journal',
  linesTitle: 'Customer payments',
  journalTypeLabel: 'Customer payment',
  enterLabel: 'Enter customer payments',
  accountType: 1,
  accountLabel: 'Account',
  amountSide: 'credit',
  settlement: 'cust',
  partyApi: customerJournalApi,
  listPath: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.LEDGER_JOURNAL_LIST_CUST_PAYM,
  linesPath: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.ledgerJournalTransCustPaym,
  accountTransactions: {
    label: 'Customer transactions',
    path: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.custTrans,
  },
  // Customer-only changes to the shared journal pages go here.
  customize: {},
};
