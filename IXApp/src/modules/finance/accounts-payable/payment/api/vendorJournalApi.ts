import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { requireApiData } from '@core/api/apiResponseData';
import type { JournalOpenInvoice } from '../../../general-ledger/journal/api/journalPartyApi';

/** A vendor as the journal lines need it (VendTable via /v1/Vendor). */
export interface JournalVendor {
  id: string;
  accountNumber: string;
  name: string;
  nameAlias?: string;
  currencyCode?: string;
  paymTermId?: string;
}

/** One open vendor transaction (VendTransOpen + VendTrans); an invoice is negative. */
export interface VendOpenTransRow {
  recId: number;
  transRecId: number;
  invoice: string;
  voucher: string;
  description: string;
  currencyCode: string;
  transDate: string;
  dueDate: string;
  amountCur: number;
  amountMst: number;
}

interface VendorDto {
  id?: number;
  recId?: number;
  accountNum: string;
  name: string;
  nameAR?: string | null;
  currency?: string | null;
  paymTermId?: string | null;
}

/** The vendor side of the vendor payment journal (accounts, methods, open invoices). */
export const vendorJournalApi = {
  async list(signal?: AbortSignal): Promise<JournalVendor[]> {
    const response = await apiClient.get<ApiResponse<VendorDto[]>>('/v1/Vendor', { signal });
    return requireApiData(response.data, 'vendors').map((row) => ({
      id: String(row.recId ?? row.id ?? row.accountNum),
      accountNumber: row.accountNum,
      name: row.name,
      nameAlias: row.nameAR ?? undefined,
      currencyCode: row.currency ?? undefined,
      paymTermId: row.paymTermId ?? undefined,
    }));
  },
  /** Vendor methods of payment (VendPaymModeTable), shaped like the customer ones. */
  async paymentMethods(signal?: AbortSignal) {
    const response = await apiClient.get<
      ApiResponse<
        {
          paymMode: string;
          name: string;
          accountType: number;
          paymentLedgerDimension?: number | null;
        }[]
      >
    >('/v1/VendPaymModeTable', { signal });
    return requireApiData(response.data, 'vendor payment methods').map((row) => ({
      code: row.paymMode,
      description: row.name,
      source: row as Record<string, unknown>,
    }));
  },
  /** Open transactions of one vendor, for marking the invoice a payment settles. */
  async openTransactions(accountNum: string, signal?: AbortSignal): Promise<VendOpenTransRow[]> {
    const response = await apiClient.get<ApiResponse<{ transactions: VendOpenTransRow[] }>>(
      '/v1/vendor-settlements/open-transactions',
      { params: { accountNum }, signal }
    );
    return requireApiData(response.data, 'vendor open transactions').transactions;
  },
  /** A vendor invoice is a negative open credit; the payment debit settles it. */
  async openInvoices(accountNum: string, signal?: AbortSignal): Promise<JournalOpenInvoice[]> {
    return (await vendorJournalApi.openTransactions(accountNum, signal))
      .filter((row) => row.amountCur < 0 && row.invoice)
      .map((row) => ({
        transRecId: row.transRecId,
        invoice: row.invoice,
        transDate: row.transDate,
        amount: -row.amountCur,
        currencyCode: row.currencyCode,
      }));
  },
};
