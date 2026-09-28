using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandWorkflowRequestAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ControlValue", table: "WfRequestDetails", type: "nvarchar(max)", nullable: true,
                oldClrType: typeof(string), oldType: "nvarchar(255)", oldMaxLength: 255, oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not silently truncate answers when rolling back this schema change.
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [WfRequestDetails] WHERE DATALENGTH([ControlValue]) > 510) THROW 51000, 'Cannot shorten request answers while values exceed 255 characters.', 1;");
            migrationBuilder.AlterColumn<string>(
                name: "ControlValue", table: "WfRequestDetails", type: "nvarchar(255)", maxLength: 255, nullable: true,
                oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true);
        }
    }
}
