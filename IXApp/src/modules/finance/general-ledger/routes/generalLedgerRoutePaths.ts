/** General ledger routes. */
export const GENERAL_LEDGER_ROUTE_PATHS = {
  MAIN_ACCOUNTS: '/general-ledger/main-accounts',
  VOUCHER_TRANSACTIONS: '/general-ledger/LedgerTransVoucher',
  /** D365 general journal (LedgerJournalType Daily). */
  LEDGER_JOURNAL_LIST_DAILY: '/general-ledger/LedgerJournalListDaily',
  LEDGER_JOURNAL_TRANS_DAILY: '/general-ledger/LedgerJournalTransDaily/:journalId',
  ledgerJournalTransDaily: (journalId: string | number) =>
    `/general-ledger/LedgerJournalTransDaily/${encodeURIComponent(journalId)}`,
  /** D365 VendTrans: one vendor's transactions. */
  VEND_TRANS: '/accounts-payable/VendTrans/:accountNum',
  vendTrans: (accountNum: string) =>
    `/accounts-payable/VendTrans/${encodeURIComponent(accountNum)}`,
  /** D365 vendor payment journal (LedgerJournalType Payment). */
  LEDGER_JOURNAL_LIST_VEND_PAYM: '/accounts-payable/LedgerJournalListVendPaym',
  LEDGER_JOURNAL_TRANS_VEND_PAYM: '/accounts-payable/LedgerJournalTransVendPaym/:journalId',
  ledgerJournalTransVendPaym: (journalId: string | number) =>
    `/accounts-payable/LedgerJournalTransVendPaym/${encodeURIComponent(journalId)}`,
} as const;
