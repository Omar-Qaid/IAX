export const customerQueryKeys = {
  all: ['customers'] as const,
  lists: () => [...customerQueryKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...customerQueryKeys.lists(), params] as const,
  details: () => [...customerQueryKeys.all, 'detail'] as const,
  detail: (id: string) => [...customerQueryKeys.details(), id] as const,
};

export const customerGroupQueryKeys = {
  all: ['customerGroups'] as const,
  lists: () => [...customerGroupQueryKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) => [...customerGroupQueryKeys.lists(), params] as const,
  detail: (id: string) => [...customerGroupQueryKeys.all, 'detail', id] as const,
};

/**
 * Sales order cache keys. Everything is scoped by company, and all data of one
 * order (header, lines, totals, addresses, journals) sits under `order(company, id)`,
 * so `invalidateQueries({ queryKey: salesOrderQueryKeys.order(company, id) })`
 * refreshes the whole order at once. Shared lookups sit under `lookups(company)`.
 */
export const salesOrderQueryKeys = {
  all: ['salesOrders'] as const,
  company: (company: string) => [...salesOrderQueryKeys.all, company] as const,
  list: (company: string) => [...salesOrderQueryKeys.company(company), 'list'] as const,
  order: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.company(company), 'order', id ?? ''] as const,
  detail: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'header'] as const,
  lines: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'lines'] as const,
  totals: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'totals'] as const,
  deliveryAddresses: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'delivery-addresses'] as const,
  confirmations: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'confirmations'] as const,
  confirmation: (company: string, id: string | undefined, journalId: unknown) =>
    [...salesOrderQueryKeys.confirmations(company, id), journalId] as const,
  confirmationPreview: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'confirmation-preview'] as const,
  packingSlips: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'packing-slips'] as const,
  packingSlip: (company: string, id: string | undefined, journalId: unknown) =>
    [...salesOrderQueryKeys.packingSlips(company, id), journalId] as const,
  packingSlipPreview: (company: string, id: string | undefined, ...params: unknown[]) =>
    [...salesOrderQueryKeys.order(company, id), 'packing-slip-preview', ...params] as const,
  invoices: (company: string, id: string | undefined) =>
    [...salesOrderQueryKeys.order(company, id), 'invoices'] as const,
  copySources: (company: string, id: string | undefined, ...params: unknown[]) =>
    [...salesOrderQueryKeys.order(company, id), 'copy-sources', ...params] as const,
  copyLines: (company: string, id: string | undefined, ...params: unknown[]) =>
    [...salesOrderQueryKeys.order(company, id), 'copy-lines', ...params] as const,
  lookups: (company: string) => [...salesOrderQueryKeys.company(company), 'lookups'] as const,
  lookup: (company: string, name: string, ...params: unknown[]) =>
    [...salesOrderQueryKeys.lookups(company), name, ...params] as const,
};

/**
 * Free text invoice keys, company-scoped like sales order: the list, one
 * invoice, and the lookups its editors need (shared under `lookups(company)`).
 */
export const freeTextInvoiceQueryKeys = {
  all: ['freeTextInvoices'] as const,
  company: (company: string) => [...freeTextInvoiceQueryKeys.all, company] as const,
  list: (company: string) => [...freeTextInvoiceQueryKeys.company(company), 'list'] as const,
  invoice: (company: string, id: string | undefined) =>
    [...freeTextInvoiceQueryKeys.company(company), 'invoice', id ?? ''] as const,
  customers: (company: string) =>
    [...freeTextInvoiceQueryKeys.company(company), 'customers'] as const,
  lookups: (company: string) => [...freeTextInvoiceQueryKeys.company(company), 'lookups'] as const,
  lookup: (company: string, name: string) =>
    [...freeTextInvoiceQueryKeys.lookups(company), name] as const,
};

/**
 * Customer payment journal keys: the company's bank accounts and a customer's open
 * transactions, which a posted payment settles against.
 */
export const customerPaymentQueryKeys = {
  all: ['customerPayments'] as const,
  company: (company: string) => [...customerPaymentQueryKeys.all, company] as const,
  bankAccounts: (company: string) =>
    [...customerPaymentQueryKeys.company(company), 'bank-accounts'] as const,
  openTransactions: (company: string, accountNum: string | undefined) =>
    [...customerPaymentQueryKeys.company(company), 'open-transactions', accountNum ?? ''] as const,
};

/**
 * Return order keys: the customer's invoiced lines a return is raised from, the
 * disposition and reason lookups, and a created return order's own lines.
 */
export const returnOrderQueryKeys = {
  all: ['returnOrders'] as const,
  company: (company: string) => [...returnOrderQueryKeys.all, company] as const,
  invoicedLines: (company: string, accountNum: string | undefined) =>
    [...returnOrderQueryKeys.company(company), 'invoiced-lines', accountNum ?? ''] as const,
  dispositionCodes: (company: string) =>
    [...returnOrderQueryKeys.company(company), 'disposition-codes'] as const,
  reasonCodes: (company: string) =>
    [...returnOrderQueryKeys.company(company), 'reason-codes'] as const,
  returnLines: (company: string, recId: number | undefined) =>
    [...returnOrderQueryKeys.company(company), 'return-lines', recId ?? 0] as const,
};
