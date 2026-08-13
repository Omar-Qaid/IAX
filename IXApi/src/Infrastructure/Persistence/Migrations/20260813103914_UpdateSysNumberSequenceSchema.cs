using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSysNumberSequenceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM SysNumberSequences;");

            migrationBuilder.DropIndex(
                name: "IX_SysNumberSequences_Code",
                table: "SysNumberSequences");

            migrationBuilder.DropIndex(
                name: "IX_SysNumberSequences_EntityName_TenantId",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "LargestValue",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "NameAR",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "NextValue",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "PaddingLength",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "ResetCycle",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "SmallestValue",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Step",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SysNumberSequences");

            migrationBuilder.RenameColumn(
                name: "LastResetAt",
                table: "SysNumberSequences",
                newName: "LatestCleanDateTime");

            migrationBuilder.RenameColumn(
                name: "FormatPattern",
                table: "SysNumberSequences",
                newName: "Txt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysNumberSequences",
                table: "SysNumberSequences");

            migrationBuilder.AlterColumn<long>(
                name: "RECID",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysNumberSequences",
                table: "SysNumberSequences",
                column: "RECID");

            migrationBuilder.AddColumn<int>(
                name: "AllowChangeDown",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AllowChangeUp",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnotatedFormat",
                table: "SysNumberSequences",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Blocked",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CleanAtAccess",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CleanInterval",
                table: "SysNumberSequences",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Continuous",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cyclic",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FetchAhead",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FetchAheadQty",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "SysNumberSequences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Highest",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InUse",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LatestCleanDateTimeTzId",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Lowest",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Manual",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedTransactionId",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextRec",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoIncrement",
                table: "SysNumberSequences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberSequence",
                table: "SysNumberSequences",
                type: "nvarchar(22)",
                maxLength: 22,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "NumberSequenceScope",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Partition",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysNumberSequences_NumberSequence",
                table: "SysNumberSequences",
                column: "NumberSequence",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysNumberSequences_NumberSequence",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "AllowChangeDown",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "AllowChangeUp",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "AnnotatedFormat",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Blocked",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "CleanAtAccess",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "CleanInterval",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Continuous",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Cyclic",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "FetchAhead",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "FetchAheadQty",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Highest",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "InUse",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "LatestCleanDateTimeTzId",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Lowest",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Manual",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "ModifiedTransactionId",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "NextRec",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "NoIncrement",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "NumberSequence",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "NumberSequenceScope",
                table: "SysNumberSequences");

            migrationBuilder.DropColumn(
                name: "Partition",
                table: "SysNumberSequences");

            migrationBuilder.RenameColumn(
                name: "Txt",
                table: "SysNumberSequences",
                newName: "FormatPattern");

            migrationBuilder.RenameColumn(
                name: "LatestCleanDateTime",
                table: "SysNumberSequences",
                newName: "LastResetAt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysNumberSequences",
                table: "SysNumberSequences");

            migrationBuilder.AlterColumn<int>(
                name: "RECID",
                table: "SysNumberSequences",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysNumberSequences",
                table: "SysNumberSequences",
                column: "RECID");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SysNumberSequences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "SysNumberSequences",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "LargestValue",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SysNumberSequences",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAR",
                table: "SysNumberSequences",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "NextValue",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "PaddingLength",
                table: "SysNumberSequences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "SysNumberSequences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ResetCycle",
                table: "SysNumberSequences",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<long>(
                name: "SmallestValue",
                table: "SysNumberSequences",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Step",
                table: "SysNumberSequences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "SysNumberSequences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "SysNumberSequences",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysNumberSequences_Code",
                table: "SysNumberSequences",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysNumberSequences_EntityName_TenantId",
                table: "SysNumberSequences",
                columns: new[] { "EntityName", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }
    }
}
