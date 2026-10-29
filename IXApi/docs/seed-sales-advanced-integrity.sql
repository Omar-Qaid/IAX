SELECT 'AdvancedSalesOrdersMissing' CheckName, CASE WHEN COUNT(*)=2 THEN 0 ELSE 1 END Violations
FROM SalesTable WHERE DataAreaId='dat' AND CustomerRef IN ('DEMO-SO-TWOITEM','DEMO-SO-DISCOUNT-UOM')
UNION ALL SELECT 'AdvancedSalesTotals',COUNT(*) FROM SalesTable s
CROSS APPLY (SELECT COUNT(*) Docs,SUM(SalesBalance) Net,SUM(SumTax) Tax,SUM(InvoiceAmount) Gross
 FROM CustInvoiceJour j WHERE j.DataAreaId=s.DataAreaId AND j.SalesId=s.SalesId) a
WHERE s.DataAreaId='dat' AND s.CustomerRef IN ('DEMO-SO-TWOITEM','DEMO-SO-DISCOUNT-UOM')
 AND (a.Docs<>1 OR a.Net<>CASE WHEN s.CustomerRef='DEMO-SO-TWOITEM' THEN 2000 ELSE 1070 END
 OR a.Tax<>a.Net*.15 OR a.Gross<>a.Net*1.15)
UNION ALL SELECT 'AdvancedSalesLineCount',COUNT(*) FROM SalesTable s
WHERE s.DataAreaId='dat' AND s.CustomerRef IN ('DEMO-SO-TWOITEM','DEMO-SO-DISCOUNT-UOM')
AND (SELECT COUNT(*) FROM SalesLine l WHERE l.DataAreaId=s.DataAreaId AND l.SalesId=s.SalesId)<>2
UNION ALL SELECT 'AdvancedSalesRemainders',COUNT(*) FROM SalesLine l JOIN SalesTable s ON s.SalesId=l.SalesId AND s.DataAreaId=l.DataAreaId
WHERE s.DataAreaId='dat' AND s.CustomerRef IN ('DEMO-SO-TWOITEM','DEMO-SO-DISCOUNT-UOM')
AND (l.RemainSalesPhysical<>0 OR l.RemainSalesFinancial<>0 OR l.RemainInventPhysical<>0 OR l.RemainInventFinancial<>0)
UNION ALL SELECT 'AdvancedSalesUnitConversion',COUNT(*) FROM SalesLine l JOIN SalesTable s ON s.SalesId=l.SalesId AND s.DataAreaId=l.DataAreaId
WHERE s.DataAreaId='dat' AND s.CustomerRef='DEMO-SO-DISCOUNT-UOM' AND l.ItemId='ITM-1002'
AND (l.SalesUnit<>'BOX' OR l.SalesQty<>1 OR l.QtyOrdered<>5)
UNION ALL SELECT 'AdvancedSalesConfirmationHistory',CASE WHEN COUNT(*)=2 THEN 0 ELSE 1 END
FROM CustConfirmJour WHERE DataAreaId='dat' AND ParmId IN ('DEMO-SO-TWOITEM-C1','DEMO-SO-TWOITEM-C2')
UNION ALL SELECT 'AdvancedSalesConfirmationSnapshots',COUNT(*) FROM CustConfirmJour j
WHERE j.DataAreaId='dat' AND j.ParmId IN ('DEMO-SO-TWOITEM-C1','DEMO-SO-TWOITEM-C2')
AND NOT EXISTS (SELECT 1 FROM CustConfirmTrans t WHERE t.DataAreaId=j.DataAreaId AND t.ConfirmId=j.ConfirmId
 AND t.ItemId='ITEM-001' AND t.Qty=CASE WHEN j.ParmId='DEMO-SO-TWOITEM-C1' THEN 9 ELSE 10 END)
UNION ALL SELECT 'AdvancedSalesUnshippedPicks',COUNT(*) FROM InventTrans t
JOIN InventTransOrigin o ON o.RECID=t.InventTransOrigin AND o.DataAreaId=t.DataAreaId
JOIN SalesTable s ON s.SalesId=o.ReferenceId AND s.DataAreaId=o.DataAreaId
WHERE s.DataAreaId='dat' AND s.CustomerRef IN ('DEMO-SO-TWOITEM','DEMO-SO-DISCOUNT-UOM')
AND t.Qty<>0 AND (t.StatusIssue<>5 OR t.DatePhysical IS NULL OR t.DateFinancial IS NULL);

SELECT s.CustomerRef,j.InvoiceId,j.SalesBalance,j.SumTax,j.InvoiceAmount
FROM SalesTable s JOIN CustInvoiceJour j ON j.DataAreaId=s.DataAreaId AND j.SalesId=s.SalesId
WHERE s.DataAreaId='dat' AND s.CustomerRef IN ('DEMO-SO-TWOITEM','DEMO-SO-DISCOUNT-UOM');
