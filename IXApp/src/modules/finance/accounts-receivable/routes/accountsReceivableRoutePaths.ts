export const ACCOUNTS_RECEIVABLE_ROUTE_PATHS = {
  ROOT: '/accounts-receivable',
  CUSTOMERS: '/accounts-receivable/customers',
  CUSTOMER_DETAILS: '/accounts-receivable/customers/:customerId',
  customer: (customerId: string) =>
    `/accounts-receivable/customers/${encodeURIComponent(customerId)}`,
  CUSTOMER_GROUPS: '/accounts-receivable/customer-groups',
  CUSTOMER_POSTING_PROFILES: '/accounts-receivable/customer-posting-profiles',
  CUSTOMER_PARAMETERS: '/accounts-receivable/customer-parameters',
  CUSTOMER_PAYMENT_METHODS: '/accounts-receivable/customer-payment-methods',
  CUSTOMER_PAYMENT_TERMS: '/accounts-receivable/customer-payment-terms',
  SALES_ORDERS: '/accounts-receivable/sales-orders',
  SALES_ORDER_DETAILS: '/accounts-receivable/sales-orders/:salesOrderId',
  salesOrder: (salesOrderId: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}`,
} as const;
