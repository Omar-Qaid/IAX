using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementP1GeneralLedgerControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LedgerPeriodModuleAccessControl",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LedgerFiscalCalendarPeriod = table.Column<long>(type: "bigint", nullable: false),
                    LedgerAccessLevel = table.Column<int>(type: "int", nullable: false),
                    LedgerUserGroupInfo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TaxAccessLevel = table.Column<int>(type: "int", nullable: false),
                    TaxUserGroupInfo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BankAccessLevel = table.Column<int>(type: "int", nullable: false),
                    BankUserGroupInfo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CustAccessLevel = table.Column<int>(type: "int", nullable: false),
                    CustUserGroupInfo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    VendAccessLevel = table.Column<int>(type: "int", nullable: false),
                    VendUserGroupInfo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("PK_LedgerPeriodModuleAccessControl", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_LedgerPeriodModuleAccessControl_LedgerFiscalCalendarPeriod_LedgerFiscalCalendarPeriod",
                        column: x => x.LedgerFiscalCalendarPeriod,
                        principalTable: "LedgerFiscalCalendarPeriod",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerJournalTrans_DataAreaId_JournalNum_LineNum",
                table: "LedgerJournalTrans",
                columns: new[] { "DataAreaId", "JournalNum", "LineNum" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerJournalTable_DataAreaId_JournalNum",
                table: "LedgerJournalTable",
                columns: new[] { "DataAreaId", "JournalNum" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerJournalName_DataAreaId_JournalName",
                table: "LedgerJournalName",
                columns: new[] { "DataAreaId", "JournalName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Ledger_DataAreaId",
                table: "Ledger",
                column: "DataAreaId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerPeriodModuleAccessControl_DataAreaId_LedgerFiscalCalendarPeriod",
                table: "LedgerPeriodModuleAccessControl",
                columns: new[] { "DataAreaId", "LedgerFiscalCalendarPeriod" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerPeriodModuleAccessControl_LedgerFiscalCalendarPeriod",
                table: "LedgerPeriodModuleAccessControl",
                column: "LedgerFiscalCalendarPeriod",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerPeriodModuleAccessControl");

            migrationBuilder.DropIndex(
                name: "IX_LedgerJournalTrans_DataAreaId_JournalNum_LineNum",
                table: "LedgerJournalTrans");

            migrationBuilder.DropIndex(
                name: "IX_LedgerJournalTable_DataAreaId_JournalNum",
                table: "LedgerJournalTable");

            migrationBuilder.DropIndex(
                name: "IX_LedgerJournalName_DataAreaId_JournalName",
                table: "LedgerJournalName");

            migrationBuilder.DropIndex(
                name: "IX_Ledger_DataAreaId",
                table: "Ledger");
        }
    }
}
