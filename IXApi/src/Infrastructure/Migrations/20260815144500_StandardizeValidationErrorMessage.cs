using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260815144500_StandardizeValidationErrorMessage")]
    public partial class StandardizeValidationErrorMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddUnifiedMessage(migrationBuilder, "WfRequestControlsValidations");
            AddUnifiedMessage(migrationBuilder, "WfActivityControlsValidations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RestoreLocalizedMessages(migrationBuilder, "WfRequestControlsValidations");
            RestoreLocalizedMessages(migrationBuilder, "WfActivityControlsValidations");
        }

        private static void AddUnifiedMessage(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.AddColumn<string>(name: "ErrorMessage", table: table, type: "nvarchar(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.Sql($"UPDATE [{table}] SET [ErrorMessage] = COALESCE(NULLIF([ErrorMessageEn], N''), NULLIF([ErrorMessageAr], N''), N'Validation failed.')");
            migrationBuilder.AlterColumn<string>(name: "ErrorMessage", table: table, type: "nvarchar(1000)", maxLength: 1000, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(1000)", oldMaxLength: 1000, oldNullable: true);
            migrationBuilder.DropColumn(name: "ErrorMessageAr", table: table);
            migrationBuilder.DropColumn(name: "ErrorMessageEn", table: table);
        }

        private static void RestoreLocalizedMessages(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.AddColumn<string>(name: "ErrorMessageAr", table: table, type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "ErrorMessageEn", table: table, type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: "");
            migrationBuilder.Sql($"UPDATE [{table}] SET [ErrorMessageAr] = [ErrorMessage], [ErrorMessageEn] = [ErrorMessage]");
            migrationBuilder.DropColumn(name: "ErrorMessage", table: table);
        }
    }
}
