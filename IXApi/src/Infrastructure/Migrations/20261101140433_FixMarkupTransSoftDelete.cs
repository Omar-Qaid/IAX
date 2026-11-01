using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMarkupTransSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MarkupTrans.IsDeleted shadowed the base entity's bool with a NoYes, so the column was
            // mapped as int and the global soft-delete filter — which only applies to a bool — never
            // covered this table. Removing the shadow changes the column to bit.
            //
            // The scaffolder's data-loss warning is benign here: the column only ever held NoYes,
            // whose members are No = 0 and Yes = 1, and SQL Server converts those to bit exactly.
            // The normalisation below makes that explicit rather than leaving it to be inferred,
            // and guards the conversion if any other value ever reached the column.
            migrationBuilder.Sql(
                "UPDATE [MarkupTrans] SET [IsDeleted] = 1 WHERE [IsDeleted] <> 0;");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MarkupTrans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "MarkupTrans",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
