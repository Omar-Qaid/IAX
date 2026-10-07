using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adddiscprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InventDimId",
                table: "InventDim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_InventDim_InventDimId",
                table: "InventDim",
                column: "InventDimId");

            migrationBuilder.CreateTable(
                name: "CustParameters",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<int>(type: "int", nullable: false),
                    CustPostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    CreditLimitCheck = table.Column<int>(type: "int", nullable: false),
                    PaymTermId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvReasonId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PriceDiscSearchPrice = table.Column<int>(type: "int", nullable: false),
                    PriceDiscSearchLineDisc = table.Column<int>(type: "int", nullable: false),
                    PriceDiscSearchMultilineDisc = table.Column<int>(type: "int", nullable: false),
                    PriceDiscSearchTotalDisc = table.Column<int>(type: "int", nullable: false),
                    PriceDiscMandatory = table.Column<int>(type: "int", nullable: false),
                    PriceDiscJournalNamePrice = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceDiscJournalNameLineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceDiscJournalNameMultilineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceDiscJournalNameTotalDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesOrderType = table.Column<int>(type: "int", nullable: false),
                    Reservation = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InvoiceJournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PackingSlipJournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustNumSeqGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Partition = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustParameters", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventSerial",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventSerialId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_InventSerial", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "PriceDiscAdmName",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefaultRelation = table.Column<int>(type: "int", nullable: false),
                    PriceDiscPriceAttributeEnable = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceDiscAdmName", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "PriceDiscAdmTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefaultRelation = table.Column<int>(type: "int", nullable: false),
                    Posted = table.Column<int>(type: "int", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExportCurrentPrice = table.Column<int>(type: "int", nullable: false),
                    LockedForDeletion = table.Column<int>(type: "int", nullable: false),
                    PriceGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceComponentCombination = table.Column<long>(type: "bigint", nullable: false),
                    PriceApplyAdjustment = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceDiscAdmTable", x => x.RECID);
                    table.UniqueConstraint("AK_PriceDiscAdmTable_JournalNum", x => x.JournalNum);
                });

            migrationBuilder.CreateTable(
                name: "PriceDiscGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Module = table.Column<int>(type: "int", nullable: false),
                    McrPriceDiscGroupType = table.Column<int>(type: "int", nullable: false),
                    RetailCheckSalesPriceStatus = table.Column<int>(type: "int", nullable: false),
                    RetailPricingPriorityNumber = table.Column<int>(type: "int", nullable: false),
                    PricingRuleRecId = table.Column<long>(type: "bigint", nullable: false),
                    PriceGroupAttributeEnable = table.Column<int>(type: "int", nullable: false),
                    Partition = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceDiscGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "PriceDiscTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCode = table.Column<int>(type: "int", nullable: false),
                    ItemRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountCode = table.Column<int>(type: "int", nullable: false),
                    AccountRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Relation = table.Column<int>(type: "int", nullable: false),
                    Module = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    Markup = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    Percent1 = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    Percent2 = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(32,12)", precision: 18, scale: 4, nullable: false),
                    QuantityAmountFrom = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    QuantityAmountTo = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalendarDays = table.Column<int>(type: "int", nullable: false),
                    DeliveryTime = table.Column<int>(type: "int", nullable: false),
                    DisregardLeadTime = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Agreement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PdsCalculationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    McrMerchandisingEventId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AllocateMarkup = table.Column<int>(type: "int", nullable: false),
                    GenericCurrency = table.Column<int>(type: "int", nullable: false),
                    SearchAgain = table.Column<int>(type: "int", nullable: false),
                    PriceApplyAdjustment = table.Column<int>(type: "int", nullable: false),
                    UnitAppliesToAll = table.Column<int>(type: "int", nullable: false),
                    IsGupTradeAgreement = table.Column<int>(type: "int", nullable: false),
                    McrPriceDiscGroupType = table.Column<int>(type: "int", nullable: false),
                    InventBaileeFreeDays_Ru = table.Column<int>(type: "int", nullable: false),
                    MaximumRetailPrice_In = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    McrFixedAmountCur = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    SubBillFlatTierPrice = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    AgreementHeaderExt_Ru = table.Column<long>(type: "bigint", nullable: false),
                    OriginalPriceDiscAdmTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    PricingRuleHeader = table.Column<long>(type: "bigint", nullable: false),
                    PricingRuleLine = table.Column<long>(type: "bigint", nullable: false),
                    PriceComponentCombination = table.Column<long>(type: "bigint", nullable: false),
                    ApplicabilityId = table.Column<long>(type: "bigint", nullable: false),
                    Partition = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceDiscTable", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_PriceDiscTable_CustTable_AccountRelation",
                        column: x => x.AccountRelation,
                        principalTable: "CustTable",
                        principalColumn: "AccountNum");
                    table.ForeignKey(
                        name: "FK_PriceDiscTable_InventDim_InventDimId",
                        column: x => x.InventDimId,
                        principalTable: "InventDim",
                        principalColumn: "InventDimId");
                    table.ForeignKey(
                        name: "FK_PriceDiscTable_InventTable_ItemRelation",
                        column: x => x.ItemRelation,
                        principalTable: "InventTable",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "PriceDiscAdmTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(32,16)", precision: 18, scale: 4, nullable: false),
                    ItemCode = table.Column<int>(type: "int", nullable: false),
                    ItemRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountCode = table.Column<int>(type: "int", nullable: false),
                    AccountRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Relation = table.Column<int>(type: "int", nullable: false),
                    Module = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    Markup = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    Percent1 = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    Percent2 = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(32,12)", precision: 18, scale: 4, nullable: false),
                    QuantityAmountFrom = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    QuantityAmountTo = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalendarDays = table.Column<int>(type: "int", nullable: false),
                    DeliveryTime = table.Column<int>(type: "int", nullable: false),
                    DisregardLeadTime = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Agreement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PdsCalculationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Log = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AllocateMarkup = table.Column<int>(type: "int", nullable: false),
                    DifferentFromPosted = table.Column<int>(type: "int", nullable: false),
                    GenericCurrency = table.Column<int>(type: "int", nullable: false),
                    MustBeDeleted = table.Column<int>(type: "int", nullable: false),
                    SearchAgain = table.Column<int>(type: "int", nullable: false),
                    PriceApplyAdjustment = table.Column<int>(type: "int", nullable: false),
                    UnitAppliesToAll = table.Column<int>(type: "int", nullable: false),
                    IsGupTradeAgreement = table.Column<int>(type: "int", nullable: false),
                    PricingAttributesHeaderAreMatched = table.Column<int>(type: "int", nullable: false),
                    PricingAttributesLineAreMatched = table.Column<int>(type: "int", nullable: false),
                    InventBaileeFreeDays_Ru = table.Column<int>(type: "int", nullable: false),
                    MaximumRetailPrice_In = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    SubBillFlatTierPrice = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    PriceDiscTableRef = table.Column<long>(type: "bigint", nullable: false),
                    AgreementHeaderExt_Ru = table.Column<long>(type: "bigint", nullable: false),
                    PricingRuleHeader = table.Column<long>(type: "bigint", nullable: false),
                    PricingRuleLine = table.Column<long>(type: "bigint", nullable: false),
                    PriceComponentCombination = table.Column<long>(type: "bigint", nullable: false),
                    Partition = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceDiscAdmTrans", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_PriceDiscAdmTrans_CustTable_AccountRelation",
                        column: x => x.AccountRelation,
                        principalTable: "CustTable",
                        principalColumn: "AccountNum");
                    table.ForeignKey(
                        name: "FK_PriceDiscAdmTrans_InventDim_InventDimId",
                        column: x => x.InventDimId,
                        principalTable: "InventDim",
                        principalColumn: "InventDimId");
                    table.ForeignKey(
                        name: "FK_PriceDiscAdmTrans_InventTable_ItemRelation",
                        column: x => x.ItemRelation,
                        principalTable: "InventTable",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_PriceDiscAdmTrans_PriceDiscAdmTable_JournalNum",
                        column: x => x.JournalNum,
                        principalTable: "PriceDiscAdmTable",
                        principalColumn: "JournalNum");
                    table.ForeignKey(
                        name: "FK_PriceDiscAdmTrans_PriceDiscTable_PriceDiscTableRef",
                        column: x => x.PriceDiscTableRef,
                        principalTable: "PriceDiscTable",
                        principalColumn: "RECID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustParameters_DataAreaId_Key",
                table: "CustParameters",
                columns: new[] { "DataAreaId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustParameters_DataAreaId_RECID",
                table: "CustParameters",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmName_DataAreaId_JournalName",
                table: "PriceDiscAdmName",
                columns: new[] { "DataAreaId", "JournalName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmName_DataAreaId_RECID",
                table: "PriceDiscAdmName",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTable_DataAreaId_JournalNum",
                table: "PriceDiscAdmTable",
                columns: new[] { "DataAreaId", "JournalNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTable_DataAreaId_RECID",
                table: "PriceDiscAdmTable",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_AccountRelation",
                table: "PriceDiscAdmTrans",
                column: "AccountRelation");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_DataAreaId_JournalNum_LineNum",
                table: "PriceDiscAdmTrans",
                columns: new[] { "DataAreaId", "JournalNum", "LineNum" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_DataAreaId_RECID",
                table: "PriceDiscAdmTrans",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_InventDimId",
                table: "PriceDiscAdmTrans",
                column: "InventDimId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_ItemRelation",
                table: "PriceDiscAdmTrans",
                column: "ItemRelation");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_JournalNum",
                table: "PriceDiscAdmTrans",
                column: "JournalNum");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscAdmTrans_PriceDiscTableRef",
                table: "PriceDiscAdmTrans",
                column: "PriceDiscTableRef");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscGroup_DataAreaId_GroupId_Module_Type",
                table: "PriceDiscGroup",
                columns: new[] { "DataAreaId", "GroupId", "Module", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscGroup_DataAreaId_RECID",
                table: "PriceDiscGroup",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscTable_AccountRelation",
                table: "PriceDiscTable",
                column: "AccountRelation");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscTable_DataAreaId_RECID",
                table: "PriceDiscTable",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscTable_InventDimId",
                table: "PriceDiscTable",
                column: "InventDimId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDiscTable_ItemRelation",
                table: "PriceDiscTable",
                column: "ItemRelation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustParameters");

            migrationBuilder.DropTable(
                name: "InventSerial");

            migrationBuilder.DropTable(
                name: "PriceDiscAdmName");

            migrationBuilder.DropTable(
                name: "PriceDiscAdmTrans");

            migrationBuilder.DropTable(
                name: "PriceDiscGroup");

            migrationBuilder.DropTable(
                name: "PriceDiscAdmTable");

            migrationBuilder.DropTable(
                name: "PriceDiscTable");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InventDim_InventDimId",
                table: "InventDim");

            migrationBuilder.AlterColumn<string>(
                name: "InventDimId",
                table: "InventDim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
