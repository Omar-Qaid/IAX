export const SALES_ORDER_V2_ROUTE_PATHS = {
  DETAILS: '/accounts-receivable/sales-orders/:salesOrderId/master-details',
  detail: (id: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(id)}/master-details`,
} as const;
