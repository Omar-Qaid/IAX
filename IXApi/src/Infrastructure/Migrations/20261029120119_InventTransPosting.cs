using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventTransPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventTransPosting",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventTransPostingType = table.Column<int>(type: "int", nullable: false),
                    IsPosted = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    PostingType = table.Column<int>(type: "int", nullable: false),
                    PostingTypeOffset = table.Column<int>(type: "int", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransBeginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransBeginTimeTzId = table.Column<int>(type: "int", nullable: false),
                    ProjId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_InventTransPosting", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_InventTransPosting_InventTransOrigin_InventTransOrigin",
                        column: x => x.InventTransOrigin,
                        principalTable: "InventTransOrigin",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransPosting_DataAreaId_InventTransOrigin_InventTransPostingType",
                table: "InventTransPosting",
                columns: new[] { "DataAreaId", "InventTransOrigin", "InventTransPostingType" });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransPosting_DataAreaId_Voucher",
                table: "InventTransPosting",
                columns: new[] { "DataAreaId", "Voucher" });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransPosting_InventTransOrigin",
                table: "InventTransPosting",
                column: "InventTransOrigin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransPosting");
        }
    }
}
