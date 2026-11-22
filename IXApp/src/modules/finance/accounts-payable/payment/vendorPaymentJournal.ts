import type { LedgerJournalKind } from '../../general-ledger/journal/config/journalKinds';
import { GENERAL_LEDGER_ROUTE_PATHS } from '../../general-ledger/routes/generalLedgerRoutePaths';
import { vendorJournalApi } from './api/vendorJournalApi';

/** D365 vendor payment journal (LedgerJournalType VendDisbursement): vendor-specific behaviour. */
export const VENDOR_PAYMENT_JOURNAL: LedgerJournalKind = {
  journalType: 2,
  title: 'Vendor payment journal',
  linesTitle: 'Vendor payments',
  journalTypeLabel: 'Vendor disbursement',
  enterLabel: 'Enter vendor payments',
  accountType: 2,
  accountLabel: 'Account',
  amountSide: 'debit',
  settlement: 'vend',
  partyApi: vendorJournalApi,
  listPath: GENERAL_LEDGER_ROUTE_PATHS.LEDGER_JOURNAL_LIST_VEND_PAYM,
  linesPath: GENERAL_LEDGER_ROUTE_PATHS.ledgerJournalTransVendPaym,
  accountTransactions: {
    label: 'Vendor transactions',
    path: GENERAL_LEDGER_ROUTE_PATHS.vendTrans,
  },
  // Vendor-only changes to the shared journal pages go here.
  customize: {},
};
