SELECT 'SalesCompletionMissing' CheckName, CASE WHEN COUNT(*)=1 THEN 0 ELSE 1 END Violations
FROM SalesTable WHERE DataAreaId='dat' AND CustomerRef='DEMO-SO-COMPLETE'
UNION ALL SELECT 'SalesCompletionTotals', COUNT(*) FROM SalesTable s
CROSS APPLY (SELECT SUM(j.SalesBalance) Net,SUM(j.SumTax) Tax,SUM(j.InvoiceAmount) Gross,COUNT(*) Docs
 FROM CustInvoiceJour j WHERE j.DataAreaId=s.DataAreaId AND j.SalesId=s.SalesId) t
WHERE s.DataAreaId='dat' AND s.CustomerRef='DEMO-SO-COMPLETE'
 AND (t.Docs<>2 OR t.Net<>1000 OR t.Tax<>150 OR t.Gross<>1150)
UNION ALL SELECT 'SalesCompletionRemainders',COUNT(*) FROM SalesLine l JOIN SalesTable s ON s.DataAreaId=l.DataAreaId AND s.SalesId=l.SalesId
WHERE s.CustomerRef='DEMO-SO-COMPLETE' AND (l.RemainSalesPhysical<>0 OR l.RemainSalesFinancial<>0)
UNION ALL SELECT 'SalesCompletionOpenInvoice',COUNT(*) FROM CustTransOpen o JOIN CustTrans t ON t.RECID=o.RefRecId AND t.DataAreaId=o.DataAreaId
JOIN CustInvoiceJour i ON i.InvoiceId=t.Invoice AND i.DataAreaId=t.DataAreaId
JOIN SalesTable s ON s.SalesId=i.SalesId AND s.DataAreaId=i.DataAreaId WHERE s.CustomerRef='DEMO-SO-COMPLETE'
UNION ALL SELECT 'SalesCompletionCredit', CASE WHEN COUNT(*)=1 AND SUM(InvoiceAmount)=-115 THEN 0 ELSE 1 END
FROM CustInvoiceJour WHERE DataAreaId='dat' AND ParmId='DEMO-SO-COMPLETE-CN'
UNION ALL SELECT 'SalesCancellationMissing',CASE WHEN COUNT(*)=1 THEN 0 ELSE 1 END
FROM SalesTable WHERE DataAreaId='dat' AND CustomerRef='DEMO-SO-CANCEL-PRICE'
UNION ALL SELECT 'SalesCancellationRemainders',COUNT(*) FROM SalesLine l JOIN SalesTable s ON s.DataAreaId=l.DataAreaId AND s.SalesId=l.SalesId
WHERE s.CustomerRef='DEMO-SO-CANCEL-PRICE' AND (l.RemainSalesPhysical<>0 OR l.RemainSalesFinancial<>0);
