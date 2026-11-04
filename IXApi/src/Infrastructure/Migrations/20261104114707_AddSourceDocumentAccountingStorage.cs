using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceDocumentAccountingStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SourceDocumentHeader",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingStatus = table.Column<int>(type: "int", nullable: true),
                    SourceRelationType = table.Column<int>(type: "int", nullable: false),
                    TypeEnumName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeEnumValue = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SourceDocumentHeader", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SubledgerJournalAccountEntryDistribution",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingCurrencyAmount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    AccountingDistribution = table.Column<long>(type: "bigint", nullable: true),
                    ParentDistribution = table.Column<long>(type: "bigint", nullable: true),
                    ReportingCurrencyAmount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    SubledgerJournalAccountEntry = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_SubledgerJournalAccountEntryDistribution", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntryDistribution_AccountingDistribution_AccountingDistribution",
                        column: x => x.AccountingDistribution,
                        principalTable: "AccountingDistribution",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntryDistribution_AccountingDistribution_ParentDistribution",
                        column: x => x.ParentDistribution,
                        principalTable: "AccountingDistribution",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntryDistribution_SubledgerJournalAccountEntry_SubledgerJournalAccountEntry",
                        column: x => x.SubledgerJournalAccountEntry,
                        principalTable: "SubledgerJournalAccountEntry",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingEvent",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: true),
                    State = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    IsAccountingRequired = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AccountingEvent", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_AccountingEvent_SourceDocumentHeader_SourceDocumentHeader",
                        column: x => x.SourceDocumentHeader,
                        principalTable: "SourceDocumentHeader",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SourceDocumentLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingStatus = table.Column<int>(type: "int", nullable: true),
                    ExchangeRateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFinal = table.Column<int>(type: "int", nullable: true),
                    ParentSourceDocumentLine = table.Column<long>(type: "bigint", nullable: true),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: false),
                    SourceRelationType = table.Column<int>(type: "int", nullable: false),
                    StateContainer = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TypeEnumName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeEnumValue = table.Column<int>(type: "int", nullable: false),
                    TermQuantity = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    DraftVersion = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SourceDocumentLine", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SourceDocumentLine_SourceDocumentHeader_SourceDocumentHeader",
                        column: x => x.SourceDocumentHeader,
                        principalTable: "SourceDocumentHeader",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SourceDocumentLine_SourceDocumentLine_ParentSourceDocumentLine",
                        column: x => x.ParentSourceDocumentLine,
                        principalTable: "SourceDocumentLine",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_AccountingEvent",
                table: "AccountingDistribution",
                column: "AccountingEvent");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_FinalizeAccountingEvent",
                table: "AccountingDistribution",
                column: "FinalizeAccountingEvent");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingEvent_AccountingDate_Type_SourceDocumentHeader",
                table: "AccountingEvent",
                columns: new[] { "AccountingDate", "Type", "SourceDocumentHeader" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingEvent_SourceDocumentHeader",
                table: "AccountingEvent",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_SourceDocumentHeader_SourceRelationType_TypeEnumValue",
                table: "SourceDocumentHeader",
                columns: new[] { "SourceRelationType", "TypeEnumValue" });

            migrationBuilder.CreateIndex(
                name: "IX_SourceDocumentLine_ParentSourceDocumentLine",
                table: "SourceDocumentLine",
                column: "ParentSourceDocumentLine");

            migrationBuilder.CreateIndex(
                name: "IX_SourceDocumentLine_SourceDocumentHeader",
                table: "SourceDocumentLine",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntryDistribution_AccountingDistribution",
                table: "SubledgerJournalAccountEntryDistribution",
                column: "AccountingDistribution");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntryDistribution_ParentDistribution",
                table: "SubledgerJournalAccountEntryDistribution",
                column: "ParentDistribution");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntryDistribution_SubledgerJournalAccountEntry",
                table: "SubledgerJournalAccountEntryDistribution",
                column: "SubledgerJournalAccountEntry");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingDistribution_AccountingEvent_AccountingEvent",
                table: "AccountingDistribution",
                column: "AccountingEvent",
                principalTable: "AccountingEvent",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingDistribution_AccountingEvent_FinalizeAccountingEvent",
                table: "AccountingDistribution",
                column: "FinalizeAccountingEvent",
                principalTable: "AccountingEvent",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingDistribution_SourceDocumentHeader_SourceDocumentHeader",
                table: "AccountingDistribution",
                column: "SourceDocumentHeader",
                principalTable: "SourceDocumentHeader",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingDistribution_SourceDocumentLine_SourceDocumentLine",
                table: "AccountingDistribution",
                column: "SourceDocumentLine",
                principalTable: "SourceDocumentLine",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubledgerJournalEntry_AccountingEvent_AccountingEvent",
                table: "SubledgerJournalEntry",
                column: "AccountingEvent",
                principalTable: "AccountingEvent",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingDistribution_AccountingEvent_AccountingEvent",
                table: "AccountingDistribution");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingDistribution_AccountingEvent_FinalizeAccountingEvent",
                table: "AccountingDistribution");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingDistribution_SourceDocumentHeader_SourceDocumentHeader",
                table: "AccountingDistribution");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingDistribution_SourceDocumentLine_SourceDocumentLine",
                table: "AccountingDistribution");

            migrationBuilder.DropForeignKey(
                name: "FK_SubledgerJournalEntry_AccountingEvent_AccountingEvent",
                table: "SubledgerJournalEntry");

            migrationBuilder.DropTable(
                name: "AccountingEvent");

            migrationBuilder.DropTable(
                name: "SourceDocumentLine");

            migrationBuilder.DropTable(
                name: "SubledgerJournalAccountEntryDistribution");

            migrationBuilder.DropTable(
                name: "SourceDocumentHeader");

            migrationBuilder.DropIndex(
                name: "IX_AccountingDistribution_AccountingEvent",
                table: "AccountingDistribution");

            migrationBuilder.DropIndex(
                name: "IX_AccountingDistribution_FinalizeAccountingEvent",
                table: "AccountingDistribution");
        }
    }
}
