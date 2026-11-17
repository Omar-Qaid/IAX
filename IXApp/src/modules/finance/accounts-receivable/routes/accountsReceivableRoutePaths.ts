export const ACCOUNTS_RECEIVABLE_ROUTE_PATHS = {
  ROOT: '/accounts-receivable',
  CUSTOMERS: '/accounts-receivable/customers',
  CUSTOMER_DETAILS: '/accounts-receivable/customers/:customerId',
  customer: (customerId: string) =>
    `/accounts-receivable/customers/${encodeURIComponent(customerId)}`,
  CUST_TRANS: '/accounts-receivable/CustTrans/:accountNum',
  custTrans: (accountNum: string) =>
    `/accounts-receivable/CustTrans/${encodeURIComponent(accountNum)}`,
  CUSTOMER_OPEN_TRANSACTIONS: '/accounts-receivable/customer-open-transactions/:accountNum',
  customerOpenTransactions: (accountNum: string) =>
    `/accounts-receivable/customer-open-transactions/${encodeURIComponent(accountNum)}`,
  customerSalesOrders: (accountNum: string) =>
    `/accounts-receivable/sales-orders?customerAccount=${encodeURIComponent(accountNum)}`,
  CUSTOMER_PACKING_SLIPS: '/accounts-receivable/customer-packing-slips/:accountNum',
  customerPackingSlips: (accountNum: string) =>
    `/accounts-receivable/customer-packing-slips/${encodeURIComponent(accountNum)}`,
  FREE_TEXT_INVOICES: '/accounts-receivable/free-text-invoices',
  FREE_TEXT_INVOICE_DETAILS: '/accounts-receivable/free-text-invoices/:invoiceRecId',
  FREE_TEXT_INVOICE_NEW: '/accounts-receivable/free-text-invoices/new',
  freeTextInvoice: (recId: string | number) =>
    `/accounts-receivable/free-text-invoices/${encodeURIComponent(recId)}`,
  newFreeTextInvoice: (account?: string) =>
    `/accounts-receivable/free-text-invoices/new${account ? `?customerAccount=${encodeURIComponent(account)}` : ''}`,
  customerFreeTextInvoices: (accountNum: string) =>
    `/accounts-receivable/free-text-invoices?customerAccount=${encodeURIComponent(accountNum)}`,
  CUSTOMER_GROUPS: '/accounts-receivable/customer-groups',
  CUSTOMER_POSTING_PROFILES: '/accounts-receivable/customer-posting-profiles',
  CUSTOMER_PARAMETERS: '/accounts-receivable/customer-parameters',
  CUSTOMER_PAYMENT_METHODS: '/accounts-receivable/customer-payment-methods',
  CUSTOMER_PAYMENT_TERMS: '/accounts-receivable/customer-payment-terms',
  TRADE_AGREEMENT_JOURNALS: '/accounts-receivable/trade-agreement-journals',
  PRICE_DISCOUNT_GROUPS: '/accounts-receivable/price-discount-groups',
  TRADE_AGREEMENT_JOURNAL_NAMES: '/accounts-receivable/trade-agreement-journal-names',
  SALES_ORDERS: '/accounts-receivable/sales-orders',
  SALES_ORDER_DETAILS: '/accounts-receivable/sales-orders/:salesOrderId',
  SALES_ORDER_CHARGES: '/accounts-receivable/sales-orders/:salesOrderId/charges',
  CUST_CONFIRM_JOURNAL: '/accounts-receivable/sales-orders/:salesOrderId/confirmations',
  CUST_PACKING_SLIP_JOURNAL: '/accounts-receivable/sales-orders/:salesOrderId/packing-slips',
  salesOrder: (salesOrderId: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}`,
  salesOrderCharges: (salesOrderId: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}/charges`,
  custConfirmJournal: (salesOrderId: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}/confirmations`,
  custPackingSlipJournal: (salesOrderId: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}/packing-slips`,
  salesOrderLineCharges: (salesOrderId: string, lineId: string) =>
    `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}/charges?lineId=${encodeURIComponent(lineId)}`,
} as const;
