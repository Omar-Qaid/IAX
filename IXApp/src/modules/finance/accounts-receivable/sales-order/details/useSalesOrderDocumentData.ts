import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { customerQuickCreateApi } from '../../customer/api/customerQuickCreateApi';
import { salesOrderCopyApi, type SalesOrderCopyMode } from '../api/salesOrderCopyApi';
import { salesOrderLinesApi } from '../api/salesOrderLinesApi';
import { salesOrderListApi } from '../api/salesOrderListApi';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';

interface Params {
  /** Route ID of the open order; without it the first order of the list opens. */
  salesOrderId: string | undefined;
  /** ID of the order whose header is being edited (loads the editors' lookups). */
  editingOrderId: string | undefined;
  copyMode: SalesOrderCopyMode | undefined;
  copySourceId: string | undefined;
}

/**
 * All server data of the sales order document, company-scoped through
 * `salesOrderQueryKeys`: the order (by ID), the record pane list, lines, totals,
 * addresses and the lookups its editors need.
 */
export function useSalesOrderDocumentData({
  salesOrderId,
  editingOrderId,
  copyMode,
  copySourceId,
}: Params) {
  const company = useCompanyStore((state) => state.currentCompany);
  // The record pane shares the list page cache; the open order loads by its ID.
  const ordersQuery = useQuery({
    queryKey: salesOrderQueryKeys.list(company),
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const orders = ordersQuery.data ?? [];
  const detailQuery = useQuery({
    queryKey: salesOrderQueryKeys.detail(company, salesOrderId),
    queryFn: ({ signal }) => salesOrderListApi.get(salesOrderId!, signal),
    enabled: Boolean(salesOrderId),
    // Moving through the record pane: show the list row at once, refresh in place.
    placeholderData: orders.find((row) => row.id === salesOrderId),
  });
  const orderQuery = salesOrderId ? detailQuery : ordersQuery;
  // A 404 from the by-ID endpoint (older API build) falls back to the list row.
  const order = salesOrderId
    ? (detailQuery.data ?? orders.find((row) => row.id === salesOrderId))
    : orders[0];
  // Still loading while the order is unknown and either source is pending
  // (prevents a "no sales orders" flash before the list arrives).
  const orderLoading =
    !order &&
    (salesOrderId ? detailQuery.isPending || ordersQuery.isPending : ordersQuery.isPending);
  const headerEditing = Boolean(order) && editingOrderId === order?.id;
  /** Refreshes the open order and the record pane. */
  const refetchOrder = async (options?: { throwOnError?: boolean }) => {
    const [result] = await Promise.all([
      orderQuery.refetch(options),
      salesOrderId ? ordersQuery.refetch() : Promise.resolve(),
    ]);
    return result;
  };
  const customersQuery = useQuery({
    queryKey: salesOrderQueryKeys.lookup(company, 'header-customers'),
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: headerEditing,
    staleTime: 5 * 60 * 1000,
  });

  const lookupsQuery = useQuery({
    queryKey: salesOrderQueryKeys.lookup(company, 'header-lookups'),
    queryFn: ({ signal }) => customerQuickCreateApi.lookups(signal),
    enabled: headerEditing,
    staleTime: 5 * 60 * 1000,
  });

  const copySourcesQuery = useQuery({
    queryKey: salesOrderQueryKeys.copySources(company, order?.id, copyMode),
    queryFn: ({ signal }) => salesOrderCopyApi.documents(order!.id, copyMode!, signal),
    enabled: Boolean(order && copyMode),
  });
  const copyLinesQuery = useQuery({
    queryKey: salesOrderQueryKeys.copyLines(company, order?.id, copyMode, copySourceId),
    queryFn: ({ signal }) => salesOrderCopyApi.lines(order!.id, copyMode!, copySourceId!, signal),
    enabled: Boolean(order && copyMode && copySourceId !== undefined),
  });
  const linesQuery = useQuery({
    queryKey: salesOrderQueryKeys.lines(company, order?.id),
    queryFn: ({ signal }) => salesOrderLinesApi.list(order!.id, signal),
    enabled: Boolean(order),
  });
  const lines = linesQuery.data ?? [];
  const deliveryAddressesQuery = useQuery({
    queryKey: salesOrderQueryKeys.deliveryAddresses(company, order?.id),
    queryFn: ({ signal }) => salesOrderLinesApi.deliveryAddresses(order!.id, signal),
    enabled: Boolean(order),
    staleTime: 5 * 60 * 1000,
  });
  const totalsQuery = useQuery({
    queryKey: salesOrderQueryKeys.totals(company, order?.id),
    queryFn: ({ signal }) => salesOrderLinesApi.totals(order!.id, signal),
    enabled: Boolean(order),
  });
  const dimensionsQuery = useQuery({
    queryKey: salesOrderQueryKeys.lookup(company, 'inventory-dimensions'),
    queryFn: ({ signal }) => salesOrderLinesApi.inventoryDimensions(signal),
    // Also used in view mode: names for the master record cards.
    staleTime: 5 * 60 * 1000,
  });
  const taxGroupsQuery = useQuery({
    queryKey: salesOrderQueryKeys.lookup(company, 'tax-groups'),
    queryFn: ({ signal }) => salesOrderLinesApi.taxGroups(signal),
    // Also used in view mode: names for the master record cards.
    staleTime: 5 * 60 * 1000,
  });

  const queryClient = useQueryClient();
  /**
   * After a change to the order: refreshes everything cached under it (header,
   * lines, totals, addresses, journals) and the list rows.
   */
  const invalidateOrder = (orderId: string) =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.order(company, orderId) }),
      queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.list(company) }),
    ]);
  return {
    company,
    invalidateOrder,
    ordersQuery,
    orders,
    orderQuery,
    orderLoading,
    order,
    refetchOrder,
    customersQuery,
    lookupsQuery,
    copySourcesQuery,
    copyLinesQuery,
    linesQuery,
    lines,
    deliveryAddressesQuery,
    totalsQuery,
    dimensionsQuery,
    taxGroupsQuery,
  };
}
