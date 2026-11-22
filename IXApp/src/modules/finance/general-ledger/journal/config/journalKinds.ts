import type { EnterpriseListCommand } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type {
  LedgerJournalTypeId,
  PaymentJournal,
  PaymentJournalLine,
} from '../api/ledgerJournalApi';
import type { JournalPartyApi } from '../api/journalPartyApi';
import type { JournalFieldGroup } from '../components/JournalShared';

/** D365 LedgerJournalACType of the journal's account column. */
export type JournalAccountType = 0 | 1 | 2;

/** What a journal's line customizations can read. */
export interface JournalLineContext {
  header: PaymentJournal | undefined;
  /** The selected line (the edited one while editing). */
  line: PaymentJournalLine | null;
  editing: boolean;
  navigate: (path: string) => void;
}

/**
 * Per-journal customization of the shared journal pages. Every hook is optional: a journal
 * changes only what differs from the standard D365 journal form.
 */
export interface LedgerJournalCustomization {
  /** Change the lines grid columns (add, remove, reorder, relabel). */
  lineColumns?: (columns: ColumnDef<PaymentJournalLine>[]) => ColumnDef<PaymentJournalLine>[];
  /** Change the line tabs (after List). */
  lineTabs?: (tabs: string[]) => string[];
  /** Fields of a line tab: return groups to replace the standard ones, or undefined to keep them. */
  lineTabGroups?: (
    tab: string,
    line: PaymentJournalLine,
    standard: JournalFieldGroup[]
  ) => JournalFieldGroup[] | undefined;
  /** Extra commands on the lines action pane. */
  lineCommands?: (context: JournalLineContext) => EnterpriseListCommand[];
  /** Extra check before a line is saved: return an error message to refuse it. */
  validateLine?: (line: PaymentJournalLine, header: PaymentJournal) => string | undefined;
  /** Defaults of a new line (after the standard ones). */
  newLine?: (line: PaymentJournalLine, header: PaymentJournal) => PaymentJournalLine;
  /** Change the journal list columns. */
  listColumns?: (columns: ColumnDef<PaymentJournal>[]) => ColumnDef<PaymentJournal>[];
  /** Extra commands on the journal list action pane. */
  listCommands?: (journal: PaymentJournal | null) => EnterpriseListCommand[];
}

/**
 * One D365 journal type shown through the shared journal pages. Customer payment, vendor payment
 * and general journals use the same tables (LedgerJournalTable / LedgerJournalTrans); each module
 * defines its own kind (what differs) next to its pages.
 */
export interface LedgerJournalKind {
  journalType: LedgerJournalTypeId;
  /** List page title (D365 form caption). */
  title: string;
  /** Lines page title. */
  linesTitle: string;
  /** "Journal type" value on the General tab. */
  journalTypeLabel: string;
  /** First D365 action on the list page (not available here, shown disabled). */
  enterLabel: string;
  /** Account type of new lines: 0 ledger, 1 customer, 2 vendor. */
  accountType: JournalAccountType;
  /** Account column label. */
  accountLabel: string;
  /** Payment direction: customers pay a credit, vendors are paid a debit; a general journal either. */
  amountSide: 'credit' | 'debit' | 'either';
  /** Marked-invoice settlement on posting (customer or vendor), or none. */
  settlement: 'cust' | 'vend' | null;
  /** Accounts, methods of payment and open invoices of the line party; none for a ledger journal. */
  partyApi?: JournalPartyApi;
  listPath: string;
  linesPath: (journalId: string | number) => string;
  /** Subledger transactions page of the line account, when the app has one. */
  accountTransactions?: { label: string; path: (account: string) => string };
  /** Journal-specific changes to the shared pages. */
  customize?: LedgerJournalCustomization;
}
