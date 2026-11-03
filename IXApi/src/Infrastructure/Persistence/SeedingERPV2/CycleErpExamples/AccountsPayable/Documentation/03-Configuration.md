# Configuration requirements

## Required settings

| Owner | Setup | Required checks |
|---|---|---|
| GL | Ledger and chart | Company association, accounting currency, valid accounts |
| GL | Fiscal calendar | All example dates fall in an open supported period |
| GL | Journal names | Valid invoice/payment journal type and voucher sequence |
| AP | Parameters | Duplicate invoice behavior, posting and settlement rules |
| AP | Vendor groups/profiles | Valid payable control account and advance account |
| AP | Terms/methods | Due-date logic, transfer method and default company bank |
| AP | Matching | Explicit price/quantity tolerances and failure behavior |
| Tax | Code/groups | Effective rate, fully recoverable demo treatment and posting accounts |
| Inventory | Model group | Explicit physical/financial posting and receipt accrual flags |
| Inventory | Posting profiles | Item/group-specific receipt, accrual and inventory accounts |
| Bank | Company account | Active currency-compatible bank and main-account link |
| Currency | Exchange setup | Rate type, direction, factor and effective dates |

## Illustrative account roles

| Business key | Role | Expected normal balance |
|---|---|---|
| AP_CONTROL | Vendor liability | Credit |
| BANK | Company bank asset | Debit |
| INVENTORY | Financial inventory | Debit |
| RECEIPT_ASSET | Physical receipt asset/interim | Debit |
| RECEIPT_ACCRUAL | Received-not-invoiced liability | Credit |
| INPUT_TAX | Recoverable input tax | Debit |
| SERVICE_EXPENSE | Service expense | Debit |
| VENDOR_ADVANCE | Vendor advance asset | Debit |
| CASH_DISCOUNT | Purchase cash discount | Credit in the illustrative example |
| FX_GAIN / FX_LOSS | Realized exchange gain/loss | Credit / debit |

These are semantic keys, not mandatory account numbers. Resolve actual accounts from posting setup. Actual vouchers can contain additional interim and offset lines.

## Receipt posting variants

The main stocked example assumes physical receipt posting and receipt liability accrual are enabled. Also validate the supported disabled configuration separately: physical quantity can change without a receipt GL voucher. Never create vendor open invoice transactions merely from receipt accrual.

## Number sequences

Use configured document/voucher generators. Keep a separate scenario reference for rerun lookup. Check sequence scope and collisions. Do not reuse posted invoice or voucher numbers across incompatible document types.
