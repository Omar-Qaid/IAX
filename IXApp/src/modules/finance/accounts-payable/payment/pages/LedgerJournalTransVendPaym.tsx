import React from 'react';
import { VENDOR_PAYMENT_JOURNAL } from '../vendorPaymentJournal';
import { LedgerJournalLinesPage } from '../../../general-ledger/journal/pages/LedgerJournalLinesPage';

/** D365 LedgerJournalTransVendPaym form: vendor payment journal lines. */
export function LedgerJournalTransVendPaym() {
  return <LedgerJournalLinesPage kind={VENDOR_PAYMENT_JOURNAL} />;
}
