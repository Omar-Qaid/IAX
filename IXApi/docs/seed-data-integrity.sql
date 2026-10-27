-- Read-only checks for a Development SQL Server database after seeding.
-- Run with: sqlcmd -S . -d ERM -E -i IXApi/docs/seed-data-integrity.sql
SET NOCOUNT ON;

-- Validate every declared FK and CHECK constraint, not just enabled/trusted flags.
DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS, ALL_ERRORMSGS;

-- All user tables, including tables that should remain empty until business activity.
SELECT SCHEMA_NAME(t.schema_id) AS SchemaName,
       t.name AS TableName,
       COALESCE(SUM(CASE WHEN p.index_id IN (0, 1) THEN p.row_count ELSE 0 END), 0) AS RowTotal
FROM sys.tables AS t
LEFT JOIN sys.dm_db_partition_stats AS p ON p.object_id = t.object_id
WHERE t.is_ms_shipped = 0
GROUP BY t.schema_id, t.name
ORDER BY t.name;

-- A disabled or untrusted foreign key weakens the database integrity guarantee.
SELECT COUNT(*) AS ForeignKeys,
       SUM(CASE WHEN is_disabled = 1 THEN 1 ELSE 0 END) AS Disabled,
       SUM(CASE WHEN is_not_trusted = 1 THEN 1 ELSE 0 END) AS Untrusted
FROM sys.foreign_keys;

-- Latest outcome for each registered seeder ID.
WITH latest AS (
    SELECT SeederId, Outcome,
           ROW_NUMBER() OVER (
               PARTITION BY SeederId ORDER BY CreatedAt DESC, RecId DESC
           ) AS rn
    FROM SysDataSeedLogs
    WHERE SeederId IS NOT NULL
)
SELECT Outcome, COUNT(*) AS SeederCount
FROM latest
WHERE rn = 1
GROUP BY Outcome;

-- Connected example data and its most important parent references.
SELECT 'Customers' AS Entity, COUNT(*) AS Rows FROM CustTable
UNION ALL SELECT 'Workers', COUNT(*) FROM HcmWorker
UNION ALL SELECT 'Products', COUNT(*) FROM InventTable
UNION ALL SELECT 'WorkflowRequests', COUNT(*) FROM WfRequests;

SELECT 'CustomerWithoutParty' AS CheckName, COUNT(*) AS Violations
FROM CustTable AS c LEFT JOIN DirPartyTable AS p ON p.RECID = c.Party
WHERE p.RECID IS NULL
UNION ALL
SELECT 'WorkerWithoutParty', COUNT(*)
FROM HcmWorker AS w LEFT JOIN DirPartyTable AS p ON p.RECID = w.Person
WHERE p.RECID IS NULL
UNION ALL
SELECT 'RequestWithoutProcess', COUNT(*)
FROM WfRequests AS r LEFT JOIN WfProcesses AS p ON p.RECID = r.ProcessId
WHERE p.RECID IS NULL
UNION ALL
SELECT 'RequestWithoutEmployee', COUNT(*)
FROM WfRequests AS r LEFT JOIN HcmWorker AS w ON w.RECID = r.EmployeeId
WHERE r.EmployeeId IS NOT NULL AND w.RECID IS NULL
UNION ALL
SELECT 'CustomerMissingGroup', COUNT(*)
FROM CustTable AS c LEFT JOIN CustGroup AS g ON g.CustGroupId = c.CustGroupId AND g.DataAreaId = c.DataAreaId
WHERE g.CustGroupId IS NULL
UNION ALL
SELECT 'ProductMissingModules', COUNT(*)
FROM InventTable AS i
WHERE 3 <> (
    SELECT COUNT(DISTINCT m.ModuleType) FROM InventTableModule AS m
    WHERE m.ItemId = i.ItemId AND m.DataAreaId = i.DataAreaId
)
UNION ALL
SELECT 'WorkerMissingNationality', COUNT(*)
FROM HcmWorker AS w LEFT JOIN HcmNationalities AS n ON n.RECID = w.NationalityId
WHERE w.NationalityId IS NOT NULL AND n.RECID IS NULL;

-- Semantic references not consistently covered by database foreign keys.
SELECT 'ReleasedItemWithoutProduct' AS CheckName, COUNT(*) AS Violations
FROM InventTable i LEFT JOIN EcoResProduct p ON p.RECID = i.Product WHERE p.RECID IS NULL
UNION ALL SELECT 'ItemWithoutGroup', COUNT(*) FROM InventTable i
WHERE NOT EXISTS (SELECT 1 FROM InventItemGroupItem g WHERE g.ItemId=i.ItemId AND g.ItemDataAreaId=i.DataAreaId)
UNION ALL SELECT 'InvalidItemGroupLink', COUNT(*) FROM InventItemGroupItem i
LEFT JOIN InventItemGroup g ON g.ItemGroupId=i.ItemGroupId AND g.DataAreaId=i.ItemGroupDataAreaId WHERE g.RECID IS NULL
UNION ALL SELECT 'BlankItemGroupCode', COUNT(*) FROM InventItemGroup WHERE LTRIM(RTRIM(ItemGroupId))=''
UNION ALL SELECT 'ModuleWithoutItemTaxGroup', COUNT(*) FROM InventTableModule m
LEFT JOIN TaxItemGroupHeading g ON g.TaxItemGroup=m.TaxItemGroupId AND g.DataAreaId=m.DataAreaId
WHERE m.TaxItemGroupId<>'' AND g.RECID IS NULL
UNION ALL SELECT 'ModuleWithoutUnit', COUNT(*) FROM InventTableModule m
LEFT JOIN UnitOfMeasure u ON u.Symbol=m.UnitId WHERE u.RECID IS NULL
UNION ALL SELECT 'ConversionWithoutProduct', COUNT(*) FROM UnitOfMeasureConversion c
LEFT JOIN EcoResProduct p ON p.RECID=c.Product WHERE c.Product<>0 AND p.RECID IS NULL
UNION ALL SELECT 'VendorWithoutParty', COUNT(*) FROM VendTable v
LEFT JOIN DirPartyTable p ON p.RECID=v.Party WHERE p.RECID IS NULL
UNION ALL SELECT 'VendorWithoutGroup', COUNT(*) FROM VendTable v
LEFT JOIN VendGroup g ON g.VendGroupCode=v.VendGroup AND g.DataAreaId=v.DataAreaId WHERE g.RECID IS NULL;

-- Workflow child records must belong to the same process, even when each FK exists.
SELECT 'ActivityControlProcessMismatch' AS CheckName, COUNT(*) AS Violations
FROM WfActivityControls c JOIN WfActivities a ON a.RECID=c.ActivityId JOIN WfSteps s ON s.StepId=a.StepId
WHERE c.ProcessId<>s.ProcessId
UNION ALL SELECT 'RequestDetailProcessMismatch', COUNT(*)
FROM WfRequestDetails d JOIN WfRequests r ON r.RECID=d.RequestId
WHERE d.ProcessId IS NOT NULL AND d.ProcessId<>r.ProcessId
UNION ALL SELECT 'TransitionStepProcessMismatch', COUNT(*)
FROM WfTransitions t JOIN WfSteps s ON s.StepId=t.StepId WHERE t.ProcessId<>s.ProcessId
UNION ALL SELECT 'TransitionControlProcessMismatch', COUNT(*)
FROM WfTransitions t JOIN WfRequestControls c ON c.RequestControlId=t.RequestControlId WHERE t.ProcessId<>c.ProcessId
UNION ALL SELECT 'InvalidFiscalPeriodRange', COUNT(*) FROM FiscalCalendarPeriod p
JOIN FiscalCalendarYear y ON y.RECID=p.FiscalCalendarYear
WHERE p.StartDate>p.EndDate OR p.StartDate<y.StartDate OR p.EndDate>y.EndDate OR p.FiscalCalendar<>y.FiscalCalendar;
