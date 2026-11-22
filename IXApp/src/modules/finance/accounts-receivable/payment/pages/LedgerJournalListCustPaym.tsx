import React from 'react';
import { CUSTOMER_PAYMENT_JOURNAL } from '../customerPaymentJournal';
import { LedgerJournalListPage } from '../../../general-ledger/journal/pages/LedgerJournalListPage';

/** D365 LedgerJournalTable form for customer payment journals. */
export function LedgerJournalListCustPaym() {
  return <LedgerJournalListPage kind={CUSTOMER_PAYMENT_JOURNAL} />;
}
