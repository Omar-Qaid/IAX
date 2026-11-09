using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261108221500_AddVendPackingSlipVersion")]
public sealed class AddVendPackingSlipVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VendPackingSlipVersion",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VendPackingSlipJour = table.Column<long>(type: "bigint", nullable: false),
                InternalPackingSlipId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LedgerVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                VersionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                table.PrimaryKey("PK_VendPackingSlipVersion", x => x.RECID);
                table.ForeignKey(
                    name: "FK_VendPackingSlipVersion_VendPackingSlipJour_VendPackingSlipJour",
                    column: x => x.VendPackingSlipJour,
                    principalTable: "VendPackingSlipJour",
                    principalColumn: "RECID",
                    onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(
            name: "IX_VendPackingSlipVersion_VendPackingSlipJour_InternalPackingSlipId",
            table: "VendPackingSlipVersion",
            columns: new[] { "VendPackingSlipJour", "InternalPackingSlipId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "VendPackingSlipVersion");
}
