export const PARTY_BALANCE_ROUTE_PATHS = {
  CUSTOMER_BALANCE: '/accounts-receivable/customer-balances/:accountNum',
  VENDOR_BALANCE: '/accounts-payable/vendor-balances/:accountNum',
  customerBalance: (accountNum: string) =>
    `/accounts-receivable/customer-balances/${encodeURIComponent(accountNum)}`,
  vendorBalance: (accountNum: string) =>
    `/accounts-payable/vendor-balances/${encodeURIComponent(accountNum)}`,
} as const;
