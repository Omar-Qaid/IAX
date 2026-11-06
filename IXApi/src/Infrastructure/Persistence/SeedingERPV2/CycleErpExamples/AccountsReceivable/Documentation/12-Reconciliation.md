# Reconciliation

The eleven checks in `AccountsReceivableReconciliationSnapshot.Checks` run after every scenario, over
one snapshot of the persisted rows.

| Concern | How it is tied |
|---|---|
| Open balances | Expected open debit and credit for the scenario |
| Customer subledger | `CustTrans` plus realized exchange, minus utilized cash discount, equals account 181006 |
| Voucher balance | Every voucher sums to zero in the accounting and the reporting currency |
| Inventory quantity | Issued quantity net of returns equals what the scenario delivered |
| Inventory value | Posted plus adjusted cost equals account 181002 |
| Delivery interim | Accounts 181007 and 281005 are back to zero once invoiced |
| Bank | `BankAccountTrans` in the accounting currency equals account 181001 |
| Output tax | `TaxTrans` equals account 281004 |
| Prepayment | Account 281003 is zero once applied |
| Revenue | `CustInvoiceTrans` line amounts equal account 481003 |

An unseeded main account is treated as an error rather than a zero, because a zero would let a
renumbered chart satisfy a check that expects a cleared account.

Bank and tax are compared in the accounting currency on purpose: a foreign-currency collection would
never sum against a SAR ledger account in its own currency.

A rejected operation is verified against a fingerprint of every posting table taken before the call,
so a refusal is proven to have written nothing.
