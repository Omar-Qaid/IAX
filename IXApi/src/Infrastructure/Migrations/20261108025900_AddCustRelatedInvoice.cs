using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261108025900_AddCustRelatedInvoice")]
public sealed class AddCustRelatedInvoice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CustRelatedInvoice",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CustInvoice = table.Column<long>(type: "bigint", nullable: true),
                InvoiceAssociationType = table.Column<int>(type: "int", nullable: true),
                OriginalCustInvoice = table.Column<long>(type: "bigint", nullable: true),
                ParentCustInvoice = table.Column<long>(type: "bigint", nullable: true),
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
                table.PrimaryKey("PK_CustRelatedInvoice", x => x.RECID);
                table.ForeignKey("FK_CustRelatedInvoice_CustInvoiceTable_CustInvoice",
                    x => x.CustInvoice, "CustInvoiceTable", "RECID", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CustRelatedInvoice_CustInvoiceTable_OriginalCustInvoice",
                    x => x.OriginalCustInvoice, "CustInvoiceTable", "RECID", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CustRelatedInvoice_CustInvoiceTable_ParentCustInvoice",
                    x => x.ParentCustInvoice, "CustInvoiceTable", "RECID", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_CustRelatedInvoice_CustInvoice", "CustRelatedInvoice",
            "CustInvoice");
        migrationBuilder.CreateIndex("IX_CustRelatedInvoice_OriginalCustInvoice", "CustRelatedInvoice",
            "OriginalCustInvoice");
        migrationBuilder.CreateIndex("IX_CustRelatedInvoice_ParentCustInvoice", "CustRelatedInvoice",
            "ParentCustInvoice");
        migrationBuilder.CreateIndex("IX_CustRelatedInvoice_DataAreaId_CustInvoice", "CustRelatedInvoice",
            new[] { "DataAreaId", "CustInvoice" });
        migrationBuilder.CreateIndex("IX_CustRelatedInvoice_DataAreaId_ParentCustInvoice_InvoiceAssociationType", "CustRelatedInvoice",
            new[] { "DataAreaId", "ParentCustInvoice", "InvoiceAssociationType" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "CustRelatedInvoice");
}
