using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsPayableDocumentIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InventTransId",
                table: "VendPackingSlipTrans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_VendTrans_AccountNum",
                table: "VendTrans",
                column: "AccountNum");

            migrationBuilder.CreateIndex(
                name: "IX_VendTrans_DataAreaId_AccountNum",
                table: "VendTrans",
                columns: new[] { "DataAreaId", "AccountNum" });

            migrationBuilder.CreateIndex(
                name: "IX_VendTrans_DataAreaId_Invoice",
                table: "VendTrans",
                columns: new[] { "DataAreaId", "Invoice" });

            migrationBuilder.CreateIndex(
                name: "IX_VendTrans_DataAreaId_Voucher",
                table: "VendTrans",
                columns: new[] { "DataAreaId", "Voucher" });

            migrationBuilder.CreateIndex(
                name: "IX_VendPurchOrderTrans_DataAreaId_PurchId",
                table: "VendPurchOrderTrans",
                columns: new[] { "DataAreaId", "PurchId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendPurchOrderJour_DataAreaId_PurchId",
                table: "VendPurchOrderJour",
                columns: new[] { "DataAreaId", "PurchId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendPackingSlipTrans_DataAreaId_InventTransId",
                table: "VendPackingSlipTrans",
                columns: new[] { "DataAreaId", "InventTransId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendPackingSlipTrans_DataAreaId_OrigPurchId",
                table: "VendPackingSlipTrans",
                columns: new[] { "DataAreaId", "OrigPurchId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendPackingSlipJour_DataAreaId_InternalPackingSlipId",
                table: "VendPackingSlipJour",
                columns: new[] { "DataAreaId", "InternalPackingSlipId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendPackingSlipJour_DataAreaId_PurchId",
                table: "VendPackingSlipJour",
                columns: new[] { "DataAreaId", "PurchId" });

            migrationBuilder.AddForeignKey(
                name: "FK_VendTrans_VendTable_AccountNum",
                table: "VendTrans",
                column: "AccountNum",
                principalTable: "VendTable",
                principalColumn: "AccountNum",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendTrans_VendTable_AccountNum",
                table: "VendTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendTrans_AccountNum",
                table: "VendTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendTrans_DataAreaId_AccountNum",
                table: "VendTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendTrans_DataAreaId_Invoice",
                table: "VendTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendTrans_DataAreaId_Voucher",
                table: "VendTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendPurchOrderTrans_DataAreaId_PurchId",
                table: "VendPurchOrderTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendPurchOrderJour_DataAreaId_PurchId",
                table: "VendPurchOrderJour");

            migrationBuilder.DropIndex(
                name: "IX_VendPackingSlipTrans_DataAreaId_InventTransId",
                table: "VendPackingSlipTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendPackingSlipTrans_DataAreaId_OrigPurchId",
                table: "VendPackingSlipTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendPackingSlipJour_DataAreaId_InternalPackingSlipId",
                table: "VendPackingSlipJour");

            migrationBuilder.DropIndex(
                name: "IX_VendPackingSlipJour_DataAreaId_PurchId",
                table: "VendPackingSlipJour");

            migrationBuilder.AlterColumn<string>(
                name: "InventTransId",
                table: "VendPackingSlipTrans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
