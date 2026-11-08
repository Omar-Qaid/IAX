using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261108135900_AddCustExchRateAdjustment")]
public sealed class AddCustExchRateAdjustment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.CreateTable(
            name: "CustExchRateAdjustment",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                ConsideredDate = table.Column<DateTime>(type: "date", nullable: true),
                DateOfRate = table.Column<DateTime>(type: "date", nullable: true),
                Dimensions = table.Column<int>(type: "int", nullable: true),
                ExchPrinciple = table.Column<int>(type: "int", nullable: true),
                ExecutedDate = table.Column<DateTime>(type: "date", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                Reviewed = table.Column<int>(type: "int", nullable: true),
                UseProfile = table.Column<int>(type: "int", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustExchRateAdjustment", x => x.RECID));

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "CustExchRateAdjustment");
}
