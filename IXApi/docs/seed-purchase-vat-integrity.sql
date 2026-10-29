-- Run after PurchaseInvoiceTaxAndReceiptMatching and Finance.PurchaseVatInvoices.
SELECT 'PurchaseVatExampleMissing' AS CheckName,
 CASE WHEN (SELECT COUNT(*) FROM PurchTable WHERE DataAreaId='dat' AND PurchName='DEMO-PO-VAT-6040')=1 THEN 0 ELSE 1 END AS Violations
UNION ALL SELECT 'PurchaseVatExampleTotals', COUNT(*)
FROM PurchTable p
CROSS APPLY (SELECT SUM(j.SalesBalance) Net, SUM(j.SumTax) Tax, SUM(j.InvoiceAmount) Gross
 FROM VendInvoiceJour j WHERE j.DataAreaId=p.DataAreaId AND j.PurchId=p.PurchId) t
WHERE p.PurchName='DEMO-PO-VAT-6040'
 AND (COALESCE(t.Net,0)<>5000 OR COALESCE(t.Tax,0)<>750 OR COALESCE(t.Gross,0)<>5750)
UNION ALL SELECT 'PurchaseReceiptMatchOwnership', COUNT(*)
FROM VendInvoicePackingSlipQuantityMatch m
JOIN VendInvoiceTrans i ON i.RECID=m.InvoiceTransRecId
JOIN VendPackingSlipTrans r ON r.RECID=m.PackingSlipTransRecId
WHERE m.DataAreaId<>i.DataAreaId OR m.DataAreaId<>r.DataAreaId
 OR i.PurchId<>r.OrigPurchId OR i.InventTransId<>r.InventTransId OR i.InventDimId<>r.InventDimId
 OR m.Quantity<=0 OR m.InventQuantity<=0
UNION ALL SELECT 'PurchaseReceiptOvermatched', COUNT(*)
FROM VendPackingSlipTrans r
CROSS APPLY (SELECT SUM(m.InventQuantity) Qty FROM VendInvoicePackingSlipQuantityMatch m
 WHERE m.PackingSlipTransRecId=r.RECID AND m.DataAreaId=r.DataAreaId) t
WHERE t.Qty>r.InventQty+0.000001
UNION ALL SELECT 'PurchaseInvoiceMatchQuantity', COUNT(*)
FROM VendInvoiceTrans i
CROSS APPLY (SELECT SUM(m.InventQuantity) Qty FROM VendInvoicePackingSlipQuantityMatch m
 WHERE m.InvoiceTransRecId=i.RECID AND m.DataAreaId=i.DataAreaId) t
WHERE t.Qty IS NOT NULL AND ABS(t.Qty-i.InventQty)>0.000001
UNION ALL SELECT 'PurchaseInputTaxSnapshot', COUNT(*)
FROM VendInvoiceTrans i
CROSS APPLY (SELECT SUM(t.TaxAmount) Amount FROM TaxTrans t WHERE t.DataAreaId=i.DataAreaId
 AND t.SourceTableId=1326239103 AND t.SourceRecId=i.RECID) t
WHERE ABS(i.TaxAmount-COALESCE(t.Amount,0))>0.01;

SELECT j.InvoiceId,j.SalesBalance,j.SumTax,j.InvoiceAmount,j.DueDate
FROM VendInvoiceJour j WHERE j.InvoiceId LIKE 'DEMO-PO-VAT-6040%';

SELECT (SELECT COUNT(*) FROM PurchTable WHERE DataAreaId='dat' AND PurchName='DEMO-PO-VAT-6040') Orders,
 (SELECT COUNT(*) FROM VendInvoiceJour WHERE DataAreaId='dat' AND InvoiceId LIKE 'DEMO-PO-VAT-6040%') Invoices,
 (SELECT COUNT(*) FROM VendPackingSlipJour WHERE DataAreaId='dat' AND PackingSlipId LIKE 'DEMO-PO-VAT-6040%') Receipts,
 (SELECT COUNT(*) FROM VendInvoicePackingSlipQuantityMatch m JOIN VendInvoiceTrans i ON i.RECID=m.InvoiceTransRecId
   WHERE i.DataAreaId='dat' AND i.InvoiceId LIKE 'DEMO-PO-VAT-6040%') Matches;
