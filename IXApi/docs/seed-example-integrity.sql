-- Additional read-only checks for the opt-in cross-module examples.
SET NOCOUNT ON;
SELECT 'LedgerWithoutChart' AS CheckName, COUNT(*) AS Violations
FROM Ledger l LEFT JOIN LedgerChartOfAccounts c ON c.RECID=l.ChartOfAccounts WHERE c.RECID IS NULL
UNION ALL SELECT 'LedgerWithoutCalendar', COUNT(*) FROM Ledger l
LEFT JOIN FiscalCalendar c ON c.RECID=l.FiscalCalendar WHERE c.RECID IS NULL
UNION ALL SELECT 'PostingLineWithoutSummaryAccount', COUNT(*) FROM CustLedgerAccounts l
LEFT JOIN DimensionAttributeValueCombination d ON d.RECID=l.SummaryLedgerDimension AND d.DataAreaId=l.DataAreaId
WHERE d.RECID IS NULL
UNION ALL SELECT 'NotificationRecipientWithoutUser', COUNT(*) FROM SysNotificationRecipients r
LEFT JOIN AspNetUsers u ON u.Id=r.UserId WHERE u.Id IS NULL
UNION ALL SELECT 'NotificationRecipientWithoutNotification', COUNT(*) FROM SysNotificationRecipients r
LEFT JOIN SysNotifications n ON n.RECID=r.NotificationId WHERE n.RECID IS NULL
UNION ALL SELECT 'DemoReminderQueuedForDispatch', COUNT(*) FROM SysScheduledNotifications
WHERE Title LIKE '[[]Example]%' AND Status IN (0,4)
UNION ALL SELECT 'ExampleDocumentWithoutCustomer', COUNT(*) FROM DocuRef r
LEFT JOIN CustTable c ON c.RECID=r.REFRECID AND c.DATAAREAID=r.REFCOMPANYID
WHERE r.DOCUMENTID IN ('8514c27f-60da-42e2-9079-53692617f101','8514c27f-60da-42e2-9079-53692617f102')
AND (r.REFTABLEID<>2001 OR c.RECID IS NULL)
UNION ALL SELECT 'ExampleDocumentWithoutValue', COUNT(*) FROM DocuRef r
LEFT JOIN DocuValue v ON v.RECID=r.VALUERECID
WHERE r.DOCUMENTID IN ('8514c27f-60da-42e2-9079-53692617f101','8514c27f-60da-42e2-9079-53692617f102')
AND v.RECID IS NULL;
