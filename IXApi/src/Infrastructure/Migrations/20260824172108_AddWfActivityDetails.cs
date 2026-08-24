using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWfActivityDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfActivityDetails",
                columns: table => new
                {
                    ActivityDetailID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskID = table.Column<long>(type: "bigint", nullable: false),
                    AssignmentID = table.Column<long>(type: "bigint", nullable: false),
                    ControlId = table.Column<byte>(type: "tinyint", nullable: false),
                    ControlDataId = table.Column<long>(type: "bigint", nullable: false),
                    ControlLabel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlLabelAR = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlValueAR = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlValueEN = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UsedAsCriteria = table.Column<bool>(type: "bit", nullable: false),
                    ControlOrder = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_WfActivityDetails", x => x.ActivityDetailID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfActivityDetails");
        }
    }
}
