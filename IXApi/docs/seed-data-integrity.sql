-- Read-only checks for a Development SQL Server database after seeding.
-- Run with: sqlcmd -S . -d ERM -E -i IXApi/docs/seed-data-integrity.sql
SET NOCOUNT ON;

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
FROM CustTable AS c LEFT JOIN CustGroup AS g ON g.CustGroupId = c.CustGroupId
WHERE g.CustGroupId IS NULL
UNION ALL
SELECT 'ProductMissingModules', COUNT(*)
FROM InventTable AS i
WHERE NOT EXISTS (
    SELECT 1 FROM InventTableModule AS m
    WHERE m.ItemId = i.ItemId AND m.DataAreaId = i.DataAreaId
)
UNION ALL
SELECT 'WorkerMissingNationality', COUNT(*)
FROM HcmWorker AS w LEFT JOIN HcmNationalities AS n ON n.RECID = w.NationalityId
WHERE w.NationalityId IS NOT NULL AND n.RECID IS NULL;
