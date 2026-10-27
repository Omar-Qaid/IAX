using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubledgerVoucherLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubledgerVoucherGeneralJournalEntry",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneralJournalEntry = table.Column<long>(type: "bigint", nullable: false),
                    SubledgerJournalEntry = table.Column<long>(type: "bigint", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferId = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SubledgerVoucherGeneralJournalEntry", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SubledgerVoucherGeneralJournalEntry_GeneralJournalEntry_GeneralJournalEntry",
                        column: x => x.GeneralJournalEntry,
                        principalTable: "GeneralJournalEntry",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerVoucherGeneralJournalEntry_GeneralJournalEntry",
                table: "SubledgerVoucherGeneralJournalEntry",
                column: "GeneralJournalEntry");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerVoucherGeneralJournalEntry_VoucherDataAreaId_Voucher_AccountingDate",
                table: "SubledgerVoucherGeneralJournalEntry",
                columns: new[] { "VoucherDataAreaId", "Voucher", "AccountingDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubledgerVoucherGeneralJournalEntry");
        }
    }
}
