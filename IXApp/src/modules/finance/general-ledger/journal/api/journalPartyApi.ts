/**
 * What a payment journal needs from the party module that owns its accounts (accounts
 * receivable for customers, accounts payable for vendors). A general journal has none.
 */
export interface JournalPartyAccount {
  /** The party record id (detail page link). */
  id: string;
  accountNumber: string;
  name: string;
  nameAlias?: string;
  currencyCode?: string;
  paymModeId?: string;
  paymTermId?: string;
}

/** A method of payment, with its source record (posting account and account type). */
export interface JournalPaymentMethod {
  code: string;
  description: string;
  source: Record<string, unknown>;
}

/** One open invoice a payment line can mark: `amount` is the positive open amount it settles. */
export interface JournalOpenInvoice {
  transRecId: number;
  invoice: string;
  transDate: string;
  amount: number;
  currencyCode: string;
}

export interface JournalPartyApi {
  list(signal?: AbortSignal): Promise<JournalPartyAccount[]>;
  paymentMethods(signal?: AbortSignal): Promise<JournalPaymentMethod[]>;
  openInvoices(accountNum: string, signal?: AbortSignal): Promise<JournalOpenInvoice[]>;
}
