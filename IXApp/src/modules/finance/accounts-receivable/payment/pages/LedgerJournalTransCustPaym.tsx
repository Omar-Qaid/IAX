import React from 'react';
import { CUSTOMER_PAYMENT_JOURNAL } from '../customerPaymentJournal';
import { LedgerJournalLinesPage } from '../../../general-ledger/journal/pages/LedgerJournalLinesPage';

/** D365 LedgerJournalTransCustPaym form: customer payment journal lines. */
export function LedgerJournalTransCustPaym() {
  return <LedgerJournalLinesPage kind={CUSTOMER_PAYMENT_JOURNAL} />;
}
