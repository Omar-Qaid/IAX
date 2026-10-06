const masterRoutes: Record<string, string> = {
  customerAccount: '/accounts-receivable/customers',
  invoiceAccount: '/accounts-receivable/customers',
  customerGroup: '/accounts-receivable/customer-groups',
  customerGroupId: '/accounts-receivable/customer-groups',
  custGroupId: '/accounts-receivable/customer-groups',
  currencyCode: '/foundation/currencies',
  paymentTerms: '/accounts-receivable/customer-payment-terms',
  paymTermId: '/accounts-receivable/customer-payment-terms',
  paymentMethod: '/accounts-receivable/customer-payment-methods',
  paymModeId: '/accounts-receivable/customer-payment-methods',
  taxGroupId: '/foundation/tax-groups',
  salesTaxGroup: '/foundation/tax-groups',
  taxItemGroup: '/foundation/tax-item-groups',
  inventSiteId: '/inventory/sites',
  inventLocationId: '/inventory/warehouses',
};

export const lookupMasterRoute = (fieldName: string, explicitRoute?: string): string | undefined =>
  explicitRoute || masterRoutes[fieldName];
