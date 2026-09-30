using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addinveinttableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HcmWorker_HcmOccupations_OccupationId",
                table: "HcmWorker");

            migrationBuilder.DropIndex(
                name: "IX_HcmWorker_OccupationId",
                table: "HcmWorker");

            migrationBuilder.DropColumn(
                name: "OccupationId",
                table: "HcmWorker");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "InventTable",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "InventTable");

            migrationBuilder.AddColumn<short>(
                name: "OccupationId",
                table: "HcmWorker",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_OccupationId",
                table: "HcmWorker",
                column: "OccupationId");

            migrationBuilder.AddForeignKey(
                name: "FK_HcmWorker_HcmOccupations_OccupationId",
                table: "HcmWorker",
                column: "OccupationId",
                principalTable: "HcmOccupations",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
