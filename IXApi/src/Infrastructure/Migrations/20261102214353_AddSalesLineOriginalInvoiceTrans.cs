using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesLineOriginalInvoiceTrans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OriginalInvoiceTransRecId",
                table: "SalesLine",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesLine_OriginalInvoiceTransRecId",
                table: "SalesLine",
                column: "OriginalInvoiceTransRecId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesLine_CustInvoiceTrans_OriginalInvoiceTransRecId",
                table: "SalesLine",
                column: "OriginalInvoiceTransRecId",
                principalTable: "CustInvoiceTrans",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesLine_CustInvoiceTrans_OriginalInvoiceTransRecId",
                table: "SalesLine");

            migrationBuilder.DropIndex(
                name: "IX_SalesLine_OriginalInvoiceTransRecId",
                table: "SalesLine");

            migrationBuilder.DropColumn(
                name: "OriginalInvoiceTransRecId",
                table: "SalesLine");
        }
    }
}
