using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261108221600_AddVendPackingSlipTransHistory")]
public sealed class AddVendPackingSlipTransHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VendPackingSlipTransHistory",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VendPackingSlipTrans = table.Column<long>(type: "bigint", nullable: false),
                DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                InventQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                Remain = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                RemainInvent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                ValueMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                table.PrimaryKey("PK_VendPackingSlipTransHistory", x => x.RECID);
                table.ForeignKey(
                    name: "FK_VendPackingSlipTransHistory_VendPackingSlipTrans_VendPackingSlipTrans",
                    column: x => x.VendPackingSlipTrans,
                    principalTable: "VendPackingSlipTrans",
                    principalColumn: "RECID",
                    onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(
            name: "IX_VendPackingSlipTransHistory_VendPackingSlipTrans_ValidFrom",
            table: "VendPackingSlipTransHistory",
            columns: new[] { "VendPackingSlipTrans", "ValidFrom" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "VendPackingSlipTransHistory");
}
