export const TAX_ROUTE_PATHS = {
  ROOT: '/tax',
  TAX_ACCOUNT_GROUPS: '/tax/ledger-posting-groups',
  TAX_AUTHORITIES: '/tax/authorities',
  TAX_PERIODS: '/tax/settlement-periods',
  TAX_CODES: '/tax/codes',
  TAX_GROUPS: '/tax/groups',
  TAX_ITEM_GROUPS: '/tax/item-groups',
} as const;

// Saved links from before Tax became a separate module remain valid.
export const LEGACY_TAX_ROUTE_PATHS = {
  TAX_AUTHORITIES: '/foundation/tax-authorities',
  TAX_PERIODS: '/foundation/tax-periods',
  TAX_CODES: '/foundation/tax-codes',
  TAX_GROUPS: '/foundation/tax-groups',
  TAX_ITEM_GROUPS: '/foundation/tax-item-groups',
} as const;
