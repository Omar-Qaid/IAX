-- Read-only checks. Run after Finance.PurchaseSettlements. Each violation count must be zero.
SELECT 'Vendor open balance mismatch' AS CheckName, COUNT_BIG(*) AS Violations
FROM VendTransOpen o JOIN VendTrans t ON t.RECID = o.RefRecId
WHERE o.DataAreaId <> t.DataAreaId OR o.AccountNum <> t.AccountNum
   OR o.AmountCur <> t.AmountCur - t.SettleAmountCur OR o.AmountMst <> t.AmountMst - t.SettleAmountMst;

SELECT 'Missing vendor open balance' AS CheckName, COUNT_BIG(*) AS Violations
FROM VendTrans t
WHERE t.IsDeleted = 0 AND t.Closed < '19000101'
  AND (t.AmountCur <> t.SettleAmountCur OR t.AmountMst <> t.SettleAmountMst)
  AND NOT EXISTS (SELECT 1 FROM VendTransOpen o WHERE o.RefRecId = t.RECID AND o.DataAreaId = t.DataAreaId);

SELECT 'Settlement ownership or sign' AS CheckName, COUNT_BIG(*) AS Violations
FROM VendSettlement s JOIN VendTrans i ON i.RECID = s.InvoiceTransRecId JOIN VendTrans p ON p.RECID = s.PaymentTransRecId
WHERE s.DataAreaId <> i.DataAreaId OR s.DataAreaId <> p.DataAreaId OR s.AccountNum <> i.AccountNum
   OR s.AccountNum <> p.AccountNum OR i.CurrencyCode <> p.CurrencyCode OR s.AmountCur <= 0 OR i.AmountCur >= 0 OR p.AmountCur <= 0;

SELECT 'Purchase example payment count or amount' AS CheckName,
       CASE WHEN COUNT_BIG(*) = 2 AND SUM(AmountCurDebit) = 5750 THEN 0 ELSE 1 END AS Violations
FROM LedgerJournalTrans WHERE DataAreaId = 'dat' AND PaymReference IN ('DEMO-VATP-1', 'DEMO-VATP-2');

SELECT 'Purchase example bank count or amount' AS CheckName,
       CASE WHEN COUNT_BIG(*) = 2 AND SUM(AmountCur) = -5750 THEN 0 ELSE 1 END AS Violations
FROM BankAccountTrans WHERE DataAreaId = 'dat' AND PaymReference IN ('DEMO-VATP-1', 'DEMO-VATP-2');

SELECT 'Purchase example payment unsettled' AS CheckName, COUNT_BIG(*) AS Violations
FROM LedgerJournalTrans l JOIN VendTrans t ON t.RECID = l.VendTransId
WHERE l.DataAreaId = 'dat' AND l.PaymReference IN ('DEMO-VATP-1', 'DEMO-VATP-2')
  AND (t.AmountCur <> t.SettleAmountCur OR t.AmountMst <> t.SettleAmountMst OR l.RemainAmount <> 0
       OR t.AmountCur <> (SELECT SUM(s.AmountCur) FROM VendSettlement s WHERE s.PaymentTransRecId = t.RECID));

SELECT 'Purchase payment voucher imbalance' AS CheckName, COUNT_BIG(*) AS Violations
FROM (
    SELECT e.RECID FROM GeneralJournalEntry e JOIN GeneralJournalAccountEntry a ON a.GeneralJournalEntry = e.RECID
    WHERE e.DataAreaId = 'dat' AND e.SubledgerVoucher IN
        (SELECT Voucher FROM LedgerJournalTrans WHERE DataAreaId = 'dat' AND PaymReference IN ('DEMO-VATP-1', 'DEMO-VATP-2'))
    GROUP BY e.RECID HAVING SUM(a.AccountingCurrencyAmount) <> 0
) unbalanced;

SELECT 'Receipt accrual reversal mismatch' AS CheckName, COUNT_BIG(*) AS Violations
FROM PurchReceiptAccrual a
WHERE a.ReversedInventQuantity > a.InventQuantity OR a.ReversedAmountMst > a.AmountMst
   OR a.ReversedInventQuantity <> COALESCE((SELECT SUM(m.InventQuantity) FROM VendInvoicePackingSlipQuantityMatch m WHERE m.ReceiptAccrualRecId = a.RECID), 0)
   OR a.ReversedAmountMst <> COALESCE((SELECT SUM(m.AccrualReversalAmountMst) FROM VendInvoicePackingSlipQuantityMatch m WHERE m.ReceiptAccrualRecId = a.RECID), 0);
