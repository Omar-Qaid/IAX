using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMainAccountStructureSegment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "DimensionAttribute",
                table: "DimensionHierarchyLevel",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            // Normalize only the historical main-account sentinel; preserve all real references.
            migrationBuilder.Sql("UPDATE [DimensionHierarchyLevel] SET [DimensionAttribute] = NULL WHERE [DimensionAttribute] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [DimensionHierarchyLevel] WHERE [DimensionAttribute] IS NULL) THROW 51000, 'Cannot restore a required dimension FK while main-account segments exist. Existing data was preserved.', 1;");
            migrationBuilder.AlterColumn<long>(
                name: "DimensionAttribute",
                table: "DimensionHierarchyLevel",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
