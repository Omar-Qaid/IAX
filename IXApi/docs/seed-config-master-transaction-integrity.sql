-- Read-only cross-layer checks for the connected ERP examples in a disposable fixture.
SET NOCOUNT ON;
IF DB_NAME() NOT LIKE 'IAX[_]SeedIntegrity[_]%'
 OR NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'IAXSeedVerification' AND CONVERT(nvarchar(100),value)=N'owned-isolated-fixture')
 THROW 51000, 'Use a marked, isolated seed verification database.',1;

DECLARE @checks TABLE(CheckName nvarchar(100), Violations bigint);
INSERT @checks
SELECT 'CashDiscountLedgerCompany',COUNT_BIG(*) FROM CashDisc c
 LEFT JOIN DimensionAttributeValueCombination d ON d.RECID=c.CashDiscLedgerDimension
 WHERE c.CashDiscCode='ERP2P10' AND (d.RECID IS NULL OR c.DataAreaId<>d.DataAreaId OR d.IsDeleted=1)
UNION ALL SELECT 'ExchangeSetupLedgerAndDimensionCompany',COUNT_BIG(*) FROM LedgerGainLossAccount a
 LEFT JOIN Ledger l ON l.RECID=a.Ledger LEFT JOIN DimensionAttributeValueCombination d ON d.RECID=a.LedgerDimension
 WHERE a.DataAreaId='dat' AND (l.RECID IS NULL OR d.RECID IS NULL OR a.DataAreaId<>l.DataAreaId OR a.DataAreaId<>d.DataAreaId)
UNION ALL SELECT 'CurrencyExchangeSetupCompanyAndCurrency',COUNT_BIG(*) FROM CurrencyLedgerGainLossAccount a
 LEFT JOIN Ledger l ON l.RECID=a.Ledger LEFT JOIN DimensionAttributeValueCombination d ON d.RECID=a.LedgerDimension
 LEFT JOIN Currency c ON c.CurrencyCode=a.CurrencyCode AND c.DataAreaId=a.DataAreaId
 WHERE a.DataAreaId='dat' AND (l.RECID IS NULL OR d.RECID IS NULL OR c.RECID IS NULL OR a.DataAreaId<>l.DataAreaId OR a.DataAreaId<>d.DataAreaId)
UNION ALL SELECT 'MainAccountLegalEntityCompany',COUNT_BIG(*) FROM MainAccountLegalEntity a
 JOIN MainAccount m ON m.RECID=a.MainAccount JOIN CompanyInfo c ON c.RECID=a.LegalEntity
 WHERE m.MainAccountId IN ('60990001','40990001','60990002') AND m.DataAreaId<>c.DataArea
UNION ALL SELECT 'VendorDefaultPostingProfile',COUNT_BIG(*) FROM VendParameters p
 WHERE p.DataAreaId='dat' AND NOT EXISTS (SELECT 1 FROM VendLedgerAccounts a WHERE a.DataAreaId=p.DataAreaId AND a.PostingProfile=p.VendPostingProfile AND a.IsDeleted=0)
UNION ALL SELECT 'VendorPaymentJournalCompany',COUNT_BIG(*) FROM VendPaymModeTable m
 LEFT JOIN LedgerJournalName j ON j.DataAreaId=m.DataAreaId AND j.JournalName=m.PaymJournalNameId
 WHERE m.DataAreaId='dat' AND m.PaymJournalNameId<>'' AND (j.RECID IS NULL OR j.IsDeleted=1)
UNION ALL SELECT 'VendorMasterGroupAndCurrency',COUNT_BIG(*) FROM VendTable v
 LEFT JOIN VendGroup g ON g.DataAreaId=v.DataAreaId AND g.VendGroupCode=v.VendGroup
 LEFT JOIN Currency c ON c.DataAreaId=v.DataAreaId AND c.CurrencyCode=v.Currency
 WHERE v.DataAreaId='dat' AND (g.RECID IS NULL OR c.RECID IS NULL)
UNION ALL SELECT 'CustomerMasterGroupAndCurrency',COUNT_BIG(*) FROM CustTable v
 LEFT JOIN CustGroup g ON g.DataAreaId=v.DataAreaId AND g.CustGroupId=v.CustGroupId
 LEFT JOIN Currency c ON c.DataAreaId=v.DataAreaId AND c.CurrencyCode=v.CurrencyCode
 WHERE v.DataAreaId='dat' AND (g.RECID IS NULL OR c.RECID IS NULL)
UNION ALL SELECT 'SalesLineMasterAndHeaderCompany',COUNT_BIG(*) FROM SalesLine l
 LEFT JOIN SalesTable h ON h.DataAreaId=l.DataAreaId AND h.SalesId=l.SalesId
 LEFT JOIN InventTable i ON i.DataAreaId=l.DataAreaId AND i.ItemId=l.ItemId
 WHERE l.DataAreaId='dat' AND (h.RECID IS NULL OR i.RECID IS NULL)
UNION ALL SELECT 'PurchaseLineMasterAndHeaderCompany',COUNT_BIG(*) FROM PurchLine l
 LEFT JOIN PurchTable h ON h.DataAreaId=l.DataAreaId AND h.PurchId=l.PurchId
 LEFT JOIN InventTable i ON i.DataAreaId=l.DataAreaId AND i.ItemId=l.ItemId
 WHERE l.DataAreaId='dat' AND (h.RECID IS NULL OR i.RECID IS NULL)
UNION ALL SELECT 'ConnectedCustomerInvoiceSubledger',COUNT_BIG(*) FROM CustInvoiceJour i
 LEFT JOIN CustTrans t ON t.DataAreaId=i.DataAreaId AND t.Invoice=i.InvoiceId AND t.AccountNum='CUST-100'
 WHERE i.DataAreaId='dat' AND i.ParmId='ERP-CYCLE-SO-001' AND (t.RECID IS NULL OR ABS(t.AmountCur-i.InvoiceAmount)>.01)
UNION ALL SELECT 'ConnectedCustomerSettlementBalance',COUNT_BIG(*) FROM CustTrans t
 JOIN CustInvoiceJour i ON i.DataAreaId=t.DataAreaId AND i.InvoiceId=t.Invoice
 WHERE i.DataAreaId='dat' AND i.ParmId='ERP-CYCLE-SO-001' AND ABS(t.AmountCur-t.SettleAmountCur)>.01
UNION ALL SELECT 'CustomerOpenTransactionBalance',COUNT_BIG(*) FROM CustTransOpen o
 JOIN CustTrans t ON t.RECID=o.RefRecId
 WHERE o.DataAreaId='dat' AND (o.DataAreaId<>t.DataAreaId OR o.AccountNum<>t.AccountNum OR ABS(o.AmountCur-(t.AmountCur-t.SettleAmountCur))>.01)
UNION ALL SELECT 'MainAccountSegmentSentinel',COUNT_BIG(*) FROM DimensionHierarchyLevel WHERE DimensionAttribute=0;

SELECT CheckName,Violations FROM @checks ORDER BY CheckName;
IF EXISTS(SELECT 1 FROM @checks WHERE Violations<>0)
 THROW 51000, 'Seed configuration, master or transaction conflicts were found; data was not changed.',1;
