import { GENERAL_LEDGER_ROUTE_PATHS } from '../routes/generalLedgerRoutePaths';
import type { LedgerJournalKind } from './config/journalKinds';

/** D365 general journal (LedgerJournalType Daily): ledger accounts, no party, no settlement. */
export const GENERAL_JOURNAL: LedgerJournalKind = {
  journalType: 0,
  title: 'General journal',
  linesTitle: 'Journal voucher',
  journalTypeLabel: 'Daily',
  enterLabel: 'Enter journal lines',
  accountType: 0,
  accountLabel: 'Account',
  amountSide: 'either',
  settlement: null,
  listPath: GENERAL_LEDGER_ROUTE_PATHS.LEDGER_JOURNAL_LIST_DAILY,
  linesPath: GENERAL_LEDGER_ROUTE_PATHS.ledgerJournalTransDaily,
  customize: {
    // A general journal has no customer/vendor payment tabs.
    lineTabs: (tabs) => tabs.filter((tab) => tab !== 'Payment' && tab !== 'Remittance'),
  },
};
