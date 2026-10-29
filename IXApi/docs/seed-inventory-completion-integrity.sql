SELECT 'InventoryCompletionJournalsMissing' CheckName, CASE WHEN COUNT(*)=2 THEN 0 ELSE 1 END Violations
FROM InventJournalTable WHERE DataAreaId='dat' AND Description IN ('DEMO-INV-MOVEMENT','DEMO-INV-TRANSFER')
UNION ALL SELECT 'InventoryTransferQuantityUnbalanced',COUNT(*) FROM InventJournalTable j
JOIN InventJournalTrans l ON l.JournalId=j.JournalId AND l.DataAreaId=j.DataAreaId
CROSS APPLY (SELECT SUM(t.Qty) Qty, SUM(t.CostAmountPosted) Cost, COUNT(*) Entries FROM InventTrans t
 JOIN InventTransOrigin o ON o.RECID=t.InventTransOrigin AND o.DataAreaId=t.DataAreaId
 WHERE o.DataAreaId=j.DataAreaId AND o.InventTransId IN (l.InventTransId,l.ToInventTransId)) a
WHERE j.DataAreaId='dat' AND j.Description='DEMO-INV-TRANSFER'
 AND (a.Entries<>2 OR ABS(COALESCE(a.Qty,1))>0.000001 OR ABS(COALESCE(a.Cost,1))>0.01)
UNION ALL SELECT 'InventoryVendorCreditMismatch', CASE WHEN COUNT(*)=1 AND SUM(SalesBalance)=-250
 AND SUM(SumTax)=-37.5 AND SUM(InvoiceAmount)=-287.5 THEN 0 ELSE 1 END
FROM VendInvoiceJour WHERE DataAreaId='dat' AND InvoiceId='DEMO-VRET-INV-CN';

SELECT j.Description,j.JournalId,j.Posted,l.ItemId,l.Qty,l.CostAmount,l.InventDimId,l.ToInventDimId
FROM InventJournalTable j JOIN InventJournalTrans l ON l.JournalId=j.JournalId AND l.DataAreaId=j.DataAreaId
WHERE j.DataAreaId='dat' AND j.Description IN ('DEMO-INV-MOVEMENT','DEMO-INV-TRANSFER');
