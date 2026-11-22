import type { JournalPartyApi } from '../../../general-ledger/journal/api/journalPartyApi';
import { customerQuickCreateApi } from '../../customer/api/customerQuickCreateApi';
import { custOpenTransApi } from '../../settlement/api/custOpenTransApi';
import { customerPaymentSetupApi } from '../../setup/api/customerPaymentSetupApi';

const paymentMethodsApi = customerPaymentSetupApi('methods');

/** The customer side of the customer payment journal (accounts, methods, open invoices). */
export const customerJournalApi: JournalPartyApi = {
  async list(signal) {
    return (await customerQuickCreateApi.list(signal)).map((row) => ({
      id: String(row.id),
      accountNumber: row.accountNumber,
      name: row.name,
      nameAlias: row.nameAr,
      currencyCode: row.currencyCode,
      paymModeId: row.paymModeId,
      paymTermId: row.paymTermId,
    }));
  },
  async paymentMethods(signal) {
    return (await paymentMethodsApi.list(signal)).map(({ code, description, source }) => ({
      code,
      description,
      source,
    }));
  },
  /** A customer invoice is a positive open debit; the payment credit settles it. */
  async openInvoices(accountNum, signal) {
    return (await custOpenTransApi.load(accountNum, signal)).transactions
      .filter((row) => row.amountCur > 0 && row.invoice)
      .map((row) => ({
        transRecId: row.transRecId,
        invoice: row.invoice,
        transDate: row.transDate,
        amount: row.amountCur,
        currencyCode: row.currencyCode,
      }));
  },
};
