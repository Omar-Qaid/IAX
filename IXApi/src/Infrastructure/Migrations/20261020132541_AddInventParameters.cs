using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventParameters",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultUnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MultiSiteDefaultInventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MultiSiteDefaultInventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MovementJournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LossProfitJournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransferJournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountJournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReserveOnOrdered = table.Column<int>(type: "int", nullable: false),
                    MandatoryItemTaxGroup = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventParameters", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventParameters_DataAreaId",
                table: "InventParameters",
                column: "DataAreaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventParameters");
        }
    }
}
