import React from 'react';
import { VENDOR_PAYMENT_JOURNAL } from '../vendorPaymentJournal';
import { LedgerJournalListPage } from '../../../general-ledger/journal/pages/LedgerJournalListPage';

/** D365 LedgerJournalTable form for vendor payment journals (LedgerJournalType VendDisbursement). */
export function LedgerJournalListVendPaym() {
  return <LedgerJournalListPage kind={VENDOR_PAYMENT_JOURNAL} />;
}
