-- Read-only acceptance checks for the dedicated FIFO worked example.
SET NOCOUNT ON;
DECLARE @item nvarchar(20)='DEMO-FIFO115';
SELECT 'ExampleDocumentCounts' CheckName, CASE WHEN
 (SELECT COUNT(*) FROM PurchTable WHERE DataAreaId='dat' AND PurchName='FIFO115-PURCHASE' AND ReturnItemNum='')=1
 AND (SELECT COUNT(*) FROM SalesTable WHERE DataAreaId='dat' AND CustomerRef='FIFO115-SALE')=1
 AND (SELECT COUNT(*) FROM InventJournalTable WHERE DataAreaId='dat' AND Description IN ('FIFO115-OPEN','FIFO115-TRANSFER','FIFO115-DAMAGE','FIFO115-COUNT'))=4
 AND (SELECT COUNT(*) FROM VendInvoiceJour WHERE DataAreaId='dat' AND InvoiceId IN ('FIFO115-PI','FIFO115-VC'))=2
 THEN 0 ELSE 1 END Violations;
SELECT 'FinalWarehouseQuantities' CheckName,
 CASE WHEN SUM(PhysicalInvent)=115
  AND SUM(CASE WHEN InventLocationId='FIFO-WH1' THEN PhysicalInvent ELSE 0 END)=55
  AND SUM(CASE WHEN InventLocationId='FIFO-WH2' THEN PhysicalInvent ELSE 0 END)=60
  AND SUM(CASE WHEN InventLocationId='FIFO-TR' THEN PhysicalInvent ELSE 0 END)=0 THEN 0 ELSE 1 END Violations
FROM InventSum WHERE DataAreaId='dat' AND ItemId=@item
UNION ALL SELECT 'TransitQuantityAndValue',COUNT(*) FROM InventSum
 WHERE DataAreaId='dat' AND ItemId=@item AND InventLocationId='FIFO-TR' AND (PhysicalInvent<>0 OR ABS(PostedValue+PhysicalValue)>.01)
UNION ALL SELECT 'CountPostedAndReleased',CASE WHEN COUNT(*)=1 THEN 0 ELSE 1 END
 FROM InventJournalTable h JOIN InventJournalTrans l ON l.DataAreaId=h.DataAreaId AND l.JournalId=h.JournalId
 WHERE h.DataAreaId='dat' AND h.Description='FIFO115-COUNT' AND h.Posted=1 AND h.CountingConfirmed=1 AND h.CountingFrozen=0
 AND l.ItemId=@item AND l.InventOnHand=95 AND l.Counted=98 AND l.Qty=3
UNION ALL SELECT 'TransitOrderComplete',CASE WHEN COUNT(*)=1 THEN 0 ELSE 1 END FROM InventTransferOrder
 WHERE DataAreaId='dat' AND TransferId='FIFO115-TRANSIT' AND ItemId=@item AND Quantity=40 AND Shipped=40 AND Received=40
UNION ALL SELECT 'TransitPostingCount',CASE WHEN COUNT(*)=2 AND SUM(CASE WHEN IsReceipt=1 THEN p.Quantity ELSE -p.Quantity END)=0
 AND ABS(SUM(CASE WHEN IsReceipt=1 THEN CostAmount ELSE -CostAmount END))<.01 THEN 0 ELSE 1 END
 FROM InventTransferPosting p JOIN InventTransferOrder o ON o.RECID=p.TransferOrderRecId AND o.DataAreaId=p.DataAreaId
 WHERE o.DataAreaId='dat' AND o.TransferId='FIFO115-TRANSIT'
UNION ALL SELECT 'FifoClosePresent',CASE WHEN COUNT(*)=1 THEN 0 ELSE 1 END FROM InventClosing WHERE DataAreaId='dat' AND ScopeItemId=@item
UNION ALL SELECT 'FifoReceiptLinks',COUNT(*) FROM InventSettlement s LEFT JOIN InventTrans r ON r.RECID=s.ReceiptTransRecId AND r.DataAreaId=s.DataAreaId
 WHERE s.DataAreaId='dat' AND s.ItemId=@item AND (r.RECID IS NULL OR r.ItemId<>s.ItemId OR r.Qty<=0)
UNION ALL SELECT 'InventoryTransactionQuantity',CASE WHEN SUM(Qty)=115 THEN 0 ELSE 1 END FROM InventTrans WHERE DataAreaId='dat' AND ItemId=@item
UNION ALL SELECT 'OutstandingReservations',COUNT(*) FROM InventSum WHERE DataAreaId='dat' AND ItemId=@item AND (ReservPhysical<>0 OR Picked<>0)
UNION ALL SELECT 'StrictFifoPolicy',CASE WHEN COUNT(*)=1 THEN 0 ELSE 1 END FROM InventModelGroup g JOIN InventModelGroupItem i
 ON i.ModelGroupId=g.ModelGroupId AND i.ModelGroupDataAreaId=g.DataAreaId
 WHERE i.ItemDataAreaId='dat' AND i.ItemId=@item AND g.ModelGroupId='DEMOFIFO' AND g.NegativePhysical=0 AND g.NegativeFinancial=0;

SELECT 'SettledSalesCost' CheckName, CASE WHEN SUM(t.Qty)=-30 AND ABS(SUM(t.CostAmountPosted+t.CostAmountAdjustment)+1500)<.01 THEN 0 ELSE 1 END Violations
FROM InventTrans t JOIN InventTransOrigin o ON o.RECID=t.InventTransOrigin AND o.DataAreaId=t.DataAreaId
JOIN SalesLine l ON l.InventTransId=o.InventTransId AND l.DataAreaId=o.DataAreaId
JOIN SalesTable s ON s.SalesId=l.SalesId AND s.DataAreaId=l.DataAreaId
WHERE s.DataAreaId='dat' AND s.CustomerRef='FIFO115-SALE'
UNION ALL SELECT 'SettledCustomerReturnCost',CASE WHEN SUM(t.Qty)=2 AND ABS(SUM(t.CostAmountPosted+t.CostAmountAdjustment)-100)<.01 THEN 0 ELSE 1 END
FROM InventTrans t JOIN InventTransOrigin o ON o.RECID=t.InventTransOrigin AND o.DataAreaId=t.DataAreaId
JOIN SalesLine l ON l.InventTransId=o.InventTransId AND l.DataAreaId=o.DataAreaId
WHERE t.DataAreaId='dat' AND t.ItemId=@item AND l.InventTransIdReturn<>''
UNION ALL SELECT 'SettledVendorReturnCost',CASE WHEN SUM(t.Qty)=-5 AND ABS(SUM(t.CostAmountPosted+t.CostAmountAdjustment)+300)<.01 THEN 0 ELSE 1 END
FROM InventTrans t JOIN InventTransOrigin o ON o.RECID=t.InventTransOrigin AND o.DataAreaId=t.DataAreaId
JOIN PurchLine l ON l.InventTransId=o.InventTransId AND l.DataAreaId=o.DataAreaId
WHERE t.DataAreaId='dat' AND t.ItemId=@item AND l.InventTransIdReturn<>'';

SELECT 'InventoryValueReconciliation' CheckName,
 CASE WHEN ABS(t.Value-s.Value)<.02 THEN 0 ELSE 1 END Violations,t.Value TransactionValue,s.Value SummaryValue
FROM (SELECT SUM(CostAmountPosted+CostAmountAdjustment) Value FROM InventTrans WHERE DataAreaId='dat' AND ItemId=@item) t
CROSS JOIN (SELECT SUM(PostedValue) Value FROM InventSum WHERE DataAreaId='dat' AND ItemId=@item) s;

-- The example uses the existing all-item inventory posting profiles; its documents contain only this item.
WITH Vouchers AS (
 SELECT Voucher FROM InventTrans WHERE DataAreaId='dat' AND ItemId=@item
 UNION SELECT Voucher FROM InventClosing WHERE DataAreaId='dat' AND ScopeItemId=@item
), InventoryAccounts AS (
 SELECT DISTINCT LedgerDimension FROM InventPosting WHERE DataAreaId='dat' AND ItemCode=2 AND CustVendCode=2
 AND InventAccountType IN (2,7,9,21)
)
SELECT 'InventoryLedgerReconciliation' CheckName,
 CASE WHEN ABS(COALESCE(SUM(a.AccountingCurrencyAmount),0)-(SELECT SUM(PostedValue) FROM InventSum WHERE DataAreaId='dat' AND ItemId=@item))<.02 THEN 0 ELSE 1 END Violations,
 SUM(a.AccountingCurrencyAmount) InventoryLedgerValue
FROM GeneralJournalAccountEntry a JOIN GeneralJournalEntry e ON e.RECID=a.GeneralJournalEntry
WHERE e.DataAreaId='dat' AND e.SubledgerVoucher IN (SELECT Voucher FROM Vouchers)
AND a.LedgerDimension IN (SELECT LedgerDimension FROM InventoryAccounts);

SELECT InventLocationId,PhysicalInvent,PostedQty,PostedValue,Picked,ReservPhysical
FROM InventSum WHERE DataAreaId='dat' AND ItemId=@item ORDER BY InventLocationId;
