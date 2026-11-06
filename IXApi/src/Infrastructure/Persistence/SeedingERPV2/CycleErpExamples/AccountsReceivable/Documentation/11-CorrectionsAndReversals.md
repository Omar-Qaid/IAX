# Corrections and reversals

| Correction | Supported path | Effect |
|---|---|---|
| Undelivered remainder no longer wanted | Cancel the order remainder | Remaining quantity closed; delivered quantity preserved |
| Goods come back | Return order, receipt, then credit note | Stock back on hand, then the debt reduced |
| Reduce a debt with no goods back | Credit note without a receipt | Debt reduced, inventory untouched |
| Wrong allocation | Reverse the settlement | Invoice and collection reopened at their own amounts |
| Wrong collection | Reverse the payment | Bank and customer movement reversed; invoice reopened |

Posted history is never edited or deleted. A reversal posts its own voucher and leaves the original
in place, which is what lets the reconciliation still tie afterwards.

A reversal is resolved before it is attempted: the flow looks for an existing reversing journal or an
already-reversed settlement and skips the call instead of reversing twice.

Reversing a settlement reopens both sides, so the scenario expects the open debit and the open credit
to come back to the invoice total each. Reversing a payment reopens the invoice only.
