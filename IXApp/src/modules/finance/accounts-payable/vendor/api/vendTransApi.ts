import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { requireApiData } from '@core/api/apiResponseData';

/** One vendor transaction (VendTrans): an invoice is a negative credit, a payment a debit. */
export interface VendTransRow {
  id: string;
  recId: number;
  accountNum: string;
  transDate: string;
  voucher: string;
  invoice: string;
  txt: string;
  currencyCode: string;
  amountCur: number;
  amountMst: number;
  settleAmountCur: number;
  settleAmountMst: number;
  dueDate: string;
  closed: string;
  transType: number;
}

interface VendTransPage {
  page: number;
  pageSize: number;
  totalCount: number;
  rows: Omit<VendTransRow, 'id'>[];
}

const PAGE_SIZE = 200; // the API's maximum page size

export const vendTransApi = {
  /** All transactions of one vendor, read page by page. */
  async list(accountNum: string, signal?: AbortSignal): Promise<VendTransRow[]> {
    const rows: VendTransRow[] = [];
    for (let page = 1; ; page += 1) {
      const response = await apiClient.get<ApiResponse<VendTransPage>>('/v1/VendTrans/paged', {
        params: { accountNum, page, pageSize: PAGE_SIZE },
        signal,
      });
      const data = requireApiData(response.data, 'vendor transactions');
      rows.push(...data.rows.map((row) => ({ ...row, id: String(row.recId) })));
      if (data.rows.length === 0 || rows.length >= data.totalCount) return rows;
    }
  },
};
