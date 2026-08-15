/* Read-only number-sequence audit. Run in the intended ERP database. */
SET NOCOUNT ON;

SELECT NumberSequence, DataAreaId, NumberSequenceScope, COUNT(*) AS ConfigurationCount
FROM dbo.SysNumberSequences
WHERE IsDeleted = 0
GROUP BY NumberSequence, DataAreaId, NumberSequenceScope
HAVING COUNT(*) > 1;

SELECT RECID, NumberSequence, Lowest, Highest, NextRec, Manual, Blocked, InUse,
       IsActive, NoIncrement, FetchAhead, FetchAheadQty, Format, AnnotatedFormat,
       DataAreaId, NumberSequenceScope
FROM dbo.SysNumberSequences
WHERE IsDeleted = 0
  AND (
      Lowest > Highest OR
      NextRec < Lowest OR NextRec > Highest OR
      (ISNULL(Manual, 0) = 0 AND AnnotatedFormat NOT LIKE '%{SEQ}%') OR
      (FetchAhead = 1 AND ISNULL(FetchAheadQty, 0) <= 0)
  );

DECLARE @codedTables table (TableName sysname);
INSERT INTO @codedTables (TableName)
SELECT DISTINCT t.name
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name = 'Code';

DECLARE @table sysname, @sql nvarchar(max);
DECLARE coded_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM @codedTables;
OPEN coded_cursor;
FETCH NEXT FROM coded_cursor INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'SELECT ' + QUOTENAME(@table,'''') + N' AS TableName, Code, DataAreaId, COUNT(*) AS Occurrences
      FROM dbo.' + QUOTENAME(@table) + N'
      WHERE IsDeleted = 0 AND (Code IS NULL OR LTRIM(RTRIM(Code)) = '''' OR Code LIKE ''%{PREFIX}%'')
      GROUP BY Code, DataAreaId;
      SELECT ' + QUOTENAME(@table,'''') + N' AS TableName, Code, DataAreaId, COUNT(*) AS Occurrences
      FROM dbo.' + QUOTENAME(@table) + N'
      WHERE IsDeleted = 0 AND Code IS NOT NULL
      GROUP BY Code, DataAreaId HAVING COUNT(*) > 1;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM coded_cursor INTO @table;
END
CLOSE coded_cursor;
DEALLOCATE coded_cursor;
