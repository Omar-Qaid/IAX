-- Read-only inventory schema audit. Run against the authoritative copied AX/D365 database.
-- Save the result with the database name and date before changing enum mappings or posting logic.
SET NOCOUNT ON;
SELECT DB_NAME() AS DatabaseName, SYSDATETIMEOFFSET() AS AuditTime;

DECLARE @InventoryTables table (TableName sysname PRIMARY KEY);
INSERT INTO @InventoryTables (TableName) VALUES
('InventDim'), ('InventTransOrigin'), ('InventTransOriginSalesLine'),
('InventTrans'), ('InventTransPosting'), ('InventSum'), ('InventSettlement'), ('InventClosing'),
('InventJournalName'), ('InventJournalTable'), ('InventJournalTrans'),
('InventCountJour'), ('InventParameters'), ('InventPosting'),
('SalesTable'), ('SalesLine'), ('CustPackingSlipJour'), ('CustPackingSlipTrans'),
('CustInvoiceJour'), ('CustInvoiceTrans'), ('PurchTable'), ('PurchLine'),
('VendPackingSlipJour'), ('VendPackingSlipTrans'), ('VendInvoiceJour'), ('VendInvoiceTrans'),
('GeneralJournalEntry'), ('GeneralJournalAccountEntry'), ('SubledgerVoucherGeneralJournalEntry');

SELECT wanted.TableName, schema_name(tab.schema_id) AS SchemaName,
       CASE WHEN tab.object_id IS NULL THEN 0 ELSE 1 END AS Present
FROM @InventoryTables AS wanted
LEFT JOIN sys.tables AS tab ON tab.name = wanted.TableName
ORDER BY wanted.TableName, SchemaName;

SELECT tab.name AS TableName, col.column_id AS ColumnOrdinal, col.name AS ColumnName,
       typ.name AS SqlType, col.max_length AS MaxLength, col.precision AS Precision,
       col.scale AS Scale, col.is_nullable AS IsNullable, col.is_identity AS IsIdentity
FROM sys.tables AS tab
JOIN @InventoryTables AS wanted ON wanted.TableName = tab.name
JOIN sys.columns AS col ON col.object_id = tab.object_id
JOIN sys.types AS typ ON typ.user_type_id = col.user_type_id
ORDER BY tab.name, col.column_id;

SELECT tab.name AS TableName, col.name AS ColumnName, def.name AS DefaultName,
       def.definition AS DefaultDefinition
FROM sys.default_constraints AS def
JOIN sys.tables AS tab ON tab.object_id = def.parent_object_id
JOIN @InventoryTables AS wanted ON wanted.TableName = tab.name
JOIN sys.columns AS col ON col.object_id = tab.object_id AND col.column_id = def.parent_column_id
ORDER BY tab.name, col.name;

SELECT tab.name AS TableName, chk.name AS CheckName, chk.definition AS CheckDefinition
FROM sys.check_constraints AS chk
JOIN sys.tables AS tab ON tab.object_id = chk.parent_object_id
JOIN @InventoryTables AS wanted ON wanted.TableName = tab.name
ORDER BY tab.name, chk.name;

SELECT tab.name AS TableName, idx.name AS IndexName, idx.is_unique AS IsUnique,
       idx.is_primary_key AS IsPrimaryKey, ic.key_ordinal AS KeyOrdinal,
       col.name AS ColumnName, ic.is_included_column AS IsIncluded
FROM sys.tables AS tab
JOIN @InventoryTables AS wanted ON wanted.TableName = tab.name
JOIN sys.indexes AS idx ON idx.object_id = tab.object_id AND idx.index_id > 0
JOIN sys.index_columns AS ic ON ic.object_id = idx.object_id AND ic.index_id = idx.index_id
JOIN sys.columns AS col ON col.object_id = ic.object_id AND col.column_id = ic.column_id
ORDER BY tab.name, idx.name, ic.key_ordinal, col.name;

SELECT parent.name AS ParentTable, fk.name AS ForeignKeyName,
       parent_col.name AS ParentColumn, referenced.name AS ReferencedTable,
       referenced_col.name AS ReferencedColumn
FROM sys.foreign_keys AS fk
JOIN sys.tables AS parent ON parent.object_id = fk.parent_object_id
JOIN sys.tables AS referenced ON referenced.object_id = fk.referenced_object_id
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns AS parent_col ON parent_col.object_id = parent.object_id AND parent_col.column_id = fkc.parent_column_id
JOIN sys.columns AS referenced_col ON referenced_col.object_id = referenced.object_id AND referenced_col.column_id = fkc.referenced_column_id
WHERE parent.name IN (SELECT TableName FROM @InventoryTables)
ORDER BY parent.name, fk.name, fkc.constraint_column_id;

IF OBJECT_ID(N'dbo.InventTrans', N'U') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'SELECT StatusIssue, StatusReceipt, COUNT_BIG(*) AS Rows
        FROM dbo.InventTrans GROUP BY StatusIssue, StatusReceipt
        ORDER BY StatusIssue, StatusReceipt';
END;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'SELECT MigrationId, ProductVersion
        FROM dbo.__EFMigrationsHistory ORDER BY MigrationId';
END;
