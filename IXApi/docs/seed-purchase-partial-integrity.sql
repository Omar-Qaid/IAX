SET NOCOUNT ON;
SELECT p.PurchName,l.PurchQty,l.PurchPrice,l.RemainPurchPhysical,l.RemainPurchFinancial
FROM PurchTable p JOIN PurchLine l ON l.DataAreaId=p.DataAreaId AND l.PurchId=p.PurchId WHERE p.PurchName IN ('DEMO-PO-60-OPEN','DEMO-PO-60-40');
SELECT j.InvoiceId,j.Qty,j.InvoiceAmount,t.TransDate,t.DueDate FROM VendInvoiceJour j JOIN VendTrans t ON t.DataAreaId=j.DataAreaId AND t.Voucher=j.LedgerVoucher WHERE j.InvoiceId LIKE 'DEMO-PO-60-%';
SELECT 'PartialPurchaseQuantities' AS CheckName,COUNT(*) AS Violations FROM PurchTable p JOIN PurchLine l ON l.DataAreaId=p.DataAreaId AND l.PurchId=p.PurchId
WHERE p.PurchName IN ('DEMO-PO-60-OPEN','DEMO-PO-60-40') AND (l.PurchQty<>100 OR l.PurchPrice<>50 OR l.RemainPurchPhysical<>CASE WHEN p.PurchName='DEMO-PO-60-OPEN' THEN 40 ELSE 0 END OR l.RemainPurchFinancial<>CASE WHEN p.PurchName='DEMO-PO-60-OPEN' THEN 40 ELSE 0 END);
