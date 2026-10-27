-- Read-only relational/accounting checks after opt-in business scenario seeding.
SET NOCOUNT ON;
SELECT 'UnbalancedLedgerVoucher' AS CheckName, COUNT(*) AS Violations
FROM (SELECT GeneralJournalEntry FROM GeneralJournalAccountEntry GROUP BY GeneralJournalEntry
      HAVING ABS(SUM(AccountingCurrencyAmount)) > 0.01 OR ABS(SUM(TransactionCurrencyAmount)) > 0.01) x
UNION ALL SELECT 'JournalAccountWithoutEntry', COUNT(*) FROM GeneralJournalAccountEntry a
LEFT JOIN GeneralJournalEntry e ON e.RECID=a.GeneralJournalEntry WHERE e.RECID IS NULL
UNION ALL SELECT 'VoucherLinkCompanyMismatch', COUNT(*) FROM SubledgerVoucherGeneralJournalEntry v
JOIN GeneralJournalEntry e ON e.RECID=v.GeneralJournalEntry WHERE v.DataAreaId<>e.DataAreaId OR v.VoucherDataAreaId<>e.DataAreaId
UNION ALL SELECT 'CustomerInvoiceTotalsMismatch', COUNT(*) FROM CustInvoiceJour j
CROSS APPLY (SELECT SUM(t.LineAmount) AS Net, SUM(t.TaxAmount) AS Tax FROM CustInvoiceTrans t
             WHERE t.DataAreaId=j.DataAreaId AND t.InvoiceId=j.InvoiceId) x
WHERE j.ParmId LIKE 'DEMO-%' AND (ABS(j.SalesBalance-COALESCE(x.Net,0))>0.01 OR ABS(j.SumTax-COALESCE(x.Tax,0))>0.01
      OR ABS(j.InvoiceAmount-j.SalesBalance-j.SumTax)>0.01)
UNION ALL SELECT 'VendorInvoiceTotalsMismatch', COUNT(*) FROM VendInvoiceJour j
CROSS APPLY (SELECT SUM(t.LineAmount) AS Net FROM VendInvoiceTrans t
             WHERE t.DataAreaId=j.DataAreaId AND t.InternalInvoiceId=j.InternalInvoiceId) x
WHERE j.InvoiceId LIKE 'DEMO-%' AND ABS(j.InvoiceAmount-COALESCE(x.Net,0))>0.01
UNION ALL SELECT 'CustomerOpenBalanceMismatch', COUNT(*) FROM CustTransOpen o
JOIN CustTrans t ON t.RECID=o.RefRecId AND t.DataAreaId=o.DataAreaId
WHERE ABS(o.AmountCur-(t.AmountCur-t.SettleAmountCur))>0.01
UNION ALL SELECT 'InventoryOriginMissing', COUNT(*) FROM InventTrans t
LEFT JOIN InventTransOrigin o ON o.RECID=t.InventTransOrigin AND o.DataAreaId=t.DataAreaId WHERE o.RECID IS NULL
UNION ALL SELECT 'InventoryDimensionMissing', COUNT(*) FROM InventTrans t
LEFT JOIN InventDim d ON d.InventDimId=t.InventDimId AND d.DataAreaId=t.DataAreaId WHERE d.RECID IS NULL
UNION ALL SELECT 'InventoryFinancialQuantityMismatch', COUNT(*) FROM InventSum s
CROSS APPLY (SELECT SUM(t.Qty) AS Quantity FROM InventTrans t WHERE t.DataAreaId=s.DataAreaId AND t.ItemId=s.ItemId
             AND t.InventDimId=s.InventDimId AND (t.StatusReceipt=5 OR t.StatusIssue=5)) x
WHERE ABS(s.PostedQty-COALESCE(x.Quantity,0))>0.000001
UNION ALL SELECT 'SiteDefaultStatusMissing', COUNT(*) FROM InventSite s
LEFT JOIN WHSInventStatus i ON i.DataAreaId=s.DataAreaId AND i.InventStatusId=s.DefaultInventStatusID
WHERE s.DefaultInventStatusID<>'' AND i.RECID IS NULL
UNION ALL SELECT 'RequestVariableProcessMismatch', COUNT(*) FROM WfRequestVariables v
JOIN WfRequests r ON r.RECID=v.RequestId JOIN WfVariables d ON d.RECID=v.VariableId WHERE d.ProcessId<>r.ProcessId
UNION ALL SELECT 'DraftTaxWithoutSalesLine', COUNT(*) FROM TaxJournalTrans t
LEFT JOIN SalesLine l ON l.RECID=t.TransRecId AND l.DataAreaId=t.DataAreaId
WHERE t.TransTableId=2003 AND l.RECID IS NULL
UNION ALL SELECT 'SettlementSelectionWithoutOpenTransaction', COUNT(*) FROM SpecTrans s
LEFT JOIN CustTransOpen o ON o.RECID=s.RefRecId AND o.DataAreaId=s.RefCompany
WHERE s.RefTableId=472585700 AND o.RECID IS NULL
UNION ALL SELECT 'SettlementSelectionWithoutPaymentLine', COUNT(*) FROM SpecTrans s
LEFT JOIN LedgerJournalTrans l ON l.RECID=s.SpecRecId AND l.DataAreaId=s.SpecCompany
WHERE s.SpecTableId=1864676057 AND l.RECID IS NULL;
