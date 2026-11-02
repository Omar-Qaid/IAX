using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventTransferJour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventTransferJour",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JournalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdateType = table.Column<int>(type: "int", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostingRequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventTransferJour", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferJour_DataAreaId_JournalId",
                table: "InventTransferJour",
                columns: new[] { "DataAreaId", "JournalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferJour_DataAreaId_TransferId_UpdateType_PostingRequestId",
                table: "InventTransferJour",
                columns: new[] { "DataAreaId", "TransferId", "UpdateType", "PostingRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferJour_DataAreaId_Voucher",
                table: "InventTransferJour",
                columns: new[] { "DataAreaId", "Voucher" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransferJour");
        }
    }
}
