import type { FetchPageFn, LookupOption } from '@shared/components/lookups/types';
import { salesOrderLinesApi } from './salesOrderLinesApi';

/**
 * Server-paged lookup sources for sales order fields (grid cells and line details).
 * Kept apart from the API object so tests that mock the API also drive these.
 */
/** Unit lookup page for `LookupField`/`DataGridLookupField` (server search, paged). */
export const fetchSalesOrderUnitOptions: FetchPageFn<LookupOption> = async (params) => {
  const page = await salesOrderLinesApi.units(params);
  return {
    ...page,
    data: page.data.map((unit) => ({ id: unit.symbol, code: unit.symbol, name: unit.symbol })),
  };
};
