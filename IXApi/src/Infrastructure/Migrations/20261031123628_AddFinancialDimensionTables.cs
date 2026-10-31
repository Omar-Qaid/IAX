using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialDimensionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DimensionAttribute",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    HashKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BackingEntityTableId = table.Column<int>(type: "int", nullable: false),
                    BackingEntityTableName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    BackingEntityKeyFieldId = table.Column<int>(type: "int", nullable: false),
                    BackingEntityKeyFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    BackingEntityValueFieldId = table.Column<int>(type: "int", nullable: false),
                    BackingEntityValueFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    BackingEntityType = table.Column<int>(type: "int", nullable: false),
                    BackingEntityPerCompanyType = table.Column<int>(type: "int", nullable: false),
                    ViewName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    DimensionKeyColumnName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    DimensionValueColumnName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    ReportColumnName = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    TranslationTableId = table.Column<int>(type: "int", nullable: false),
                    TranslationTableName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationKeyFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationKeyFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationNameFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationNameFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationLanguageIdFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationLanguageIdFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewKeyFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewKeyFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewValueFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewValueFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewNameFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewNameFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewLanguageIdFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewLanguageIdFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewSystemLanguageIdFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewSystemLanguageIdFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    TranslationViewTranslatedNameFieldId = table.Column<int>(type: "int", nullable: false),
                    TranslationViewTranslatedNameFieldName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    CopyValuesOnCreate = table.Column<int>(type: "int", nullable: false),
                    GiveDerivedDimensionsPrecedence = table.Column<int>(type: "int", nullable: false),
                    KeyAttribute = table.Column<int>(type: "int", nullable: false),
                    NameAttribute = table.Column<int>(type: "int", nullable: false),
                    ValueAttribute = table.Column<int>(type: "int", nullable: false),
                    UseTranslationNameMethod = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttribute", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DimensionHierarchy",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    StructureType = table.Column<int>(type: "int", nullable: false),
                    DraftName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DraftDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IsDraft = table.Column<int>(type: "int", nullable: false),
                    HashKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedVersion = table.Column<long>(type: "bigint", nullable: false),
                    IsSystemGenerated = table.Column<int>(type: "int", nullable: false),
                    FocusState = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionHierarchy", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeValue",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionAttribute = table.Column<long>(type: "bigint", nullable: false),
                    EntityInstance = table.Column<long>(type: "bigint", nullable: false),
                    OriginalEntityInstance = table.Column<long>(type: "bigint", nullable: false),
                    DisplayValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HashKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBlockedForManualEntry = table.Column<int>(type: "int", nullable: false),
                    IsSuspended = table.Column<int>(type: "int", nullable: false),
                    IsTotal = table.Column<int>(type: "int", nullable: false),
                    PendingSuccessfulDeleteValidation = table.Column<int>(type: "int", nullable: false),
                    BackingRecordDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    GroupDimension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Owner = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeValue", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeValue_DimensionAttribute_DimensionAttribute",
                        column: x => x.DimensionAttribute,
                        principalTable: "DimensionAttribute",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeValueGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionHierarchy = table.Column<long>(type: "bigint", nullable: false),
                    Levels = table.Column<int>(type: "int", nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    HashVersion = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeValueGroup", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeValueGroup_DimensionHierarchy_DimensionHierarchy",
                        column: x => x.DimensionHierarchy,
                        principalTable: "DimensionHierarchy",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeLevelValue",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionAttributeValueGroup = table.Column<long>(type: "bigint", nullable: false),
                    DimensionAttributeValue = table.Column<long>(type: "bigint", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    DisplayValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ModifiedTransactionId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeLevelValue", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeLevelValue_DimensionAttributeValueGroup_DimensionAttributeValueGroup",
                        column: x => x.DimensionAttributeValueGroup,
                        principalTable: "DimensionAttributeValueGroup",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeLevelValue_DimensionAttributeValue_DimensionAttributeValue",
                        column: x => x.DimensionAttributeValue,
                        principalTable: "DimensionAttributeValue",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeValueGroupCombination",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionAttributeValueCombination = table.Column<long>(type: "bigint", nullable: false),
                    DimensionAttributeValueGroup = table.Column<long>(type: "bigint", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    ModifiedTransactionId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeValueGroupCombination", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeValueGroupCombination_DimensionAttributeValueCombination_DimensionAttributeValueCombination",
                        column: x => x.DimensionAttributeValueCombination,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeValueGroupCombination_DimensionAttributeValueGroup_DimensionAttributeValueGroup",
                        column: x => x.DimensionAttributeValueGroup,
                        principalTable: "DimensionAttributeValueGroup",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueCombination_DataAreaId_DisplayValue",
                table: "DimensionAttributeValueCombination",
                columns: new[] { "DataAreaId", "DisplayValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttribute_DataAreaId_Name",
                table: "DimensionAttribute",
                columns: new[] { "DataAreaId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeLevelValue_DataAreaId_DimensionAttributeValueGroup_Ordinal",
                table: "DimensionAttributeLevelValue",
                columns: new[] { "DataAreaId", "DimensionAttributeValueGroup", "Ordinal" });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeLevelValue_DimensionAttributeValue",
                table: "DimensionAttributeLevelValue",
                column: "DimensionAttributeValue");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeLevelValue_DimensionAttributeValueGroup",
                table: "DimensionAttributeLevelValue",
                column: "DimensionAttributeValueGroup");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValue_DataAreaId_DimensionAttribute_DisplayValue",
                table: "DimensionAttributeValue",
                columns: new[] { "DataAreaId", "DimensionAttribute", "DisplayValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValue_DimensionAttribute",
                table: "DimensionAttributeValue",
                column: "DimensionAttribute");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueGroup_DataAreaId_DimensionHierarchy",
                table: "DimensionAttributeValueGroup",
                columns: new[] { "DataAreaId", "DimensionHierarchy" });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueGroup_DimensionHierarchy",
                table: "DimensionAttributeValueGroup",
                column: "DimensionHierarchy");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueGroupCombination_DataAreaId_DimensionAttributeValueCombination",
                table: "DimensionAttributeValueGroupCombination",
                columns: new[] { "DataAreaId", "DimensionAttributeValueCombination" });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueGroupCombination_DimensionAttributeValueCombination",
                table: "DimensionAttributeValueGroupCombination",
                column: "DimensionAttributeValueCombination");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueGroupCombination_DimensionAttributeValueGroup",
                table: "DimensionAttributeValueGroupCombination",
                column: "DimensionAttributeValueGroup");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionHierarchy_DataAreaId_Name",
                table: "DimensionHierarchy",
                columns: new[] { "DataAreaId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DimensionAttributeLevelValue");

            migrationBuilder.DropTable(
                name: "DimensionAttributeValueGroupCombination");

            migrationBuilder.DropTable(
                name: "DimensionAttributeValue");

            migrationBuilder.DropTable(
                name: "DimensionAttributeValueGroup");

            migrationBuilder.DropTable(
                name: "DimensionAttribute");

            migrationBuilder.DropTable(
                name: "DimensionHierarchy");

            migrationBuilder.DropIndex(
                name: "IX_DimensionAttributeValueCombination_DataAreaId_DisplayValue",
                table: "DimensionAttributeValueCombination");
        }
    }
}
