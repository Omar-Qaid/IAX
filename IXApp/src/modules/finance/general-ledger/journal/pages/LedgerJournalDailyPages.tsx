import React from 'react';
import { GENERAL_JOURNAL } from '../generalJournal';
import { LedgerJournalLinesPage } from './LedgerJournalLinesPage';
import { LedgerJournalListPage } from './LedgerJournalListPage';

/** D365 general journal list (LedgerJournalType Daily). */
export function LedgerJournalListDaily() {
  return <LedgerJournalListPage kind={GENERAL_JOURNAL} />;
}

/** D365 LedgerJournalTransDaily form: journal voucher lines. */
export function LedgerJournalTransDaily() {
  return <LedgerJournalLinesPage kind={GENERAL_JOURNAL} />;
}
