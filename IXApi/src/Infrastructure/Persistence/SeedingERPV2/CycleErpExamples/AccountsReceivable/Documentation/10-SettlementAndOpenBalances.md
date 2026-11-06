# Settlement and open balances

`CustTrans` holds every invoice, collection and adjustment. `CustTransOpen` holds what is still
open. `CustSettlement` records what was allocated against what.

- A partial collection leaves the remainder open on the invoice.
- A full settlement closes the open amount and keeps the transaction history.
- An on-account collection stays open until it is settled against an invoice.
- A credit note is settled against an invoice the same way a collection is.

Every open row must equal its own transaction unsettled remainder, because the reversal services
rebuild `CustTransOpen` from that difference rather than recomputing it. A drifted open row would
silently reopen a wrong amount, which is why `CustomerBalanceValidator` checks each row rather than
only the total.

A zero customer balance does not mean every transaction is settled: an invoice and a collection of
equal size can both still be open if nothing settled them. The scenarios therefore assert the open
debit and the open credit separately, never their difference alone.

Settlement may also produce cash discount, exchange difference or rounding entries. Those move the
control account without changing the invoice amount in its own currency, so `CustomerToLedgerValidator`
adds realized exchange adjustments and utilized cash discounts back before comparing the subledger
with the control account.
