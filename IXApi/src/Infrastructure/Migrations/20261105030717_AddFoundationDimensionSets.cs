using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFoundationDimensionSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DimensionAttributeSet",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseEnumTypeName = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    BaseEnumType = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeSet", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeValueSet",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    HashVersion = table.Column<int>(type: "int", nullable: false),
                    MainAccount = table.Column<long>(type: "bigint", nullable: false),
                    MainAccountValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Account = table.Column<long>(type: "bigint", nullable: false),
                    AccountValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BusinessUnit = table.Column<long>(type: "bigint", nullable: false),
                    BusinessUnitValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CostCenter = table.Column<long>(type: "bigint", nullable: false),
                    CostCenterValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Department = table.Column<long>(type: "bigint", nullable: false),
                    DepartmentValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Division = table.Column<long>(type: "bigint", nullable: false),
                    DivisionValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Project = table.Column<long>(type: "bigint", nullable: false),
                    ProjectValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceLine = table.Column<long>(type: "bigint", nullable: false),
                    ServiceLineValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Fund = table.Column<long>(type: "bigint", nullable: false),
                    FundValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Program = table.Column<long>(type: "bigint", nullable: false),
                    ProgramValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ItemGroup = table.Column<long>(type: "bigint", nullable: false),
                    ItemGroupValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductGroup = table.Column<long>(type: "bigint", nullable: false),
                    ProductGroupValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Agreement = table.Column<long>(type: "bigint", nullable: false),
                    AgreementValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Terminal = table.Column<long>(type: "bigint", nullable: false),
                    TerminalValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Worker = table.Column<long>(type: "bigint", nullable: false),
                    WorkerValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Groups = table.Column<long>(type: "bigint", nullable: false),
                    GroupsValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpenseAndIncomeCode = table.Column<long>(type: "bigint", nullable: false),
                    ExpenseAndIncomeCodeValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ObjectClass = table.Column<long>(type: "bigint", nullable: false),
                    ObjectClassValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Filial = table.Column<long>(type: "bigint", nullable: false),
                    FilialValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FiscalEstablishment = table.Column<long>(type: "bigint", nullable: false),
                    FiscalEstablishmentValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TaxBranch = table.Column<long>(type: "bigint", nullable: false),
                    TaxBranchValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CostCenter_CnValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Department_CnValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CashFlow_CnValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BankAccount = table.Column<long>(type: "bigint", nullable: false),
                    Campaign = table.Column<long>(type: "bigint", nullable: false),
                    Cargo = table.Column<long>(type: "bigint", nullable: false),
                    Center = table.Column<long>(type: "bigint", nullable: false),
                    Condition = table.Column<long>(type: "bigint", nullable: false),
                    Contract = table.Column<long>(type: "bigint", nullable: false),
                    Customer = table.Column<long>(type: "bigint", nullable: false),
                    JobSkills = table.Column<long>(type: "bigint", nullable: false),
                    LaborType = table.Column<long>(type: "bigint", nullable: false),
                    LegalEntity = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: false),
                    Primary_ = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<long>(type: "bigint", nullable: false),
                    Store = table.Column<long>(type: "bigint", nullable: false),
                    Vehicle = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeBankAccount = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeCustomer = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeEmployee = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeFixedAsset = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeItem = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeProject = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeRCash = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeRDeferrals = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeVendor = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeValueSet", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeSetItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionAttributeSet = table.Column<long>(type: "bigint", nullable: false),
                    DimensionAttribute = table.Column<long>(type: "bigint", nullable: false),
                    EnumerationValue = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeSetItem", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeSetItem_DimensionAttributeSet_DimensionAttributeSet",
                        column: x => x.DimensionAttributeSet,
                        principalTable: "DimensionAttributeSet",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeSetItem_DimensionAttribute_DimensionAttribute",
                        column: x => x.DimensionAttribute,
                        principalTable: "DimensionAttribute",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeValueSetItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionAttributeValueSet = table.Column<long>(type: "bigint", nullable: false),
                    DimensionAttributeValue = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeValueSetItem", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeValueSetItem_DimensionAttributeValueSet_DimensionAttributeValueSet",
                        column: x => x.DimensionAttributeValueSet,
                        principalTable: "DimensionAttributeValueSet",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DimensionAttributeValueSetItem_DimensionAttributeValue_DimensionAttributeValue",
                        column: x => x.DimensionAttributeValue,
                        principalTable: "DimensionAttributeValue",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeSetItem_DataAreaId_DimensionAttributeSet_DimensionAttribute",
                table: "DimensionAttributeSetItem",
                columns: new[] { "DataAreaId", "DimensionAttributeSet", "DimensionAttribute" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeSetItem_DimensionAttribute",
                table: "DimensionAttributeSetItem",
                column: "DimensionAttribute");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeSetItem_DimensionAttributeSet",
                table: "DimensionAttributeSetItem",
                column: "DimensionAttributeSet");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueSetItem_DataAreaId_DimensionAttributeValueSet_DimensionAttributeValue",
                table: "DimensionAttributeValueSetItem",
                columns: new[] { "DataAreaId", "DimensionAttributeValueSet", "DimensionAttributeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueSetItem_DimensionAttributeValue",
                table: "DimensionAttributeValueSetItem",
                column: "DimensionAttributeValue");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionAttributeValueSetItem_DimensionAttributeValueSet",
                table: "DimensionAttributeValueSetItem",
                column: "DimensionAttributeValueSet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DimensionAttributeSetItem");

            migrationBuilder.DropTable(
                name: "DimensionAttributeValueSetItem");

            migrationBuilder.DropTable(
                name: "DimensionAttributeSet");

            migrationBuilder.DropTable(
                name: "DimensionAttributeValueSet");
        }
    }
}
