using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261107160000_AddCustPackingSlipVersion")]
public sealed class AddCustPackingSlipVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CustPackingSlipVersion",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustPackingSlipJour = table.Column<long>(type: "bigint", nullable: false),
                InternalPackingSlipId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LedgerVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                VersionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
            constraints: table =>
            {
                table.PrimaryKey("PK_CustPackingSlipVersion", x => x.RECID);
                table.ForeignKey(
                    name: "FK_CustPackingSlipVersion_CustPackingSlipJour_CustPackingSlipJour",
                    column: x => x.CustPackingSlipJour,
                    principalTable: "CustPackingSlipJour",
                    principalColumn: "RECID",
                    onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(
            name: "IX_CustPackingSlipVersion_CustPackingSlipJour_InternalPackingSlipId",
            table: "CustPackingSlipVersion",
            columns: new[] { "CustPackingSlipJour", "InternalPackingSlipId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "CustPackingSlipVersion");
}
