using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFreeTextInvoiceCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_InventTransId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_InvoiceId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_ItemId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_SalesId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTable_InvoiceDate",
                table: "CustInvoiceTable");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTable_InvoiceId",
                table: "CustInvoiceTable");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTable_OrderAccount",
                table: "CustInvoiceTable");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceLine_ParentRecId",
                table: "CustInvoiceLine");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_InvoiceAccount",
                table: "CustInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_InvoiceId",
                table: "CustInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_OrderAccount",
                table: "CustInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_SalesId",
                table: "CustInvoiceJour");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_InventTransId",
                table: "CustInvoiceTrans",
                columns: new[] { "DataAreaId", "InventTransId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_InvoiceId",
                table: "CustInvoiceTrans",
                columns: new[] { "DataAreaId", "InvoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_ItemId",
                table: "CustInvoiceTrans",
                columns: new[] { "DataAreaId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_SalesId",
                table: "CustInvoiceTrans",
                columns: new[] { "DataAreaId", "SalesId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_DataAreaId_InvoiceDate",
                table: "CustInvoiceTable",
                columns: new[] { "DataAreaId", "InvoiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_DataAreaId_InvoiceId",
                table: "CustInvoiceTable",
                columns: new[] { "DataAreaId", "InvoiceId" },
                unique: true,
                filter: "[InvoiceId] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_DataAreaId_OrderAccount",
                table: "CustInvoiceTable",
                columns: new[] { "DataAreaId", "OrderAccount" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceLine_DataAreaId_ParentRecId_LineNum",
                table: "CustInvoiceLine",
                columns: new[] { "DataAreaId", "ParentRecId", "LineNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_DataAreaId_InvoiceAccount",
                table: "CustInvoiceJour",
                columns: new[] { "DataAreaId", "InvoiceAccount" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_DataAreaId_InvoiceId",
                table: "CustInvoiceJour",
                columns: new[] { "DataAreaId", "InvoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_DataAreaId_OrderAccount",
                table: "CustInvoiceJour",
                columns: new[] { "DataAreaId", "OrderAccount" });

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_DataAreaId_SalesId",
                table: "CustInvoiceJour",
                columns: new[] { "DataAreaId", "SalesId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_InventTransId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_InvoiceId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_ItemId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTrans_DataAreaId_SalesId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTable_DataAreaId_InvoiceDate",
                table: "CustInvoiceTable");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTable_DataAreaId_InvoiceId",
                table: "CustInvoiceTable");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceTable_DataAreaId_OrderAccount",
                table: "CustInvoiceTable");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceLine_DataAreaId_ParentRecId_LineNum",
                table: "CustInvoiceLine");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_DataAreaId_InvoiceAccount",
                table: "CustInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_DataAreaId_InvoiceId",
                table: "CustInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_DataAreaId_OrderAccount",
                table: "CustInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_CustInvoiceJour_DataAreaId_SalesId",
                table: "CustInvoiceJour");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_InventTransId",
                table: "CustInvoiceTrans",
                column: "InventTransId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_InvoiceId",
                table: "CustInvoiceTrans",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_ItemId",
                table: "CustInvoiceTrans",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_SalesId",
                table: "CustInvoiceTrans",
                column: "SalesId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_InvoiceDate",
                table: "CustInvoiceTable",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_InvoiceId",
                table: "CustInvoiceTable",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_OrderAccount",
                table: "CustInvoiceTable",
                column: "OrderAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceLine_ParentRecId",
                table: "CustInvoiceLine",
                column: "ParentRecId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_InvoiceAccount",
                table: "CustInvoiceJour",
                column: "InvoiceAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_InvoiceId",
                table: "CustInvoiceJour",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_OrderAccount",
                table: "CustInvoiceJour",
                column: "OrderAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_SalesId",
                table: "CustInvoiceJour",
                column: "SalesId");
        }
    }
}
