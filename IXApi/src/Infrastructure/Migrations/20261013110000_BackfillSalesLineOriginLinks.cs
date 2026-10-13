using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261013110000_BackfillSalesLineOriginLinks")]
public sealed class BackfillSalesLineOriginLinks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO [InventTransOriginSalesLine]
                ([InventTransOrigin], [SalesLineDataAreaId], [SalesLineInventTransId],
                 [IsDeleted], [IsActive], [RecVersion], [DataAreaId])
            SELECT origin.[RECID], line.[DATAAREAID], line.[InventTransId],
                   0, 1, 1, line.[DATAAREAID]
            FROM [SalesLine] AS line
            INNER JOIN [SalesTable] AS sales
                ON sales.[DATAAREAID] = line.[DATAAREAID]
                AND sales.[SalesId] = line.[SalesId]
                AND sales.[IsDeleted] = 0
            INNER JOIN [InventTransOrigin] AS origin
                ON origin.[DATAAREAID] = line.[DATAAREAID]
                AND origin.[InventTransId] = line.[InventTransId]
                AND origin.[ReferenceId] = line.[SalesId]
                AND origin.[IsDeleted] = 0
            WHERE line.[IsDeleted] = 0
                AND line.[InventTransId] <> ''
                AND NOT EXISTS (
                    SELECT 1
                    FROM [InventTransOriginSalesLine] AS link WITH (UPDLOCK, HOLDLOCK)
                    WHERE link.[DATAAREAID] = line.[DATAAREAID]
                        AND link.[InventTransOrigin] = origin.[RECID]
                        AND link.[SalesLineInventTransId] = line.[InventTransId]
                        AND link.[IsDeleted] = 0);
            """);
    }

    // These links describe existing inventory origins and should remain after a rollback.
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
