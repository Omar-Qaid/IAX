using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetPermissions",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetPermissions", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BankGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    SwiftNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegistrationNum = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    BankAccountStatus = table.Column<int>(type: "int", nullable: false),
                    BankCodeType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AccountingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    ReportingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    LastRevalResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BankMultiCurrency = table.Column<int>(type: "int", nullable: false),
                    RevalDimensionSetting = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    BridgingAccountLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    CustomerPaymentFeeLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceRemittanceLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    RemittanceCollectionLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    RemittanceDiscountLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    CustPaymFeePost = table.Column<int>(type: "int", nullable: false),
                    OverdraftLimit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CfmBankBalanceMinimum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscCreditMaxMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InvoiceRemitAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemitCollectionAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemitDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BankStatementFormat = table.Column<long>(type: "bigint", nullable: false),
                    BankReconciliationReportFormat = table.Column<long>(type: "bigint", nullable: false),
                    BankReconciliationMatchRuleSet = table.Column<long>(type: "bigint", nullable: false),
                    BankReconAllowedPennyDifference = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BankReconciliationEnabled = table.Column<int>(type: "int", nullable: false),
                    BankReconMatchAutoAfterImport = table.Column<int>(type: "int", nullable: false),
                    IsRunMatchingRule = table.Column<int>(type: "int", nullable: false),
                    BankReconBridgedAutoClearing = table.Column<int>(type: "int", nullable: false),
                    BankReconciliationStmtAsPaymConfirm = table.Column<int>(type: "int", nullable: false),
                    CompanyPaymId = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    DebitDirectId = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    BankCompanyStatementName = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BankDestinationName = table.Column<string>(type: "nvarchar(23)", maxLength: 23, nullable: false),
                    CustPaymentJournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VendPaymentJournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BankConstantSymbol = table.Column<long>(type: "bigint", nullable: false),
                    BankPositivePayStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrenoteResponseDays = table.Column<int>(type: "int", nullable: false),
                    IsBankPrenote = table.Column<int>(type: "int", nullable: false),
                    IsNachaFileBlocked = table.Column<int>(type: "int", nullable: false),
                    ReverseDebitCredit = table.Column<int>(type: "int", nullable: false),
                    NsfLedgerJournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NsfFeeMarkupGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NsfFeeMarkupGroupModule = table.Column<int>(type: "int", nullable: false),
                    CorrAccount_W = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveFromTzId = table.Column<int>(type: "int", nullable: false),
                    ActiveTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveToTzId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
                    LvDefaultBank = table.Column<int>(type: "int", nullable: false),
                    LvPayOrderType = table.Column<int>(type: "int", nullable: false),
                    TimeZonePreference = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_BankAccountTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "BankGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RegistrationNum = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    BankCodeType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CompanyPaymId = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    BankStatementFormat = table.Column<long>(type: "bigint", nullable: false),
                    TemplateRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyTemplateRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    BankCorrAccount_W = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_BankGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CurrencyCodeIso = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsEuro = table.Column<int>(type: "int", nullable: false),
                    RoundOffSales = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOffTypeSales = table.Column<int>(type: "int", nullable: false),
                    RoundOffPurch = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOffTypePurch = table.Column<int>(type: "int", nullable: false),
                    RoundOffPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOffTypePrice = table.Column<int>(type: "int", nullable: false),
                    RoundingPrecision = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LtmRoundOffLineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LtmRoundOffTypeLineAmount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Currency", x => x.RECID);
                    table.UniqueConstraint("AK_Currency_CurrencyCode", x => x.CurrencyCode);
                });

            migrationBuilder.CreateTable(
                name: "CustConfirmJour",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfirmDocNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfirmDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ConfirmAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EndDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    InclTax = table.Column<int>(type: "int", nullable: false),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FixedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    IntercompanyPosted = table.Column<int>(type: "int", nullable: false),
                    Triangulation = table.Column<int>(type: "int", nullable: false),
                    SubBillSuppressChildItems = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CustConfirmJour", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustConfirmTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfirmDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrigSalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineHeader = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DlvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LinePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmountTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxWriteCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    SalesCategory = table.Column<long>(type: "bigint", nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_CustConfirmTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PaymTermId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    AccountingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    ReportingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    CustAccountNumSeq = table.Column<long>(type: "bigint", nullable: false),
                    CustWriteOffRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    BankCustPaymIdTable = table.Column<long>(type: "bigint", nullable: false),
                    ClearingPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceIncludeSalesTax = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustInvoiceJour",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LedgerVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SalesType = table.Column<int>(type: "int", nullable: false),
                    DocumentStatus = table.Column<int>(type: "int", nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoicingName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IntercompanyCompanyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    McrEmail = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OneTimeCustomer = table.Column<int>(type: "int", nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InvoicePostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EndDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InvoiceRoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HeaderTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InclTax = table.Column<int>(type: "int", nullable: false),
                    InvoiceAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesBalanceMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDiscMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EndDiscMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumMarkupMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumTaxMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InvoiceRoundOffMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchangeRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchangeRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentSched = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymDayId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FixedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscBaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DirectDebitMandate = table.Column<long>(type: "bigint", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    VatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartyTaxId = table.Column<long>(type: "bigint", nullable: false),
                    ReverseChargeAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GiroType = table.Column<int>(type: "int", nullable: false),
                    TaxPrintOnInvoice = table.Column<int>(type: "int", nullable: false),
                    TaxSpecifyByLine = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrintMgmtSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BillOfLadingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransportationDocument = table.Column<long>(type: "bigint", nullable: false),
                    BankLcExportLine = table.Column<long>(type: "bigint", nullable: false),
                    Backorder = table.Column<int>(type: "int", nullable: false),
                    CovStatus = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierBlindShipment = table.Column<int>(type: "int", nullable: false),
                    SalesOriginId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetailStoreIdTable = table.Column<long>(type: "bigint", nullable: false),
                    OfferId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReturnItemNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReturnStatus = table.Column<int>(type: "int", nullable: false),
                    McrDueAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrPaymAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OnAccountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ZatcaRetInvoiceRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ZatcaRetReason = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EInvoiceLineSpecific = table.Column<int>(type: "int", nullable: false),
                    SentElectronically = table.Column<int>(type: "int", nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    ReasonTableRef = table.Column<long>(type: "bigint", nullable: false),
                    ReversedRecId = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    ServiceCodeRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    RefNum = table.Column<int>(type: "int", nullable: false),
                    PrintedOriginals = table.Column<int>(type: "int", nullable: false),
                    InvoiceType_W = table.Column<int>(type: "int", nullable: false),
                    Proforma = table.Column<int>(type: "int", nullable: false),
                    IsCorrection = table.Column<int>(type: "int", nullable: false),
                    Updated = table.Column<int>(type: "int", nullable: false),
                    IntercompanyPosted = table.Column<int>(type: "int", nullable: false),
                    Triangulation = table.Column<int>(type: "int", nullable: false),
                    PostedState = table.Column<int>(type: "int", nullable: false),
                    Prepayment = table.Column<int>(type: "int", nullable: false),
                    SubBillSuppressChildItems = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustInvoiceJour", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustInvoiceLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InvoiceTxt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BillingCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustBillingCode = table.Column<long>(type: "bigint", nullable: false),
                    CustInvoiceLineTemplate = table.Column<long>(type: "bigint", nullable: false),
                    PeriodChargeInvoiceLineBaseFromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodChargeInvoiceLineBaseToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    AccountingDistributionTemplate = table.Column<long>(type: "bigint", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxWithholdGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssetBookId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParentRecId = table.Column<long>(type: "bigint", nullable: false),
                    CorrectedCustInvoiceLine = table.Column<long>(type: "bigint", nullable: false),
                    RefReturnInvoiceTrans_W = table.Column<long>(type: "bigint", nullable: false),
                    ProjFundingSource = table.Column<long>(type: "bigint", nullable: false),
                    IntrastatCommodity = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    ReasonRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustInvoiceLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustInvoiceTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderAccountRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    CustGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjIntercompany = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OneTimeCustomer = table.Column<int>(type: "int", nullable: false),
                    PostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryLocation = table.Column<long>(type: "bigint", nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransportationDocument = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchRate_W = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentSched = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesDate_W = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustBankAccountID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectDebitMandate = table.Column<long>(type: "bigint", nullable: false),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CashDiscDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscBaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscBaseDays = table.Column<int>(type: "int", nullable: false),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    CustBillingClassification = table.Column<long>(type: "bigint", nullable: false),
                    AccountingDistributionTemplate = table.Column<long>(type: "bigint", nullable: false),
                    Posted = table.Column<int>(type: "int", nullable: false),
                    SubledgerJournalStatus = table.Column<int>(type: "int", nullable: false),
                    ExcludeFromDecoupledPostingProcess = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    VatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VatNumRecId = table.Column<long>(type: "bigint", nullable: false),
                    VatDueDate_W = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxWithholdCalculate = table.Column<int>(type: "int", nullable: false),
                    PostponeVat = table.Column<int>(type: "int", nullable: false),
                    VatNumTableType = table.Column<int>(type: "int", nullable: false),
                    ZatcaRetInvoiceRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ZatcaRetReason = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EInvoiceLineSpec = table.Column<int>(type: "int", nullable: false),
                    InvoiceType_W = table.Column<int>(type: "int", nullable: false),
                    InvoiceComplementaryType = table.Column<int>(type: "int", nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    CorrectionReasonCode = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    ServiceCodeRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    AdjustingInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseDateTzId = table.Column<int>(type: "int", nullable: false),
                    Touched = table.Column<int>(type: "int", nullable: false),
                    WorkflowApprovalState = table.Column<int>(type: "int", nullable: false),
                    WorkflowApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    CovStatus = table.Column<int>(type: "int", nullable: false),
                    ForInterestAdjustment = table.Column<int>(type: "int", nullable: false),
                    GiroType = table.Column<int>(type: "int", nullable: false),
                    UseDefaultFromCustomer = table.Column<int>(type: "int", nullable: false),
                    IntercompanyPosted = table.Column<int>(type: "int", nullable: false),
                    ListCode = table.Column<int>(type: "int", nullable: false),
                    ManualNumbering_W = table.Column<int>(type: "int", nullable: false),
                    McrGiftCard = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustInvoiceTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustInvoiceTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrigSalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCreationSequenceNumber = table.Column<int>(type: "int", nullable: false),
                    LineHeader = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefType = table.Column<byte>(type: "tinyint", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QtyPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remain = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainBefore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CustomerLineNum = table.Column<int>(type: "int", nullable: false),
                    PartDelivery = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LinePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCharge = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OlapCostValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDiscMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StatLineAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxWriteCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmountTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmountTaxMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    ReverseCharge_W = table.Column<int>(type: "int", nullable: false),
                    ReverseChargeSalesList = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    CustInvoiceLineIdRef = table.Column<long>(type: "bigint", nullable: false),
                    CommissAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CommissAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CommissCalc = table.Column<int>(type: "int", nullable: false),
                    DlvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    McrDeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    McrDlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    SalesCategory = table.Column<long>(type: "bigint", nullable: false),
                    RetailCategory = table.Column<long>(type: "bigint", nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BillingCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssetBookId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReturnArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnClosedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDispositionCodeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PeriodChargeInvoiceLineBaseFromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodChargeInvoiceLineBaseToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubBillRevenueSplitParentAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SubBillRevenueSplitParentLineRecId = table.Column<long>(type: "bigint", nullable: false),
                    RevRecDeferred = table.Column<int>(type: "int", nullable: false),
                    RevRecDeferredProcessed = table.Column<int>(type: "int", nullable: false),
                    SubBillRevenueSplit = table.Column<int>(type: "int", nullable: false),
                    SubBillRevenueSplitAllocationMethod = table.Column<int>(type: "int", nullable: false),
                    ParentRecId = table.Column<long>(type: "bigint", nullable: false),
                    ReversedRecId = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    ReasonRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustInvoiceTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustLedger",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CollectionLetter = table.Column<int>(type: "int", nullable: false),
                    Interest = table.Column<int>(type: "int", nullable: false),
                    Settlement = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustLedger", x => x.RECID);
                    table.UniqueConstraint("AK_CustLedger_PostingProfile", x => x.PostingProfile);
                });

            migrationBuilder.CreateTable(
                name: "CustPackingSlipJour",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackingSlipId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InternalPackingSlipId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LedgerVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SalesType = table.Column<int>(type: "int", nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoicingName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IntercompanyCompanyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InvoicePostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    PartyTaxId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceIssueDueDate_W = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrintMgmtSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransportationDocument = table.Column<long>(type: "bigint", nullable: false),
                    BankLcExportLine = table.Column<long>(type: "bigint", nullable: false),
                    TransportationDeliveryContractor = table.Column<long>(type: "bigint", nullable: false),
                    TransportationDeliveryLoader = table.Column<long>(type: "bigint", nullable: false),
                    TransportationDeliveryOwner = table.Column<long>(type: "bigint", nullable: false),
                    FreightSlipType = table.Column<int>(type: "int", nullable: false),
                    BolFreightedBy = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierBlindShipment = table.Column<int>(type: "int", nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    Compiler = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: false),
                    RefNum = table.Column<int>(type: "int", nullable: false),
                    ListCode = table.Column<int>(type: "int", nullable: false),
                    IntercompanyPosted = table.Column<int>(type: "int", nullable: false),
                    PostedState = table.Column<int>(type: "int", nullable: false),
                    Printed = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CustPackingSlipJour", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustPackingSlipTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackingSlipId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrigSalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineHeader = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventRefTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefType = table.Column<byte>(type: "tinyint", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Ordered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remain = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainInvent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwRemain = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    SalesLineShippingDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesLineShippingDateConfirmed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ValueMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StatValueMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    SalesCategory = table.Column<long>(type: "bigint", nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ParentRecId = table.Column<long>(type: "bigint", nullable: false),
                    ParmLine = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceTransRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    DeferredPostInvoiceTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    IntrastatCommodity = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    FullyMatched = table.Column<int>(type: "int", nullable: false),
                    Scrap = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CustPackingSlipTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustPaymModeTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    PaymStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    TypeOfDraft = table.Column<int>(type: "int", nullable: false),
                    PaymJournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DimensionAttributeSetId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    InterCompanyLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    DiscGraceDays = table.Column<int>(type: "int", nullable: false),
                    LastSequenceNumber = table.Column<int>(type: "int", nullable: false),
                    LastSequenceNumDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumToday = table.Column<int>(type: "int", nullable: false),
                    PaymGenerationLineLimit = table.Column<int>(type: "int", nullable: false),
                    PaymSumBy = table.Column<int>(type: "int", nullable: false),
                    SplitPaymentW = table.Column<int>(type: "int", nullable: false),
                    DimCtrl = table.Column<int>(type: "int", nullable: false),
                    DimUse = table.Column<int>(type: "int", nullable: false),
                    DimUse2 = table.Column<int>(type: "int", nullable: false),
                    DimUse3 = table.Column<int>(type: "int", nullable: false),
                    BankCustPaymIdTable = table.Column<long>(type: "bigint", nullable: true),
                    ErFormatMappingId = table.Column<long>(type: "bigint", nullable: true),
                    ErModelMappingTable = table.Column<long>(type: "bigint", nullable: true),
                    CategoryPurposeW = table.Column<long>(type: "bigint", nullable: true),
                    ChargeBearerW = table.Column<long>(type: "bigint", nullable: true),
                    LocalInstrumentW = table.Column<long>(type: "bigint", nullable: true),
                    ServiceLevelW = table.Column<long>(type: "bigint", nullable: true),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    ClassIdFileAnalyze = table.Column<int>(type: "int", nullable: false),
                    ClassIdIn = table.Column<int>(type: "int", nullable: false),
                    ClassIdRemittance = table.Column<int>(type: "int", nullable: false),
                    ClassIdReturn = table.Column<int>(type: "int", nullable: false),
                    UseGerImport = table.Column<int>(type: "int", nullable: false),
                    UseGerConfiguration = table.Column<int>(type: "int", nullable: false),
                    PdcClearingPosting = table.Column<int>(type: "int", nullable: false),
                    ExportOnInvoice = table.Column<int>(type: "int", nullable: false),
                    FurtherPosting = table.Column<int>(type: "int", nullable: false),
                    IsSepa = table.Column<int>(type: "int", nullable: false),
                    PaymOnInvoice = table.Column<int>(type: "int", nullable: false),
                    DirectDebit = table.Column<int>(type: "int", nullable: false),
                    BridgingAccountByBank = table.Column<int>(type: "int", nullable: false),
                    ExportRefund = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustPaymModeTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustQuotationJour",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotationDocNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    QuotationType = table.Column<int>(type: "int", nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusRelAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    QuotationAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EndDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    InclTax = table.Column<int>(type: "int", nullable: false),
                    QuotationAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesBalanceMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumLineDiscMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EndDiscMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumMarkupMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SumTaxMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOffMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FixedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespiteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    IntercompanyPosted = table.Column<int>(type: "int", nullable: false),
                    Triangulation = table.Column<int>(type: "int", nullable: false),
                    Assessment = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustQuotationJour", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustQuotationTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrigQuotationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineHeader = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DlvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LinePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmountTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxWriteCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    LineAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmountTaxMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    OffsetAccountType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjCategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProjDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LinePropertyId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProjectResource = table.Column<long>(type: "bigint", nullable: false),
                    ProjTransType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    SalesCategory = table.Column<long>(type: "bigint", nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustQuotationTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustSettlement",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SettlementVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TransType = table.Column<int>(type: "int", nullable: false),
                    TransRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransOpen = table.Column<long>(type: "bigint", nullable: false),
                    OffsetRecId = table.Column<long>(type: "bigint", nullable: false),
                    OffsetAccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OffsetCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    OffsetTransVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettleAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettlementGroup = table.Column<long>(type: "bigint", nullable: false),
                    SettleAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchAdjustment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PennyDiff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleAmountReporting = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchAdjustmentReporting = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UtilizedCashDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CustCashDiscDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscountLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastInterestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SettleTax1099Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleTax1099StateAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    CanBeReversed = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CustSettlement", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustCategory = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustGroupId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Party = table.Column<long>(type: "bigint", nullable: false),
                    SalesPoolId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VendAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreditMax = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    MandatoryCreditLimit = table.Column<int>(type: "int", nullable: false),
                    Blocked = table.Column<int>(type: "int", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    PaymModeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymTermId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CashDiscBaseDays = table.Column<int>(type: "int", nullable: false),
                    UseCashDisc = table.Column<int>(type: "int", nullable: false),
                    TaxGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VatFileAttachment = table.Column<int>(type: "int", nullable: false),
                    VatNumRecId = table.Column<long>(type: "bigint", nullable: false),
                    VatNumTableType = table.Column<int>(type: "int", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    InclTax = table.Column<int>(type: "int", nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvModeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PartyCountry = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PartyState = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InvoiceAddress = table.Column<int>(type: "int", nullable: false),
                    PrepaymentValue = table.Column<decimal>(type: "decimal(32,6)", precision: 18, scale: 4, nullable: false),
                    PrePayType = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountStatement = table.Column<int>(type: "int", nullable: false),
                    AccStmtSign = table.Column<int>(type: "int", nullable: false),
                    CollectionLetterCode = table.Column<int>(type: "int", nullable: false),
                    GiroTypeAccountStatement = table.Column<int>(type: "int", nullable: false),
                    GiroTypeCollectionLetter = table.Column<int>(type: "int", nullable: false),
                    CreditCardAddressVerification = table.Column<int>(type: "int", nullable: false),
                    CreditCardAddressVerificationLevel = table.Column<int>(type: "int", nullable: false),
                    CreditCardAddressVerificationVoid = table.Column<int>(type: "int", nullable: false),
                    CreditCardCvc = table.Column<int>(type: "int", nullable: false),
                    CredManCustCreditMaxAlt = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CredManCustomerSince = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CredManCustUnlimitedCredit = table.Column<int>(type: "int", nullable: false),
                    CredManEligibleCreditLimitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CredManEligibleCreditMax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CredManExclude = table.Column<int>(type: "int", nullable: false),
                    CredManLastReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CredManNextSchedReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CredManTitleHeld = table.Column<int>(type: "int", nullable: false),
                    CredManWithAgency = table.Column<int>(type: "int", nullable: false),
                    CredManNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    CredManAccountStatusId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CredManBusinessStarted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CredManCreditLimitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CredManCreditLimitExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustExcludeCollectionFee = table.Column<int>(type: "int", nullable: false),
                    CustExcludeInterestCharges = table.Column<int>(type: "int", nullable: false),
                    CustTradingPartnerCode = table.Column<long>(type: "bigint", nullable: false),
                    CustWriteOffRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDirectDebitMandate = table.Column<long>(type: "bigint", nullable: false),
                    DocValid = table.Column<int>(type: "int", nullable: false),
                    EInvoice = table.Column<int>(type: "int", nullable: false),
                    EInvoiceAttachment = table.Column<int>(type: "int", nullable: false),
                    EntryCertificateRequiredW = table.Column<int>(type: "int", nullable: false),
                    ExpressBillOfLading = table.Column<int>(type: "int", nullable: false),
                    FedNonFedIndicator = table.Column<int>(type: "int", nullable: false),
                    ForecastDmpInclude = table.Column<int>(type: "int", nullable: false),
                    GiroType = table.Column<int>(type: "int", nullable: false),
                    GiroTypeFreeTextInvoice = table.Column<int>(type: "int", nullable: false),
                    GiroTypeInterestNote = table.Column<int>(type: "int", nullable: false),
                    GiroTypeProjInvoice = table.Column<int>(type: "int", nullable: false),
                    CompanyNafCode = table.Column<long>(type: "bigint", nullable: false),
                    Government = table.Column<int>(type: "int", nullable: false),
                    Cr = table.Column<int>(type: "int", nullable: false),
                    CrDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CrEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CrParentEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterCompanyAllowIndirectCreation = table.Column<int>(type: "int", nullable: false),
                    InterCompanyAutoCreateOrders = table.Column<int>(type: "int", nullable: false),
                    InterCompanyDirectDelivery = table.Column<int>(type: "int", nullable: false),
                    IsExternallyMaintained = table.Column<int>(type: "int", nullable: false),
                    WorkflowState = table.Column<int>(type: "int", nullable: false),
                    ValidatedFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidatedTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BankCustPaymIdTable = table.Column<long>(type: "bigint", nullable: false),
                    LvPaymTransCodes = table.Column<long>(type: "bigint", nullable: false),
                    MainContactWorker = table.Column<long>(type: "bigint", nullable: false),
                    MainHolding = table.Column<int>(type: "int", nullable: false),
                    JointVenture = table.Column<int>(type: "int", nullable: false),
                    OneTimeCustomer = table.Column<int>(type: "int", nullable: false),
                    NationalAddress = table.Column<int>(type: "int", nullable: false),
                    SiteSketch = table.Column<int>(type: "int", nullable: false),
                    BlockFloorLimitUseInChannel = table.Column<int>(type: "int", nullable: false),
                    PdsFreightAccrued = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierBlindShipment = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierFuelSurcharge = table.Column<int>(type: "int", nullable: false),
                    IssueOwnEntryCertificateW = table.Column<int>(type: "int", nullable: false),
                    QmsCustomerCheckItem = table.Column<int>(type: "int", nullable: false),
                    QmsPrintCustSpecificCertOfAnalysis = table.Column<int>(type: "int", nullable: false),
                    RfidCaseTagging = table.Column<int>(type: "int", nullable: false),
                    RfidItemTagging = table.Column<int>(type: "int", nullable: false),
                    RfidPalletTagging = table.Column<int>(type: "int", nullable: false),
                    UsePurchRequest = table.Column<int>(type: "int", nullable: false),
                    OpeningAccFile = table.Column<int>(type: "int", nullable: false),
                    OwnerIdCopy = table.Column<int>(type: "int", nullable: false),
                    RecipSign = table.Column<int>(type: "int", nullable: false),
                    RevRecDisableInterCompany = table.Column<int>(type: "int", nullable: false),
                    Stamp = table.Column<int>(type: "int", nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustTable", x => x.RECID);
                    table.UniqueConstraint("AK_CustTable_AccountNum", x => x.AccountNum);
                });

            migrationBuilder.CreateTable(
                name: "CustTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Invoice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TransType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateSecond = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FixedExchRate = table.Column<int>(type: "int", nullable: false),
                    AmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CustExchAdjustmentRealized = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CustExchAdjustmentUnrealized = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchAdjustment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleAmountReporting = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingExchAdjustmentRealized = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingExchAdjustmentUnrealized = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchAdjustmentReporting = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyCrossRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Closed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSettleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSettleAccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastSettleCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    LastSettleVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OffsetRecId = table.Column<long>(type: "bigint", nullable: false),
                    Settlement = table.Column<int>(type: "int", nullable: false),
                    LastExchAdj = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastExchAdjRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LastExchAdjRateReporting = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LastExchAdjVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymTermId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymReference = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    PaymSchedId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    McrPaymOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CashDiscBaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyBankAccountID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ThirdPartyBankAccountID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DirectDebitMandate = table.Column<long>(type: "bigint", nullable: false),
                    PaymMethod = table.Column<int>(type: "int", nullable: false),
                    Prepayment = table.Column<int>(type: "int", nullable: false),
                    CashPayment = table.Column<int>(type: "int", nullable: false),
                    CancelledPayment = table.Column<int>(type: "int", nullable: false),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    CustBillingClassification = table.Column<long>(type: "bigint", nullable: false),
                    Approver = table.Column<long>(type: "bigint", nullable: false),
                    ReasonRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    Approved = table.Column<int>(type: "int", nullable: false),
                    Correct = table.Column<int>(type: "int", nullable: false),
                    Interest = table.Column<int>(type: "int", nullable: false),
                    InvoiceProject = table.Column<int>(type: "int", nullable: false),
                    CredManExcludeFromCreditControl = table.Column<int>(type: "int", nullable: false),
                    CollectionLetter = table.Column<int>(type: "int", nullable: false),
                    CollectionLetterCode = table.Column<int>(type: "int", nullable: false),
                    CustAutomationExclude = table.Column<int>(type: "int", nullable: false),
                    CustAutomationPredictionSent = table.Column<int>(type: "int", nullable: false),
                    CustAutomationPredunningSent = table.Column<int>(type: "int", nullable: false),
                    BillOfExchangeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillOfExchangeSeqNum = table.Column<int>(type: "int", nullable: false),
                    BillOfExchangeStatus = table.Column<int>(type: "int", nullable: false),
                    RetailStoreId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetailTerminalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetailTransactionId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RetailCustTrans = table.Column<int>(type: "int", nullable: false),
                    DeliveryMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BankLcExportLine = table.Column<long>(type: "bigint", nullable: false),
                    EuroTriangulation = table.Column<int>(type: "int", nullable: false),
                    AccountingEvent = table.Column<long>(type: "bigint", nullable: false),
                    CreatedTransactionId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CustTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "CustTransOpen",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchAdjUnrealized = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchAdjUnrealizedReporting = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PossibleCashDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscountLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    SettlementPriorityCashDiscDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UseCashDisc = table.Column<int>(type: "int", nullable: false),
                    LastInterestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CollectionLetter = table.Column<int>(type: "int", nullable: false),
                    CollectionLetterCode = table.Column<int>(type: "int", nullable: false),
                    BankLcExportLine = table.Column<long>(type: "bigint", nullable: false),
                    BankDiscNoticeDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CovStatus = table.Column<int>(type: "int", nullable: false),
                    TaxDistribution = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustTransOpen", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DimensionAttributeValueCombination",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    HashVersion = table.Column<int>(type: "int", nullable: false),
                    AccountStructure = table.Column<long>(type: "bigint", nullable: false),
                    LedgerDimensionType = table.Column<int>(type: "int", nullable: false),
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
                    LegalEntity = table.Column<long>(type: "bigint", nullable: false),
                    LegalEntityValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    RetailChannel = table.Column<long>(type: "bigint", nullable: false),
                    RetailChannelValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    Ownership_CnValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeFixedAssets_RuValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedJournalAccount = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedJournalAccountValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedJournalAccountType = table.Column<int>(type: "int", nullable: false),
                    SystemGeneratedAttributeBankAccount = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeBankAccountValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeCustomer = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeCustomerValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeVendor = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeVendorValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeFixedAsset = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeFixedAssetValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeProject = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeProjectValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeEmployee = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeItem = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeRCash = table.Column<long>(type: "bigint", nullable: false),
                    SystemGeneratedAttributeRCashValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SystemGeneratedAttributeRDeferrals = table.Column<long>(type: "bigint", nullable: false),
                    BankAccount = table.Column<long>(type: "bigint", nullable: false),
                    Campaign = table.Column<long>(type: "bigint", nullable: false),
                    Cargo = table.Column<long>(type: "bigint", nullable: false),
                    Center = table.Column<long>(type: "bigint", nullable: false),
                    Condition = table.Column<long>(type: "bigint", nullable: false),
                    Contract = table.Column<long>(type: "bigint", nullable: false),
                    Customer = table.Column<long>(type: "bigint", nullable: false),
                    JobSkills = table.Column<long>(type: "bigint", nullable: false),
                    LaborType = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: false),
                    Primary_ = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<long>(type: "bigint", nullable: false),
                    Store = table.Column<long>(type: "bigint", nullable: false),
                    Vehicle = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionAttributeValueCombination", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DlvMode",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierDlvType = table.Column<int>(type: "int", nullable: false),
                    MarkupGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    McrExpedite = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DomPriority = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DlvMode", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DlvTerm",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShipCarrierFreeMinimum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightChargeTerm = table.Column<int>(type: "int", nullable: false),
                    TaxLocationRole = table.Column<int>(type: "int", nullable: false),
                    ItmGoodsInTransitControl = table.Column<int>(type: "int", nullable: false),
                    ItmPortMandatory = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DlvTerm", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "DocuType",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ACTIONCLASSID = table.Column<int>(type: "int", nullable: false),
                    ARCHIVEPATH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOCUSTRUCTURETYPE = table.Column<int>(type: "int", nullable: false),
                    FILEPLACE = table.Column<int>(type: "int", nullable: false),
                    FILEREMOVALCONFIRMATION = table.Column<int>(type: "int", nullable: false),
                    TYPEID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NAME = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TYPEGROUP = table.Column<int>(type: "int", nullable: false),
                    PARAMETERS = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    REMOVEOPTION = table.Column<int>(type: "int", nullable: false),
                    HOST = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: true),
                    SITE = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: true),
                    FOLDERPATH = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: true),
                    PARTITION = table.Column<long>(type: "bigint", nullable: false),
                    CREATEDBY = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CREATEDDATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    MODIFIEDBY = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    MODIFIEDDATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SYSROWVERSION = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RECVERSION = table.Column<int>(type: "int", nullable: false),
                    DATAAREAID = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocuType", x => x.RECID);
                    table.UniqueConstraint("AK_DocuType_TYPEID", x => x.TYPEID);
                });

            migrationBuilder.CreateTable(
                name: "DocuValue",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FILE_ = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FILENAME = table.Column<string>(type: "nvarchar(518)", maxLength: 518, nullable: false),
                    FILETYPE = table.Column<string>(type: "nvarchar(518)", maxLength: 518, nullable: false),
                    NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ORIGINALFILENAME = table.Column<string>(type: "nvarchar(518)", maxLength: 518, nullable: false),
                    PATH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TYPE = table.Column<int>(type: "int", nullable: false),
                    FILEID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MCRDOCUSUBJECT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCESSINFORMATION = table.Column<string>(type: "nvarchar(2520)", maxLength: 2520, nullable: false),
                    STORAGEPROVIDERID = table.Column<int>(type: "int", nullable: false),
                    DOCUMENTHASHNUMBER = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PARTITION = table.Column<long>(type: "bigint", nullable: false),
                    CREATEDBY = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CREATEDDATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    MODIFIEDBY = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    MODIFIEDDATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SYSROWVERSION = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RECVERSION = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocuValue", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRateType",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("PK_ExchangeRateType", x => x.RECID);
                    table.UniqueConstraint("AK_ExchangeRateType_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "FiscalCalendar",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalendarId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("PK_FiscalCalendar", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "GeneralJournalAccountEntry",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneralJournalEntry = table.Column<long>(type: "bigint", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    MainAccount = table.Column<long>(type: "bigint", nullable: false),
                    LedgerAccount = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PostingType = table.Column<int>(type: "int", nullable: false),
                    TransactionCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TransactionCurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AccountingCurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HistoricalExchangeRateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCredit = table.Column<int>(type: "int", nullable: false),
                    IsCorrection = table.Column<int>(type: "int", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    ReasonRef = table.Column<long>(type: "bigint", nullable: false),
                    OriginalAccountEntry = table.Column<long>(type: "bigint", nullable: false),
                    AllocationLevel = table.Column<int>(type: "int", nullable: false),
                    AssetLeasePostingTypes = table.Column<int>(type: "int", nullable: false),
                    AssetLeaseTransactionType = table.Column<int>(type: "int", nullable: false),
                    CreatedTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_GeneralJournalAccountEntry", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "GeneralJournalEntry",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ledger = table.Column<long>(type: "bigint", nullable: false),
                    LedgerEntryJournal = table.Column<long>(type: "bigint", nullable: false),
                    JournalCategory = table.Column<int>(type: "int", nullable: false),
                    PostingLayer = table.Column<int>(type: "int", nullable: false),
                    SubledgerVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubledgerVoucherDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FiscalCalendarPeriod = table.Column<long>(type: "bigint", nullable: false),
                    FiscalCalendarYear = table.Column<long>(type: "bigint", nullable: false),
                    BudgetSourceLedgerEntryPosted = table.Column<long>(type: "bigint", nullable: false),
                    TransferId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_GeneralJournalEntry", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventBatch",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventBatchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdsBestBeforeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdsShelfAdviceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdsFinishedGoodsDateTested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdsDispositionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PdsSameLot = table.Column<int>(type: "int", nullable: false),
                    PdsVendBatchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PdsVendBatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdsVendExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManufacturerId = table.Column<long>(type: "bigint", nullable: false),
                    OriginManufacturerId = table.Column<long>(type: "bigint", nullable: false),
                    PdsUseVendBatchDate = table.Column<int>(type: "int", nullable: false),
                    PdsUseVendBatchExp = table.Column<int>(type: "int", nullable: false),
                    PdsInheritBatchAttrib = table.Column<int>(type: "int", nullable: false),
                    PdsInheritedShelfLife = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventBatch", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventClosing",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PeriodCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RunNum = table.Column<int>(type: "int", nullable: false),
                    NextRunNum = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjustmentSpec = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    InventCostStatus = table.Column<int>(type: "int", nullable: false),
                    Cancellation = table.Column<int>(type: "int", nullable: false),
                    CancelClosing = table.Column<int>(type: "int", nullable: false),
                    RunRecalculation = table.Column<int>(type: "int", nullable: false),
                    Executed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BomLevel = table.Column<int>(type: "int", nullable: false),
                    NumOfIteration = table.Column<int>(type: "int", nullable: false),
                    MaxIterations = table.Column<int>(type: "int", nullable: false),
                    MinTransferValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HelpersCreated = table.Column<int>(type: "int", nullable: false),
                    CancelClosingRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    Start_ = table.Column<int>(type: "int", nullable: false),
                    End_ = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<int>(type: "int", nullable: false),
                    LedgerPostingBatch = table.Column<long>(type: "bigint", nullable: false),
                    Ledger = table.Column<int>(type: "int", nullable: false),
                    LedgerCorrection = table.Column<int>(type: "int", nullable: false),
                    ItmAdjustment = table.Column<int>(type: "int", nullable: false),
                    ProdJournal = table.Column<int>(type: "int", nullable: false),
                    StopOnError = table.Column<int>(type: "int", nullable: false),
                    StopRunning = table.Column<int>(type: "int", nullable: false),
                    ShouldSummarizeInfolog = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventClosing", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventCountJour",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventOnHand = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CountedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Worker = table.Column<long>(type: "bigint", nullable: false),
                    Ok = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventCountJour", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventDim",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sha1HashHex = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Sha3HashHex = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ConfigId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InventSizeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventColorId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventStyleId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventVersionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WmsLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LicensePlateId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    InventBatchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventSerialId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventStatusId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventDimension10 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventDimension9 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventDimension9TzId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventDim", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventItemBarcode",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemBarcode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BarcodeSetupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UseForInput = table.Column<int>(type: "int", nullable: false),
                    UseForPrinting = table.Column<int>(type: "int", nullable: false),
                    Blocked = table.Column<int>(type: "int", nullable: false),
                    RetailVariantId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetailShowForItem = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventItemBarcode", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventItemGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TaxItemGroupIdSales = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroupIdPurch = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RevRecDefaultRevenueRecognitionSchedule = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RevRecMedianPriceMinimumTolerance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecMedianPriceMaximumTolerance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecRevenueRecognitionEnabled = table.Column<int>(type: "int", nullable: false),
                    RevRecRevenueType = table.Column<int>(type: "int", nullable: false),
                    RevRecMedianPrice = table.Column<int>(type: "int", nullable: false),
                    RevRecExcludeFromCarveOut = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventItemGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventItemLocation",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickingLocationMaxQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PickingLocationRefillMin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwPickingLocationMaxQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwPickingLocationRefillMin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    WmsLocationIdDefaultReceipt = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WmsLocationIdDefaultIssue = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UseWmsOrder = table.Column<int>(type: "int", nullable: false),
                    UseEmptyPalletLocation = table.Column<int>(type: "int", nullable: false),
                    CountGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_InventItemLocation", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventItemPrice",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VersionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceCalcId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CostingType = table.Column<int>(type: "int", nullable: false),
                    PriceType = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Markup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceAllocateMarkup = table.Column<int>(type: "int", nullable: false),
                    ActivationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StdCostTransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StdCostVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastPriceUniquenessAllowance = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_InventItemPrice", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventJournalName",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    JournalType = table.Column<byte>(type: "tinyint", nullable: false),
                    VoucherNumberSequenceTable = table.Column<long>(type: "bigint", nullable: false),
                    VoucherChange = table.Column<int>(type: "int", nullable: false),
                    VoucherDraw = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowApproval = table.Column<int>(type: "int", nullable: false),
                    DetailSummary = table.Column<int>(type: "int", nullable: false),
                    DeletePostedLines = table.Column<int>(type: "int", nullable: false),
                    Reservation = table.Column<int>(type: "int", nullable: false),
                    CountingStatusRegistrationPolicy = table.Column<int>(type: "int", nullable: false),
                    ExcludeWarehouseInventoryUpdateLogs = table.Column<int>(type: "int", nullable: false),
                    RetailInventJournalPosAdjustmentType = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventJournalName", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventJournalTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NumOfLines = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JournalIdOrignal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalType = table.Column<byte>(type: "tinyint", nullable: false),
                    JournalOriginType = table.Column<int>(type: "int", nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventDimFixed = table.Column<int>(type: "int", nullable: false),
                    VoucherNumberSequenceTable = table.Column<long>(type: "bigint", nullable: false),
                    VoucherChange = table.Column<int>(type: "int", nullable: false),
                    VoucherDraw = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    PostedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedDateTimeTzId = table.Column<int>(type: "int", nullable: false),
                    PostedUserId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Worker = table.Column<long>(type: "bigint", nullable: false),
                    Posted = table.Column<int>(type: "int", nullable: false),
                    DetailSummary = table.Column<int>(type: "int", nullable: false),
                    DeletePostedLines = table.Column<int>(type: "int", nullable: false),
                    Reservation = table.Column<int>(type: "int", nullable: false),
                    CountingStatusRegistrationPolicy = table.Column<int>(type: "int", nullable: false),
                    InventoryServiceJournalExpectedStatus = table.Column<int>(type: "int", nullable: false),
                    IsRetailCommitted = table.Column<int>(type: "int", nullable: false),
                    RetailReplenishmentType = table.Column<int>(type: "int", nullable: false),
                    RetailRetailStatusType = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    SessionLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionLoginDateTimeTzId = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    SystemBlocked = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventJournalTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventJournalTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JournalType = table.Column<byte>(type: "tinyint", nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToInventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToInventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransIdFather = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefType = table.Column<byte>(type: "tinyint", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventOnHand = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Counted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UnitQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwInventOnHand = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwInventQtyCounted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCopyBatchAttrib = table.Column<int>(type: "int", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProfitSet = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    ProjId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjCategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProjLinePropertyId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProjSalesCurrencyId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ProjUnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProjTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjSalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProjTaxGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProjTaxItemGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProdGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EntAssetWorkOrderLine = table.Column<long>(type: "bigint", nullable: false),
                    BomLine = table.Column<int>(type: "int", nullable: false),
                    AssetTransType = table.Column<int>(type: "int", nullable: false),
                    SubBillDeferralRecIdOriginal = table.Column<long>(type: "bigint", nullable: false),
                    ItmOverUnderTransfer = table.Column<int>(type: "int", nullable: false),
                    Worker = table.Column<long>(type: "bigint", nullable: false),
                    ReasonRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseDateTzId = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventJournalTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventLocation",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationType = table.Column<int>(type: "int", nullable: false),
                    InventLocationLevel = table.Column<int>(type: "int", nullable: false),
                    InventLocationIdTransit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationIdQuarantine = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationIdReqMain = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItmInventLocationIdGit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItmInventLocationIdUnder = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VendAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaxPickingRouteVolume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxPickingRouteTime = table.Column<int>(type: "int", nullable: false),
                    PickingLineTime = table.Column<int>(type: "int", nullable: false),
                    WhsEnabled = table.Column<int>(type: "int", nullable: false),
                    WarehouseAutoReleaseReservation = table.Column<int>(type: "int", nullable: false),
                    AutoUpdateShipment = table.Column<int>(type: "int", nullable: false),
                    ReserveAtLoadPost = table.Column<int>(type: "int", nullable: false),
                    DecrementLoadLine = table.Column<int>(type: "int", nullable: false),
                    PrintBolBeforeShipConfirm = table.Column<int>(type: "int", nullable: false),
                    CycleCountAllowPalletMove = table.Column<int>(type: "int", nullable: false),
                    AllowLaborStandards = table.Column<int>(type: "int", nullable: false),
                    AllowMarkingReservationRemoval = table.Column<int>(type: "int", nullable: false),
                    LoadReleaseReservationPolicy = table.Column<int>(type: "int", nullable: false),
                    ReleaseToWarehouseRule = table.Column<int>(type: "int", nullable: false),
                    ReleaseRuleFailureOption = table.Column<int>(type: "int", nullable: false),
                    WmsRackFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WmsLevelFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WmsPositionFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UseWmsOrders = table.Column<int>(type: "int", nullable: false),
                    WmsAisleNameActive = table.Column<int>(type: "int", nullable: false),
                    WmsRackNameActive = table.Column<int>(type: "int", nullable: false),
                    WmsLevelNameActive = table.Column<int>(type: "int", nullable: false),
                    WmsPositionNameActive = table.Column<int>(type: "int", nullable: false),
                    UniqueCheckDigits = table.Column<int>(type: "int", nullable: false),
                    WmsLocationIdDefaultReceipt = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WmsLocationIdDefaultIssue = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultStatusID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EnableQualityManagement = table.Column<int>(type: "int", nullable: false),
                    RemoveInventBlockingOnStatusChange = table.Column<int>(type: "int", nullable: false),
                    DefaultProductionInputLocation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultProductionFinishGoodsLocation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultKanbanFinishedGoodsLocation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProdReserveOnlyWhse = table.Column<int>(type: "int", nullable: false),
                    WhsProdOrderBackflushMustUseReservedQty = table.Column<int>(type: "int", nullable: false),
                    InventUseDefaultProductionLocationForFormulaBom = table.Column<int>(type: "int", nullable: false),
                    WhsRawMaterialPolicy = table.Column<int>(type: "int", nullable: false),
                    RafPostingMethod = table.Column<int>(type: "int", nullable: false),
                    RboDefaultWmsLocationID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetailWmsLocationIDDefaultReturn = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultReturnCreditOnlyLocation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RejectOrderFulfillment = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetailWeightEx1 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FshStore = table.Column<int>(type: "int", nullable: false),
                    ConsolidateShipAtRtw = table.Column<int>(type: "int", nullable: false),
                    RetailInventNegPhysical = table.Column<int>(type: "int", nullable: false),
                    RetailInventNegFinancial = table.Column<int>(type: "int", nullable: false),
                    Manual = table.Column<int>(type: "int", nullable: false),
                    ReqRefill = table.Column<int>(type: "int", nullable: false),
                    EnableExternalWarehouse = table.Column<int>(type: "int", nullable: false),
                    WorkflowApproval = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventLocation", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventPosting",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventAccountType = table.Column<int>(type: "int", nullable: false),
                    ItemRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CategoryRelation = table.Column<long>(type: "bigint", nullable: false),
                    ItemCode = table.Column<int>(type: "int", nullable: false),
                    CustVendRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustVendCode = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    CostCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventPosting", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventSettlement",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettleTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransRecId = table.Column<long>(type: "bigint", nullable: false),
                    QtySettled = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountSettled = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountAdjustment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwSettled = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleType = table.Column<int>(type: "int", nullable: false),
                    SettleModel = table.Column<int>(type: "int", nullable: false),
                    BalanceSheetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OperationsLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    BalanceSheetPosting = table.Column<int>(type: "int", nullable: false),
                    OperationsPosting = table.Column<int>(type: "int", nullable: false),
                    ItmCostTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransBeginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransBeginTimeTzId = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    Posted = table.Column<int>(type: "int", nullable: false),
                    Cancelled = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventSettlement", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventSite",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DefaultInventStatusID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
                    IsReceivingWarehouseOverrideAllowed = table.Column<int>(type: "int", nullable: false),
                    TaxBranchRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_InventSite", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventSum",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WmsLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LicensePlateId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ConfigId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InventSizeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventColorId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventStyleId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventVersionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventBatchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventSerialId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventStatusId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PostedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Deducted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Picked = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Received = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Registered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Arrived = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Ordered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OnOrder = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReservPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReservOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuotationReceipt = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuotationIssue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PhysicalInvent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AvailPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AvailOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PostedValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PhysicalValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwPostedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwDeducted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwPicked = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwReceived = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwRegistered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwArrived = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwOnOrder = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwReservPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwReservOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwQuotationReceipt = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwQuotationIssue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwPhysicalInvent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwAvailPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwAvailOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventDimension10 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventDimension9 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventDimension9TzId = table.Column<int>(type: "int", nullable: false),
                    LastUpdDatePhysical = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdDateExpected = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Closed = table.Column<int>(type: "int", nullable: false),
                    ClosedQty = table.Column<int>(type: "int", nullable: false),
                    IsExcludedFromInventoryValue = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventSum", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Product = table.Column<long>(type: "bigint", nullable: false),
                    NameAlias = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    UseAltItemId = table.Column<int>(type: "int", nullable: false),
                    SortCode = table.Column<int>(type: "int", nullable: false),
                    ABCValue = table.Column<int>(type: "int", nullable: false),
                    ABCRevenue = table.Column<int>(type: "int", nullable: false),
                    ABCTieUp = table.Column<int>(type: "int", nullable: false),
                    ABCContributionMargin = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaraWeight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrossHeight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrossWidth = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrossDepth = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Height = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Depth = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Density = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitVolume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StatisticsFactor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrimaryVendorID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemBuyerGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PurchModel = table.Column<int>(type: "int", nullable: false),
                    MatchingPolicy = table.Column<int>(type: "int", nullable: false),
                    SalesPercentMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesContributionRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MarketLowestPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesModel = table.Column<int>(type: "int", nullable: false),
                    SalesPriceModelBasic = table.Column<int>(type: "int", nullable: false),
                    CostGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CostBomLevel = table.Column<int>(type: "int", nullable: false),
                    CostModel = table.Column<int>(type: "int", nullable: false),
                    ItemDimCostPrice = table.Column<int>(type: "int", nullable: false),
                    InventFiscalLifoGroup = table.Column<long>(type: "bigint", nullable: false),
                    FiscalLifoNormalValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FiscalLifoAvoidCalc = table.Column<int>(type: "int", nullable: false),
                    FiscalLifoNormalValueCalc = table.Column<int>(type: "int", nullable: false),
                    BomUnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BomCalcGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProdOriginId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProdGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BomLevel = table.Column<int>(type: "int", nullable: false),
                    ScrapConst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ScrapVar = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Phantom = table.Column<int>(type: "int", nullable: false),
                    Bundle = table.Column<int>(type: "int", nullable: false),
                    AutoReportFinished = table.Column<int>(type: "int", nullable: false),
                    BomManualReceipt = table.Column<int>(type: "int", nullable: false),
                    BomWhsReleasePolicy = table.Column<int>(type: "int", nullable: false),
                    ProdFlushingPrincip = table.Column<int>(type: "int", nullable: false),
                    BatchNumGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SerialNumGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PmfPlanningItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PmfYieldPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PmfProductType = table.Column<int>(type: "int", nullable: false),
                    BatchMergedDateCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    PdsBaseAttributeID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PdsShelfLife = table.Column<int>(type: "int", nullable: false),
                    PdsShelfAdvice = table.Column<int>(type: "int", nullable: false),
                    PdsBestBefore = table.Column<int>(type: "int", nullable: false),
                    PdsTargetFactor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsVendorCheckItem = table.Column<int>(type: "int", nullable: false),
                    PdsPotencyAttribRecording = table.Column<int>(type: "int", nullable: false),
                    WmsPalletTypeIdId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    WmsArrivalHandlingTime = table.Column<int>(type: "int", nullable: false),
                    WmsPickingQtyTime = table.Column<int>(type: "int", nullable: false),
                    QtyPerLayer = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StandardPalletQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinimumPalletQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwWmsQtyPerLayer = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwWmsStandardPalletQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwWmsMinimumPalletQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReqGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ForecastDmpInclude = table.Column<int>(type: "int", nullable: false),
                    StandardConfigId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StandardInventSizeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StandardInventColorId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StandardInventStyleId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QmsOverDispensePct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QmsUnderDispensePct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QmsCustomerCheckItem = table.Column<int>(type: "int", nullable: false),
                    QmsDispensingControl = table.Column<int>(type: "int", nullable: false),
                    QmsAuthorizedPersonnel = table.Column<int>(type: "int", nullable: false),
                    RevRecDefaultRevenueRecognitionSchedule = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RevRecMedianPriceMinimumTolerance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecMedianPriceMaximumTolerance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecRevenueRecognitionEnabled = table.Column<int>(type: "int", nullable: false),
                    RevRecRevenueType = table.Column<int>(type: "int", nullable: false),
                    RevRecBundle = table.Column<int>(type: "int", nullable: false),
                    RevRecMedianPrice = table.Column<int>(type: "int", nullable: false),
                    RevRecExcludeFromCarveOut = table.Column<int>(type: "int", nullable: false),
                    ItmOverUnderToleranceGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItmCostTypeGroupId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItmCostTransferGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItmArrivalGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IntrastatCommodity = table.Column<long>(type: "bigint", nullable: false),
                    IntrastatChargePerKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxPackagingQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxRateType = table.Column<long>(type: "bigint", nullable: false),
                    IntrastatExclude = table.Column<int>(type: "int", nullable: false),
                    CooDualUseProduct = table.Column<int>(type: "int", nullable: false),
                    IsExclusiveHbmc = table.Column<int>(type: "int", nullable: false),
                    HmimIndicator = table.Column<int>(type: "int", nullable: false),
                    ProjCategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CommissionGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_InventTable", x => x.RECID);
                    table.UniqueConstraint("AK_InventTable_ItemId", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "InventTableModule",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OverDeliveryPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnderDeliveryPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IntercompanyBlocked = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Markup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MarkupGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AllocateMarkup = table.Column<int>(type: "int", nullable: false),
                    BasePricePurchase = table.Column<int>(type: "int", nullable: false),
                    LineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndDisc = table.Column<int>(type: "int", nullable: false),
                    RetailInventoryAvailabilityBuffer = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RetailInventoryAvailabilityLevelProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PdsPricingPrecision = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventTableModule", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventTransOrigin",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemInventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCategory = table.Column<byte>(type: "tinyint", nullable: false),
                    Party = table.Column<long>(type: "bigint", nullable: false),
                    IsExcludedFromInventoryValue = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventTransOrigin", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "Ledger",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ChartOfAccounts = table.Column<long>(type: "bigint", nullable: false),
                    FiscalCalendar = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryForLegalEntity = table.Column<long>(type: "bigint", nullable: false),
                    AccountingCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ReportingCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DefaultExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    ReportingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    BudgetExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    MostRecentYearEndClose = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBudgetControlEnabled = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Ledger", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LedgerChartOfAccounts",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MainAccountFormatMask = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_LedgerChartOfAccounts", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LedgerJournalName",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NumberSequenceTable = table.Column<long>(type: "bigint", nullable: false),
                    JournalType = table.Column<int>(type: "int", nullable: false),
                    NewVoucher = table.Column<int>(type: "int", nullable: false),
                    VoucherAllocatedAtPosting = table.Column<int>(type: "int", nullable: false),
                    DetailSummary = table.Column<int>(type: "int", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetAccountType = table.Column<int>(type: "int", nullable: false),
                    FixedOffsetAccount = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FixedExchRate = table.Column<int>(type: "int", nullable: false),
                    IsAdvancedPayment = table.Column<int>(type: "int", nullable: false),
                    LedgerJournalFeePosting = table.Column<int>(type: "int", nullable: false),
                    Prepayment_W = table.Column<int>(type: "int", nullable: false),
                    ApproveGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApproveActive = table.Column<int>(type: "int", nullable: false),
                    WorkflowApproval = table.Column<int>(type: "int", nullable: false),
                    LedgerJournalInclTax = table.Column<int>(type: "int", nullable: false),
                    DelayTaxCalculation = table.Column<int>(type: "int", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    CurrentOperationsTax = table.Column<int>(type: "int", nullable: false),
                    TaxHideAmountFields = table.Column<int>(type: "int", nullable: false),
                    TaxBookTypeJournal = table.Column<int>(type: "int", nullable: false),
                    LinesLimitBeforeDistribution = table.Column<int>(type: "int", nullable: false),
                    BankTransSummarizationEnabled = table.Column<int>(type: "int", nullable: false),
                    BankTransSummarizationCriteria = table.Column<int>(type: "int", nullable: false),
                    EndBalanceControl = table.Column<int>(type: "int", nullable: false),
                    RemoveLineAfterPosting = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_LedgerJournalName", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LedgerJournalTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NumberSequenceTable = table.Column<long>(type: "bigint", nullable: false),
                    NumOfLines = table.Column<int>(type: "int", nullable: false),
                    JournalType = table.Column<int>(type: "int", nullable: false),
                    VoucherAllocatedAtPosting = table.Column<int>(type: "int", nullable: false),
                    JournalBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalTotalOffsetBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EndBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalTotalDebit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalTotalCredit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalTotalDebitReportingCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalTotalCreditReportingCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetAccountType = table.Column<int>(type: "int", nullable: false),
                    FixedOffsetAccount = table.Column<int>(type: "int", nullable: false),
                    IsLedgerDimensionNameUpdated = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FixedExchRate = table.Column<int>(type: "int", nullable: false),
                    ReportingCurrencyFixedExchRate = table.Column<int>(type: "int", nullable: false),
                    EuroTriangulation = table.Column<int>(type: "int", nullable: false),
                    PostedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedDateTimeTzId = table.Column<int>(type: "int", nullable: false),
                    Posted = table.Column<int>(type: "int", nullable: false),
                    Approver = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    SystemBlocked = table.Column<int>(type: "int", nullable: false),
                    SystemBlockedReason = table.Column<int>(type: "int", nullable: false),
                    LedgerJournalInclTax = table.Column<int>(type: "int", nullable: false),
                    DelayTaxCalculation = table.Column<int>(type: "int", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    CurrentOperationsTax = table.Column<int>(type: "int", nullable: false),
                    TaxObligationCompany = table.Column<int>(type: "int", nullable: false),
                    LinesLimitBeforeDistribution = table.Column<int>(type: "int", nullable: false),
                    DetailSummaryPosting = table.Column<int>(type: "int", nullable: false),
                    BankTransSummarizationEnabled = table.Column<int>(type: "int", nullable: false),
                    BankTransSummarizationCriteria = table.Column<int>(type: "int", nullable: false),
                    RemoveLineAfterPosting = table.Column<int>(type: "int", nullable: false),
                    OriginalJournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentJournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ReverseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReverseEntry = table.Column<int>(type: "int", nullable: false),
                    IsAdjustmentJournal = table.Column<int>(type: "int", nullable: false),
                    RetailStatementId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AssetLeaseProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    BankRemittanceType = table.Column<int>(type: "int", nullable: false),
                    CustVendNegInstProtestProcess = table.Column<int>(type: "int", nullable: false),
                    ProtestSettledBill = table.Column<int>(type: "int", nullable: false),
                    Log = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    SessionLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionLoginDateTimeTzId = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LedgerJournalTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LedgerJournalTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    LedgerDimensionName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    AmountCurDebit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountCurCredit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateSecond = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyExchRateSecondary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetDefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetFinTag = table.Column<long>(type: "bigint", nullable: false),
                    OffsetCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    OffsetAccountType = table.Column<int>(type: "int", nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OffsetTxt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DocumentNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Invoice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Due = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustTransId = table.Column<long>(type: "bigint", nullable: false),
                    VendTransId = table.Column<long>(type: "bigint", nullable: false),
                    MarkedInvoice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarkedInvoiceCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    MarkedInvoiceRecId = table.Column<long>(type: "bigint", nullable: false),
                    RemainAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SettleVoucher = table.Column<int>(type: "int", nullable: false),
                    CashDiscCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DateCashDisc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscBaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscBaseDays = table.Column<int>(type: "int", nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymSpec = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymReference = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    PaymentNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BankChequeNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BankTransType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BankDepositNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankCentralBankPurposeText = table.Column<string>(type: "nvarchar(210)", maxLength: 210, nullable: false),
                    CustVendBankAccountID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustBankAccount = table.Column<long>(type: "bigint", nullable: false),
                    VendBankAccount = table.Column<long>(type: "bigint", nullable: false),
                    DirectDebitMandate = table.Column<long>(type: "bigint", nullable: false),
                    RemittanceAddress = table.Column<long>(type: "bigint", nullable: false),
                    RemittanceLocation = table.Column<long>(type: "bigint", nullable: false),
                    BankChequeDepositTransRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    BankCurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BankDepositVoucher = table.Column<int>(type: "int", nullable: false),
                    BankReconAccountAtPost = table.Column<int>(type: "int", nullable: false),
                    BankRemittanceType = table.Column<int>(type: "int", nullable: false),
                    CustVendNegInstProtestReason = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxWithholdGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VatNumJournal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TaxBase_W = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    VatDueDate_W = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorVatDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DelayTaxCalculation = table.Column<int>(type: "int", nullable: false),
                    IntracomVatDueDate_W = table.Column<int>(type: "int", nullable: false),
                    TaxDirectionControl = table.Column<int>(type: "int", nullable: false),
                    Tax1099Fields = table.Column<long>(type: "bigint", nullable: false),
                    Tax1099RecId = table.Column<long>(type: "bigint", nullable: false),
                    Tax1099Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Tax1099StateAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ZatcaRetReason = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ZatcaRetInvoiceRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ExcludeFromZatca = table.Column<int>(type: "int", nullable: false),
                    Agz_Ksa_DebitNoteType = table.Column<int>(type: "int", nullable: false),
                    Approver = table.Column<long>(type: "bigint", nullable: false),
                    AcknowledgementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approved = table.Column<int>(type: "int", nullable: false),
                    Cancel = table.Column<int>(type: "int", nullable: false),
                    NoEdit = table.Column<int>(type: "int", nullable: false),
                    Invisible = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    TransferredBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferredTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransferredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTransferred = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Transfer = table.Column<int>(type: "int", nullable: false),
                    Transferred = table.Column<int>(type: "int", nullable: false),
                    PoolRecId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetSourceLedgerEntryUnposted = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Prepayment = table.Column<int>(type: "int", nullable: false),
                    Triangulation = table.Column<int>(type: "int", nullable: false),
                    SkipBlockedForManualEntryCheck = table.Column<int>(type: "int", nullable: false),
                    AssetLeasePostingTypes = table.Column<int>(type: "int", nullable: false),
                    AssetLeaseStatus = table.Column<int>(type: "int", nullable: false),
                    RevRecId = table.Column<long>(type: "bigint", nullable: false),
                    RevRecDeferredLine = table.Column<long>(type: "bigint", nullable: false),
                    RevRecDeferredRecognizedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecDeferredType = table.Column<int>(type: "int", nullable: false),
                    RevRecLedgerPostingType = table.Column<int>(type: "int", nullable: false),
                    RevRecNewValuesFromReallocation = table.Column<int>(type: "int", nullable: false),
                    SubBillSchedLineRecId = table.Column<long>(type: "bigint", nullable: false),
                    SubBillRenewalLineRecId = table.Column<long>(type: "bigint", nullable: false),
                    SubBillEscalationTableRecId = table.Column<long>(type: "bigint", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PurchLedgerPosting = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseDateTzId = table.Column<int>(type: "int", nullable: false),
                    InvoiceReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceReleaseDateTzId = table.Column<int>(type: "int", nullable: false),
                    ReverseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FurtherPostingRecId = table.Column<long>(type: "bigint", nullable: false),
                    ReverseEntry = table.Column<int>(type: "int", nullable: false),
                    FurtherPostingType = table.Column<int>(type: "int", nullable: false),
                    RCashDocRepresType = table.Column<int>(type: "int", nullable: false),
                    RCashPayTransType = table.Column<int>(type: "int", nullable: false),
                    NegInstId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    McrPaymOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SalesOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ForeignCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ForeignVoucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReasonRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    FileCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoadingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDate_W = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItmCostRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    CustEInvoicePaymDeliveryNum = table.Column<int>(type: "int", nullable: false),
                    CustEInvoicePaymSectionNum = table.Column<int>(type: "int", nullable: false),
                    CustEInvoicePaymTransNum = table.Column<int>(type: "int", nullable: false),
                    FreqCode = table.Column<int>(type: "int", nullable: false),
                    FreqValue = table.Column<int>(type: "int", nullable: false),
                    ListCode = table.Column<int>(type: "int", nullable: false),
                    ItmCostArea = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LedgerJournalTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsAddressCountryRegion",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsoCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    AddrFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AddressUseZipPlus4 = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
                    IsImmutable = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsAddressCountryRegion", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsAddressCountryRegion_CountryRegionId", x => x.CountryRegionId);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsAddressZipCode",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    County = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CityAlias = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DistrictName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CityRecId = table.Column<long>(type: "bigint", nullable: false),
                    District = table.Column<long>(type: "bigint", nullable: false),
                    StreetName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FromNum = table.Column<int>(type: "int", nullable: false),
                    ToNum = table.Column<int>(type: "int", nullable: false),
                    EvenOdd = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsAddressZipCode", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsAddressZipCode_ZipCode", x => x.ZipCode);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsLocation",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ParentLocation = table.Column<long>(type: "bigint", nullable: false),
                    IsPostalAddress = table.Column<int>(type: "int", nullable: false),
                    DunsNumberRecId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsLocation", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsLocationRole",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsPostalAddress = table.Column<int>(type: "int", nullable: false),
                    IsContactInfo = table.Column<int>(type: "int", nullable: false),
                    DisableAddOrEditInEmployeeSelfService = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsLocationRole", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "MainAccount",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MainAccountId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LedgerChartOfAccounts = table.Column<long>(type: "bigint", nullable: false),
                    ParentMainAccount = table.Column<long>(type: "bigint", nullable: false),
                    MainAccountTemplate = table.Column<long>(type: "bigint", nullable: false),
                    AccountCategoryRef = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeAdjustmentRateType = table.Column<long>(type: "bigint", nullable: false),
                    ReportingExchangeAdjustmentRateType = table.Column<long>(type: "bigint", nullable: false),
                    FinancialReportingExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    ExchangeAdjusted = table.Column<int>(type: "int", nullable: false),
                    Monetary = table.Column<int>(type: "int", nullable: false),
                    FinancialReportingTranslationType = table.Column<int>(type: "int", nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    UnitOfMeasure = table.Column<long>(type: "bigint", nullable: false),
                    PostingType = table.Column<int>(type: "int", nullable: false),
                    DebitCreditProposal = table.Column<int>(type: "int", nullable: false),
                    DebitCreditCheck = table.Column<int>(type: "int", nullable: false),
                    DebitCreditBalanceDemand = table.Column<int>(type: "int", nullable: false),
                    MandatoryPaymentReference = table.Column<int>(type: "int", nullable: false),
                    ValidateCurrency = table.Column<int>(type: "int", nullable: false),
                    ValidatePosting = table.Column<int>(type: "int", nullable: false),
                    ValidateUser = table.Column<int>(type: "int", nullable: false),
                    OpeningAccount = table.Column<long>(type: "bigint", nullable: false),
                    CloseType = table.Column<int>(type: "int", nullable: false),
                    Closing = table.Column<int>(type: "int", nullable: false),
                    ConsolidationMainAccount = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GroupLevel01 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GroupLevel02 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GroupLevel03 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StandardMainAccount_W = table.Column<long>(type: "bigint", nullable: false),
                    ReportingAccountType = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_MainAccount", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "MarkupTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkupCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarkupCategory = table.Column<int>(type: "int", nullable: false),
                    ModuleCategory = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    TransRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransTableId = table.Column<int>(type: "int", nullable: false),
                    OrigRecId = table.Column<long>(type: "bigint", nullable: false),
                    OrigTableId = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Posted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculatedAmountMst_W = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculatedProratedAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Keep = table.Column<int>(type: "int", nullable: false),
                    IsCompound = table.Column<int>(type: "int", nullable: false),
                    IsAutoCharge = table.Column<int>(type: "int", nullable: false),
                    IsTieredCharge = table.Column<int>(type: "int", nullable: false),
                    IsAdvancedLineProrated = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmountMst_W = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxWithholdItemGroup = table.Column<long>(type: "bigint", nullable: false),
                    TaxExemptPriceInclusiveOriginalPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxExemptPriceInclusiveReductionAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    IntercompanyRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    IntercompanyMarkupValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IntercompanyMarkupUseValue = table.Column<int>(type: "int", nullable: false),
                    McrOriginalMiscChargeValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrSavedRecId = table.Column<long>(type: "bigint", nullable: false),
                    McrSavedTableId = table.Column<int>(type: "int", nullable: false),
                    RetailShippingPromotionDiscount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrBrokerContractFee = table.Column<int>(type: "int", nullable: false),
                    McrCouponMarkup = table.Column<int>(type: "int", nullable: false),
                    McrInstallmentEligible = table.Column<int>(type: "int", nullable: false),
                    McrMiscChargeOverride = table.Column<int>(type: "int", nullable: false),
                    McrMarkupTransCreatedBy = table.Column<int>(type: "int", nullable: false),
                    CustInvoiceLineIdRef = table.Column<long>(type: "bigint", nullable: false),
                    CustInvoiceLineTemplate = table.Column<long>(type: "bigint", nullable: false),
                    VendInvoiceLineTemplate = table.Column<long>(type: "bigint", nullable: false),
                    VendInvoiceTableMarkupTrans = table.Column<long>(type: "bigint", nullable: false),
                    VendInvoiceTemplate = table.Column<long>(type: "bigint", nullable: false),
                    ItemBasePriceRecId = table.Column<long>(type: "bigint", nullable: false),
                    MarkupAutoLineRecId = table.Column<long>(type: "bigint", nullable: false),
                    MarkupAutoTableRecId = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    CorrectedMarkupTrans = table.Column<long>(type: "bigint", nullable: false),
                    DocumentStatus = table.Column<int>(type: "int", nullable: false),
                    IsModified = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    IsOverriddenLine = table.Column<int>(type: "int", nullable: false),
                    IsOverriddenProratedLine = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgAnnouncements",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhotoURL = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgAnnouncements", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "OrgDepartments",
                columns: table => new
                {
                    RECID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgDepartments", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgEmployeeCategories",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForAll = table.Column<bool>(type: "bit", nullable: true),
                    Manager1 = table.Column<bool>(type: "bit", nullable: true),
                    Manager2 = table.Column<bool>(type: "bit", nullable: true),
                    Manager3 = table.Column<bool>(type: "bit", nullable: true),
                    Manager4 = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgEmployeeCategories", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgEmployeeGroups",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgEmployeeGroups", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgGenders",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgGenders", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgManagementLevels",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false),
                    Level = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgManagementLevels", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgNationalities",
                columns: table => new
                {
                    RECID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgNationalities", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "OrgOccupations",
                columns: table => new
                {
                    RECID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgOccupations", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "PaymSched",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumOfPayment = table.Column<int>(type: "int", nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LowestAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PayBy = table.Column<int>(type: "int", nullable: false),
                    PeriodUnit = table.Column<int>(type: "int", nullable: false),
                    QtyUnit = table.Column<int>(type: "int", nullable: false),
                    TaxDistribution = table.Column<int>(type: "int", nullable: false),
                    McrMiscChargeDist = table.Column<int>(type: "int", nullable: false),
                    McrMinOrderValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrMaxOrderValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrMinNumInstallments = table.Column<int>(type: "int", nullable: false),
                    McrMaxNumInstallments = table.Column<int>(type: "int", nullable: false),
                    McrFlexiblePlan = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PaymSched", x => x.RECID);
                    table.UniqueConstraint("AK_PaymSched_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "PaymTerm",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymTermId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NumOfMonths = table.Column<int>(type: "int", nullable: false),
                    NumOfDays = table.Column<int>(type: "int", nullable: false),
                    CutOffDay = table.Column<int>(type: "int", nullable: false),
                    AdditionalMonths = table.Column<int>(type: "int", nullable: false),
                    PaymDayId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymMethod = table.Column<int>(type: "int", nullable: false),
                    CashLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    Cash = table.Column<int>(type: "int", nullable: false),
                    PostOffsettingAr = table.Column<int>(type: "int", nullable: false),
                    CreditCardPaymentType = table.Column<int>(type: "int", nullable: false),
                    CreditCardCreditCheck = table.Column<int>(type: "int", nullable: false),
                    PaymSched = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerUpdateDueDate = table.Column<int>(type: "int", nullable: false),
                    VendorUpdateDueDate = table.Column<int>(type: "int", nullable: false),
                    CfmPaymentRequestTypePayment = table.Column<long>(type: "bigint", nullable: false),
                    CfmPaymentRequestTypePrepayment = table.Column<long>(type: "bigint", nullable: false),
                    ShipCarrierCertifiedCheck = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierAncillaryCharge = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PaymTerm", x => x.RECID);
                    table.UniqueConstraint("AK_PaymTerm_PaymTermId", x => x.PaymTermId);
                });

            migrationBuilder.CreateTable(
                name: "SalesLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CustomerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerLineNum = table.Column<int>(type: "int", nullable: false),
                    Complete = table.Column<int>(type: "int", nullable: false),
                    Blocked = table.Column<int>(type: "int", nullable: false),
                    CustAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustGroupId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AddressRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    AddressRefTableId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemReplaced = table.Column<int>(type: "int", nullable: false),
                    ItemTagging = table.Column<int>(type: "int", nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    SalesUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransIdReturn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventRefType = table.Column<byte>(type: "tinyint", nullable: false),
                    InventDeliverNow = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventoryServiceAutoOffset = table.Column<int>(type: "int", nullable: false),
                    QtyOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesDeliverNow = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainSalesPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainSalesFinancial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainInventPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainInventFinancial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExpectedRetQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LinePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OverDeliveryPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnderDeliveryPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrMarginPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ConfirmedDlv = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDateConfirmed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateConfirmed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    LineDeliveryType = table.Column<int>(type: "int", nullable: false),
                    DeliveryDateControlType = table.Column<int>(type: "int", nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipCarrierDlvType = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    AccountingDistributionTemplate = table.Column<long>(type: "bigint", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    SalesStatus = table.Column<int>(type: "int", nullable: false),
                    SalesType = table.Column<int>(type: "int", nullable: false),
                    SalesSalesOrderCreationMethod = table.Column<int>(type: "int", nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesCategory = table.Column<long>(type: "bigint", nullable: false),
                    PurchOrderFormNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceLineType = table.Column<int>(type: "int", nullable: false),
                    DomExceptionType = table.Column<int>(type: "int", nullable: false),
                    Reservation = table.Column<int>(type: "int", nullable: false),
                    SoftReserveBlockLevel = table.Column<int>(type: "int", nullable: false),
                    IsSoftReservedExternally = table.Column<int>(type: "int", nullable: false),
                    ReturnAllowReservation = table.Column<int>(type: "int", nullable: false),
                    ReturnStatus = table.Column<int>(type: "int", nullable: false),
                    ReturnArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnClosedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDispositionCodeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RefReturnInvoiceTransW = table.Column<long>(type: "bigint", nullable: false),
                    RevRecBundle = table.Column<int>(type: "int", nullable: false),
                    RevRecBundleSalesStatus = table.Column<int>(type: "int", nullable: false),
                    RevRecIsBundleComponent = table.Column<int>(type: "int", nullable: false),
                    RevRecOccurrences = table.Column<int>(type: "int", nullable: false),
                    RevRecContractStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevRecContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BundleLineStatus = table.Column<int>(type: "int", nullable: false),
                    BundleLineType = table.Column<int>(type: "int", nullable: false),
                    RevRecBundleQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecBundleQtyOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecBundleSalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecBundleNetAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevRecBundleRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwExpectedRetQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwInventDeliverNow = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwRemainInventPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwRemainInventFinancial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsBatchAttribAutoRes = table.Column<int>(type: "int", nullable: false),
                    PdsExcludeFromRebate = table.Column<int>(type: "int", nullable: false),
                    PdsSameLot = table.Column<int>(type: "int", nullable: false),
                    PdsSameLotOverride = table.Column<int>(type: "int", nullable: false),
                    PlanningPriority = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MpsExcludeSalesLine = table.Column<int>(type: "int", nullable: false),
                    MpsFullRunCtpStatus = table.Column<int>(type: "int", nullable: false),
                    IntercompanyOrigin = table.Column<int>(type: "int", nullable: false),
                    MatchingAgreementLine = table.Column<long>(type: "bigint", nullable: false),
                    ManualEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    CreditNoteReasonCode = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    ProjFundingSource = table.Column<long>(type: "bigint", nullable: false),
                    IntrastatCommodity = table.Column<long>(type: "bigint", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    AgreementSkipAutoLink = table.Column<int>(type: "int", nullable: false),
                    CaseTagging = table.Column<int>(type: "int", nullable: false),
                    KittingSkipUpdateHelper = table.Column<int>(type: "int", nullable: false),
                    PalletTagging = table.Column<int>(type: "int", nullable: false),
                    Scrap = table.Column<int>(type: "int", nullable: false),
                    SourcingOrigin = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    SystemEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    SystemEntrySource = table.Column<int>(type: "int", nullable: false),
                    StAtTriangularDeal = table.Column<int>(type: "int", nullable: false),
                    TamRebateExcludeRebateManagement = table.Column<int>(type: "int", nullable: false),
                    UnbilledRevenueCredit = table.Column<int>(type: "int", nullable: false),
                    EInvoiceAccountCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PackingUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PackingUnitQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PsaProjProposalQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PsaProjProposalInventQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_SalesLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SalesPool",
                columns: table => new
                {
                    RECID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesPoolId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPool", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCreationSequenceNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CustomerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    QuotationType = table.Column<int>(type: "int", nullable: false),
                    QuotationStatus = table.Column<int>(type: "int", nullable: false),
                    QuotationLineCreationMethod = table.Column<int>(type: "int", nullable: false),
                    CustAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AddressRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    AddressRefTableId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemTagging = table.Column<int>(type: "int", nullable: false),
                    SalesUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OrigCostPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesMarkup = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NewSalesPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NewTotalContributionRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventDeliverNow = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventRefType = table.Column<byte>(type: "tinyint", nullable: false),
                    QtyOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesDeliverNow = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainSalesPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainSalesFinancial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainInventPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LinePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnDisc = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MultiLnPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OverDeliveryPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnderDeliveryPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    McrOrderLine2PriceHistoryRef = table.Column<long>(type: "bigint", nullable: false),
                    ConfirmedDlv = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LineDeliveryType = table.Column<int>(type: "int", nullable: false),
                    DeliveryDateControlType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    OffsetCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    OffsetAccountType = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    SalesCategory = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectResource = table.Column<long>(type: "bigint", nullable: false),
                    PsaRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    Transferred2Forecast = table.Column<int>(type: "int", nullable: false),
                    Transferred2ItemReq = table.Column<int>(type: "int", nullable: false),
                    Transferred2Journal = table.Column<int>(type: "int", nullable: false),
                    ProjTransType = table.Column<int>(type: "int", nullable: false),
                    PdsCwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwDeliverNow = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdsCwRemainInventPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GupFreeItemLineRecId = table.Column<long>(type: "bigint", nullable: false),
                    IsFreeItemLine = table.Column<int>(type: "int", nullable: false),
                    IntrastatCommodity = table.Column<long>(type: "bigint", nullable: false),
                    KittingSkipUpdateHelper = table.Column<int>(type: "int", nullable: false),
                    StatTriangularDeal = table.Column<int>(type: "int", nullable: false),
                    ManualEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    SystemEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    CaseTagging = table.Column<int>(type: "int", nullable: false),
                    PalletTagging = table.Column<int>(type: "int", nullable: false),
                    SystemEntrySource = table.Column<int>(type: "int", nullable: false),
                    PackingUnitQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_SalesQuotationLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotationName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ConfirmDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevision = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    QuotationType = table.Column<int>(type: "int", nullable: false),
                    QuotationStatus = table.Column<int>(type: "int", nullable: false),
                    QuotationOwnership = table.Column<int>(type: "int", nullable: false),
                    QuotationHeaderCreationMethod = table.Column<int>(type: "int", nullable: false),
                    CustAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusRelAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpportunityId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AddressRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    AddressRefTableId = table.Column<int>(type: "int", nullable: false),
                    ShippingDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeliveryDateControlType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "SAR"),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Estimate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FixedExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyFixedExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    PriceGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GupDelayPricingCalculation = table.Column<int>(type: "int", nullable: false),
                    GupSkipPricingCalculation = table.Column<int>(type: "int", nullable: false),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FixedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SettleVoucher = table.Column<int>(type: "int", nullable: false),
                    BankDocumentType = table.Column<int>(type: "int", nullable: false),
                    TaxGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VatNumRecId = table.Column<long>(type: "bigint", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    InclTax = table.Column<int>(type: "int", nullable: false),
                    VatNumTableType = table.Column<int>(type: "int", nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CovStatus = table.Column<int>(type: "int", nullable: false),
                    FreightSlipType = table.Column<int>(type: "int", nullable: false),
                    SalesPoolId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesOriginId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesIdRef = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkerSalesResponsible = table.Column<long>(type: "bigint", nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    ReasonId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QuotationExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuotationFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuotationFollowUpActivity = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ListCode = table.Column<int>(type: "int", nullable: false),
                    ProjIdRef = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjInvoiceProjId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PsaEstProjDuration = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PsaEstProjStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PsaEstProjEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PsaSchedCalendarId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ScopeOfWork = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    PsaWizardNotOk = table.Column<int>(type: "int", nullable: false),
                    TransferredToForecast = table.Column<int>(type: "int", nullable: false),
                    TransferredToItemReq = table.Column<int>(type: "int", nullable: false),
                    PsaSchedIgnoreCalendar = table.Column<int>(type: "int", nullable: false),
                    RetailChannelTable = table.Column<long>(type: "bigint", nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ManualEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    SystemEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    TemplateActive = table.Column<int>(type: "int", nullable: false),
                    Touched = table.Column<int>(type: "int", nullable: false),
                    CaseTagging = table.Column<int>(type: "int", nullable: false),
                    PalletTagging = table.Column<int>(type: "int", nullable: false),
                    ItemTagging = table.Column<int>(type: "int", nullable: false),
                    SystemEntrySource = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SalesTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SalesNameAlias = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SalesStatus = table.Column<int>(type: "int", nullable: false),
                    DocumentStatus = table.Column<int>(type: "int", nullable: false),
                    SalesType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Sales"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TaxGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymTerm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesPoolId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    OneTimeCustomer = table.Column<int>(type: "int", nullable: false),
                    CustomerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustRequisitionNum = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    WorkerSalesResponsible = table.Column<long>(type: "bigint", nullable: false),
                    WorkerSalesTaker = table.Column<long>(type: "bigint", nullable: false),
                    AddressRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    AddressRefTableId = table.Column<int>(type: "int", nullable: false),
                    DeliveryPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    ShipCarrierPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    SubBillBillToPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDateControlType = table.Column<int>(type: "int", nullable: false),
                    ShippingDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDateConfirmed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateConfirmed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashDiscBaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevRecContractStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevRecContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Estimate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscBaseDays = table.Column<int>(type: "int", nullable: false),
                    FixedExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportingCurrencyFixedExchRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SmmSalesAmountTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreditCardApprovalAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreditCardAuthorizationError = table.Column<int>(type: "int", nullable: false),
                    CreditCardCustRefId = table.Column<long>(type: "bigint", nullable: false),
                    AccountingDistributionTemplate = table.Column<long>(type: "bigint", nullable: false),
                    FundingSource = table.Column<long>(type: "bigint", nullable: false),
                    FinTag = table.Column<long>(type: "bigint", nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    VatNumRecId = table.Column<long>(type: "bigint", nullable: false),
                    VatNumTableType = table.Column<int>(type: "int", nullable: false),
                    CovStatus = table.Column<int>(type: "int", nullable: false),
                    ReleaseStatus = table.Column<int>(type: "int", nullable: false),
                    Reservation = table.Column<int>(type: "int", nullable: false),
                    ReturnStatus = table.Column<int>(type: "int", nullable: false),
                    ReturnReplacementCreated = table.Column<bool>(type: "bit", nullable: false),
                    CredManExcludeSalesOrder = table.Column<bool>(type: "bit", nullable: false),
                    CredManInCreditControl = table.Column<bool>(type: "bit", nullable: false),
                    CredManRejected = table.Column<bool>(type: "bit", nullable: false),
                    CredManReleasedFromCreditControl = table.Column<bool>(type: "bit", nullable: false),
                    CreditNoteReasonCode = table.Column<long>(type: "bigint", nullable: false),
                    IntercompanyOrder = table.Column<bool>(type: "bit", nullable: false),
                    IntercompanyAutoCreateOrders = table.Column<bool>(type: "bit", nullable: false),
                    IntercompanyDirectDelivery = table.Column<bool>(type: "bit", nullable: false),
                    IntercompanyDirectDeliveryOrig = table.Column<bool>(type: "bit", nullable: false),
                    IntercompanyAllowIndirectCreation = table.Column<bool>(type: "bit", nullable: false),
                    IntercompanyAllowIndirectCreationOrig = table.Column<bool>(type: "bit", nullable: false),
                    IntercompanyOrigin = table.Column<int>(type: "int", nullable: false),
                    InclTax = table.Column<bool>(type: "bit", nullable: false),
                    GiroType = table.Column<int>(type: "int", nullable: false),
                    FreightSlipType = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierDlvType = table.Column<int>(type: "int", nullable: false),
                    ShipCarrierResidential = table.Column<bool>(type: "bit", nullable: false),
                    ShipCarrierBlindShipment = table.Column<int>(type: "int", nullable: false),
                    RevRecFollowOriginalPricingMethod = table.Column<bool>(type: "bit", nullable: false),
                    RevRecMultipleSoReallocation = table.Column<bool>(type: "bit", nullable: false),
                    RevRecLatestReverseJournal = table.Column<long>(type: "bigint", nullable: false),
                    CaseTagging = table.Column<int>(type: "int", nullable: false),
                    ItemTagging = table.Column<int>(type: "int", nullable: false),
                    PalletTagging = table.Column<int>(type: "int", nullable: false),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: false),
                    TransportationDocument = table.Column<long>(type: "bigint", nullable: false),
                    RetailChannelTable = table.Column<long>(type: "bigint", nullable: false),
                    ServiceCodeRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    DirectDebitMandate = table.Column<long>(type: "bigint", nullable: false),
                    ManualEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    SystemEntryChangePolicy = table.Column<long>(type: "bigint", nullable: false),
                    SystemEntrySource = table.Column<int>(type: "int", nullable: false),
                    MatchingAgreement = table.Column<long>(type: "bigint", nullable: false),
                    CreatedTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    BankDocumentType = table.Column<int>(type: "int", nullable: false),
                    ListCode = table.Column<int>(type: "int", nullable: false),
                    McrOrderStopped = table.Column<bool>(type: "bit", nullable: false),
                    MpsExcludeSalesOrder = table.Column<bool>(type: "bit", nullable: false),
                    MpsFullRunCtpStatus = table.Column<int>(type: "int", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    PdsBatchAttribAutoRes = table.Column<int>(type: "int", nullable: false),
                    SettleVoucher = table.Column<int>(type: "int", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReturnItemNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReturnReasonCodeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CredManId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchOrderFormNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    ReturnNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    DomProcessedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevRecReallocationId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SmmCampaignId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TamRebateReference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TamDeductionId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PdsCustRebateGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PdsRebateProgramTmaGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SubBillCreatedFromSb = table.Column<int>(type: "int", nullable: false),
                    SubBillBillToName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SubBillSuppressChild = table.Column<int>(type: "int", nullable: false),
                    EInvoiceLineSpec = table.Column<int>(type: "int", nullable: false),
                    EInvoiceAccountCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EnterpriseNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExportReason = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StatProcId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AutoSummaryModuleType = table.Column<int>(type: "int", nullable: false),
                    NumberSequenceGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GupDelayPricingCalculation = table.Column<int>(type: "int", nullable: false),
                    GupSkipPricingCalculation = table.Column<int>(type: "int", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    AsohOrderClass = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SalesOriginId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CashDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IntercompanyCompanyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ProjId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IntercompanyPurchId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentSched = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IntercompanyOriginalCustAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IntercompanyOriginalSalesId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReturnReplacementId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreditCardAuthorization = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymSpec = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MarkupGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MultiLineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ContactPersonId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustInvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CommissionGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalesUnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountyOrigDest = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DlvReason = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FreightZone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Port = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipCarrierAccount = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipCarrierAccountCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipCarrierDeliveryContact = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ShipCarrierId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipCarrierName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Transport = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FixedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DomIgnore = table.Column<bool>(type: "bit", nullable: false),
                    DomProcessed = table.Column<bool>(type: "bit", nullable: false),
                    DomExceptionType = table.Column<int>(type: "int", nullable: false),
                    DomIterations = table.Column<int>(type: "int", nullable: false),
                    DomProcessedDateTimeTZID = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTable", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SpecTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RefRecId = table.Column<long>(type: "bigint", nullable: false),
                    RefTableId = table.Column<int>(type: "int", nullable: false),
                    RefCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    SpecRecId = table.Column<long>(type: "bigint", nullable: false),
                    SpecTableId = table.Column<int>(type: "int", nullable: false),
                    SpecCompany = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Balance01 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CashDiscToTake = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CrossRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SelectedDateUsedToCalcCashDisc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FullSettlement = table.Column<int>(type: "int", nullable: false),
                    Payment = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SpecTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysAuditLogs",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysAuditLogs", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "SysBackgroundJobs",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ScheduleType = table.Column<int>(type: "int", nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: true),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreventOverlap = table.Column<bool>(type: "bit", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    RetryDelaySeconds = table.Column<int>(type: "int", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RunCount = table.Column<int>(type: "int", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStatus = table.Column<int>(type: "int", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SysBackgroundJobs", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysChatMessages",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_SysChatMessages", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysChatReadStates",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_SysChatReadStates", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysDataSeedLogs",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysDataSeedLogs", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "SysExceptionLogs",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientIpMasked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpanId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControllerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Server = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestBodyPreview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestContentLength = table.Column<long>(type: "bigint", nullable: true),
                    ElapsedMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysExceptionLogs", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "SysNotificationPreferences",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnableInApp = table.Column<bool>(type: "bit", nullable: false),
                    EnableEmail = table.Column<bool>(type: "bit", nullable: false),
                    EnableSms = table.Column<bool>(type: "bit", nullable: false),
                    EnablePush = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysNotificationPreferences", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "SysNotificationTemplates",
                columns: table => new
                {
                    RECID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SubjectAR = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyAR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variables = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultPriority = table.Column<int>(type: "int", nullable: false),
                    DefaultCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultChannel = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_SysNotificationTemplates", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysNumberSequences",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumberSequence = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LatestCleanDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestCleanDateTimeTzId = table.Column<int>(type: "int", nullable: true),
                    Lowest = table.Column<int>(type: "int", nullable: true),
                    Highest = table.Column<int>(type: "int", nullable: true),
                    NextRec = table.Column<int>(type: "int", nullable: true),
                    Blocked = table.Column<int>(type: "int", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Continuous = table.Column<int>(type: "int", nullable: true),
                    Cyclic = table.Column<int>(type: "int", nullable: true),
                    AnnotatedFormat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CleanAtAccess = table.Column<int>(type: "int", nullable: true),
                    InUse = table.Column<int>(type: "int", nullable: true),
                    NoIncrement = table.Column<int>(type: "int", nullable: true),
                    NumberSequenceScope = table.Column<long>(type: "bigint", nullable: true),
                    CleanInterval = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AllowChangeUp = table.Column<int>(type: "int", nullable: true),
                    AllowChangeDown = table.Column<int>(type: "int", nullable: true),
                    Manual = table.Column<int>(type: "int", nullable: true),
                    FetchAheadQty = table.Column<int>(type: "int", nullable: true),
                    FetchAhead = table.Column<int>(type: "int", nullable: true),
                    ModifiedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    Partition = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_SysNumberSequences", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysScheduledNotifications",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    SendAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RecipientUserIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TemplatePlaceholdersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalNotificationId = table.Column<long>(type: "bigint", nullable: true),
                    EscalationUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RecurringIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxOccurrences = table.Column<int>(type: "int", nullable: false),
                    CurrentOccurrence = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysScheduledNotifications", x => x.RecId);
                });

            migrationBuilder.CreateTable(
                name: "SysSettings",
                columns: table => new
                {
                    RECID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DefaultLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnableAuditLog = table.Column<bool>(type: "bit", nullable: false),
                    MaxUploadSize = table.Column<long>(type: "bigint", nullable: false),
                    PaginationSize = table.Column<int>(type: "int", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SysSettings", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "TaxExemptCodeTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExemptCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("PK_TaxExemptCodeTable", x => x.RECID);
                    table.UniqueConstraint("AK_TaxExemptCodeTable_ExemptCode", x => x.ExemptCode);
                });

            migrationBuilder.CreateTable(
                name: "TaxGroupHeading",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxGroupName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TaxGroupSetup = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    TaxGroupRounding = table.Column<int>(type: "int", nullable: false),
                    TaxReverseOnCashDisc = table.Column<int>(type: "int", nullable: false),
                    EuTrade_W = table.Column<int>(type: "int", nullable: false),
                    MandatorySalesDate_W = table.Column<int>(type: "int", nullable: false),
                    FillSalesDate_W = table.Column<int>(type: "int", nullable: false),
                    FillVatDueDatePeriodNumber = table.Column<int>(type: "int", nullable: false),
                    FillVatDueDate_W = table.Column<int>(type: "int", nullable: false),
                    FillVatDueDateBasedOn = table.Column<int>(type: "int", nullable: false),
                    FillVatDueDatePeriod = table.Column<int>(type: "int", nullable: false),
                    TaxPrintDetail = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxGroupHeading", x => x.RECID);
                    table.UniqueConstraint("AK_TaxGroupHeading_TaxGroup", x => x.TaxGroup);
                });

            migrationBuilder.CreateTable(
                name: "TaxItemGroupHeading",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    EuSalesListType = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxItemGroupHeading", x => x.RECID);
                    table.UniqueConstraint("AK_TaxItemGroupHeading_TaxItemGroup", x => x.TaxItemGroup);
                });

            migrationBuilder.CreateTable(
                name: "TaxJournalTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransTableId = table.Column<int>(type: "int", nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SourceBaseAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceTaxAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceRegulateAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxBaseAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxBaseQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxDirection = table.Column<int>(type: "int", nullable: false),
                    TaxOrigin = table.Column<int>(type: "int", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    ExemptCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExemptTax = table.Column<int>(type: "int", nullable: false),
                    EuroTriangulation = table.Column<int>(type: "int", nullable: false),
                    PrintCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OperationLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    TaxPrintDetail = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxJournalTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "TaxTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceRecId = table.Column<long>(type: "bigint", nullable: false),
                    SourceTableId = table.Column<int>(type: "int", nullable: false),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: false),
                    HeadingTableId = table.Column<int>(type: "int", nullable: false),
                    SourceCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SourceBaseAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceBaseAmountCurRegulated = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceTaxAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceRegulateAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TaxBaseAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxBaseAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxBaseAmountRep = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmountRep = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxBaseQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxInCostPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxInCostPriceCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxInCostPriceMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxInCostPriceRep = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxInCostPriceRegulated = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAccountType = table.Column<int>(type: "int", nullable: false),
                    TaxDirection = table.Column<int>(type: "int", nullable: false),
                    TaxOrigin = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    TaxAutoGenerated = table.Column<int>(type: "int", nullable: false),
                    ExemptCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VatExemptPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RealizedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExemptTax = table.Column<int>(type: "int", nullable: false),
                    UnrealizedTax = table.Column<int>(type: "int", nullable: false),
                    UnrealizedTaxExt = table.Column<int>(type: "int", nullable: false),
                    PostponeVat = table.Column<int>(type: "int", nullable: false),
                    PrintCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxJurisdictionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    PartyTaxId = table.Column<long>(type: "bigint", nullable: false),
                    TaxBook = table.Column<long>(type: "bigint", nullable: false),
                    TaxBookSection = table.Column<long>(type: "bigint", nullable: false),
                    TaxRepCounter = table.Column<int>(type: "int", nullable: false),
                    ReverseCharge_W = table.Column<int>(type: "int", nullable: false),
                    EuroTriangulation = table.Column<int>(type: "int", nullable: false),
                    EmptyTaxBaseForOutgoingTax_W = table.Column<int>(type: "int", nullable: false),
                    TaxObligationCompany = table.Column<int>(type: "int", nullable: false),
                    TaxPrintDetail = table.Column<int>(type: "int", nullable: false),
                    OrigTaxTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    TaxTransRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    ExchRateDiffOrigRecId = table.Column<long>(type: "bigint", nullable: false),
                    IsvFeatureSetupGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    IsOverUnderpayment = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxTrans", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasure",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    UnitOfMeasureClass = table.Column<int>(type: "int", nullable: false),
                    SystemOfUnits = table.Column<int>(type: "int", nullable: false),
                    DecimalPrecision = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_UnitOfMeasure", x => x.RECID);
                    table.UniqueConstraint("AK_UnitOfMeasure_Symbol", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "WfActivityTypes",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfActivityTypes", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfCategories",
                columns: table => new
                {
                    RECID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SysField = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfCategories", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfControls",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ControlType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfControls", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfDataTypes",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfDataTypes", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfOperators",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfOperators", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfPerformerType",
                columns: table => new
                {
                    RECID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfPerformerType", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfPriorities",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfPriorities", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfProcessTypes",
                columns: table => new
                {
                    RECID = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfProcessTypes", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestDetails",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<long>(type: "bigint", nullable: true),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    ControlId = table.Column<byte>(type: "tinyint", nullable: true),
                    ControlDataId = table.Column<long>(type: "bigint", nullable: true),
                    ControlLabel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlLabelAR = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlValueAR = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ControlValueEN = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UsedAsCriteria = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_WfRequestDetails", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_AspNetRolePermissions_AspNetPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AspNetPermissions",
                        principalColumn: "RecId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetRolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustLedgerAccounts",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountCode = table.Column<int>(type: "int", nullable: false),
                    Num = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CollectionLetterCourse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClearingLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DepositLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    EndorseLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    ExportSalesLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    LiabilitiesForDiscountLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    SummaryLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    VatPrepaymentsLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    WriteOffLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    CustInterest = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustLedgerAccounts", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_CustLedgerAccounts_CustLedger_PostingProfile",
                        column: x => x.PostingProfile,
                        principalTable: "CustLedger",
                        principalColumn: "PostingProfile");
                });

            migrationBuilder.CreateTable(
                name: "MarkupTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkupCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    CustType = table.Column<int>(type: "int", nullable: false),
                    CustPosting = table.Column<int>(type: "int", nullable: false),
                    CustomerLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    VendType = table.Column<int>(type: "int", nullable: false),
                    VendPosting = table.Column<int>(type: "int", nullable: false),
                    VendorLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UseInMatching = table.Column<int>(type: "int", nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxRateType = table.Column<long>(type: "bigint", nullable: false),
                    TaxWithholdItemGroup = table.Column<long>(type: "bigint", nullable: false),
                    IncludeIntoIntrastatInvoiceValue = table.Column<int>(type: "int", nullable: false),
                    IncludeIntoIntrastatStatisticalValue = table.Column<int>(type: "int", nullable: false),
                    IsShipping = table.Column<int>(type: "int", nullable: false),
                    Refundable = table.Column<int>(type: "int", nullable: false),
                    McrProrate = table.Column<int>(type: "int", nullable: false),
                    McrBrokerContractFee = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_MarkupTable", x => x.RECID);
                    table.UniqueConstraint("AK_MarkupTable_MarkupCode", x => x.MarkupCode);
                    table.ForeignKey(
                        name: "FK_MarkupTable_DimensionAttributeValueCombination_CustomerLedgerDimension",
                        column: x => x.CustomerLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarkupTable_DimensionAttributeValueCombination_VendorLedgerDimension",
                        column: x => x.VendorLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxLedgerAccountGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxAccountGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TaxOutgoingLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxIncomingLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxReportLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxUseTaxLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxOffsetUseTaxLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxReverseOffsetIncLedgerDimension_W = table.Column<long>(type: "bigint", nullable: true),
                    TaxReverseOffsetOutLedgerDimension_W = table.Column<long>(type: "bigint", nullable: true),
                    TaxNonDeductibleTaxLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxFreePercentLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxInterimTransitLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxUnrealizedPayablesLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxUnrealizedReceivablesLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    CashDiscountIncomingLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    CashDiscountOutgoingLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxIncomingDifferenceLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxIncomingDiffOffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxOutgoingDifferenceLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxOutgoingDiffOffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    PennyDifferenceCustomerLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    PennyDifferenceVendorLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_TaxLedgerAccountGroup", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_TaxLedgerAccountGroup_DimensionAttributeValueCombination_TaxIncomingLedgerDimension",
                        column: x => x.TaxIncomingLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_TaxLedgerAccountGroup_DimensionAttributeValueCombination_TaxOutgoingLedgerDimension",
                        column: x => x.TaxOutgoingLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_TaxLedgerAccountGroup_DimensionAttributeValueCombination_TaxReportLedgerDimension",
                        column: x => x.TaxReportLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID");
                });

            migrationBuilder.CreateTable(
                name: "DocuRef",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    REFTABLEID = table.Column<int>(type: "int", nullable: false),
                    REFRECID = table.Column<long>(type: "bigint", nullable: false),
                    REFCOMPANYID = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ACTUALCOMPANYID = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    AUTHOR = table.Column<long>(type: "bigint", nullable: false),
                    PARTY = table.Column<long>(type: "bigint", nullable: false),
                    TYPEID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    VALUERECID = table.Column<long>(type: "bigint", nullable: false),
                    NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NOTES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RESTRICTION = table.Column<int>(type: "int", nullable: false),
                    SMMEMAILENTRYID = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: false),
                    SMMEMAILSTOREID = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    SMMTABLE = table.Column<int>(type: "int", nullable: false),
                    ENCYCLOPEDIAITEMID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CONTACTPERSONID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PARTITION = table.Column<long>(type: "bigint", nullable: false),
                    ISJUSTIFICATION = table.Column<int>(type: "int", nullable: false),
                    DOCUMENTID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DEFAULTATTACHMENT = table.Column<int>(type: "int", nullable: false),
                    ENGCHGENGINEERINGREFERENCE = table.Column<string>(type: "nvarchar(72)", maxLength: 72, nullable: false),
                    ENGCHGENGINEERINGDOCUMENT = table.Column<long>(type: "bigint", nullable: false),
                    ISENABLEDFORVIRTUALENTITYSYNC = table.Column<int>(type: "int", nullable: false),
                    CREATEDBY = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CREATEDDATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    MODIFIEDBY = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    MODIFIEDDATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SYSROWVERSION = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RECVERSION = table.Column<int>(type: "int", nullable: false),
                    DATAAREAID = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocuRef", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DocuRef_DocuType_TYPEID",
                        column: x => x.TYPEID,
                        principalTable: "DocuType",
                        principalColumn: "TYPEID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocuRef_DocuValue_VALUERECID",
                        column: x => x.VALUERECID,
                        principalTable: "DocuValue",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRateCurrencyPair",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ToCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRateType = table.Column<long>(type: "bigint", nullable: false),
                    ExchangeRateDisplayFactor = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ExchangeRateCurrencyPair", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_ExchangeRateCurrencyPair_Currency_FromCurrencyCode",
                        column: x => x.FromCurrencyCode,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeRateCurrencyPair_Currency_ToCurrencyCode",
                        column: x => x.ToCurrencyCode,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeRateCurrencyPair_ExchangeRateType_ExchangeRateType",
                        column: x => x.ExchangeRateType,
                        principalTable: "ExchangeRateType",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FiscalCalendarYear",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalCalendar = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_FiscalCalendarYear", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_FiscalCalendarYear_FiscalCalendar_FiscalCalendar",
                        column: x => x.FiscalCalendar,
                        principalTable: "FiscalCalendar",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    InventDimFixed = table.Column<int>(type: "int", nullable: false),
                    StatusIssue = table.Column<byte>(type: "tinyint", nullable: false),
                    StatusReceipt = table.Column<byte>(type: "tinyint", nullable: false),
                    ValueOpen = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QtySettled = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PdscwSettled = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CostAmountPosted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountAdjustment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountSettled = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountStd = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostAmountOperations = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevenueAmountPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmountPhysical = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherPhysical = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PackingSlipId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceReturned = table.Column<int>(type: "int", nullable: false),
                    PackingSlipReturned = table.Column<int>(type: "int", nullable: false),
                    DateStatus = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatePhysical = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFinancial = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateClosed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateExpected = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeExpected = table.Column<int>(type: "int", nullable: false),
                    DateInvent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDateConfirmed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjCategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProjAdjustRefId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickingRouteId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransChildRefId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransChildType = table.Column<int>(type: "int", nullable: false),
                    IntercompanyInventDimTransferred = table.Column<int>(type: "int", nullable: false),
                    MarkingRefInventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    ReturnInventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    NonFinancialTransferInventClosing = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_InventTrans", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_InventTrans_InventTable_ItemId",
                        column: x => x.ItemId,
                        principalTable: "InventTable",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsAddressState",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultStateForCountryRegion = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsAddressState", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsAddressState_CountryRegionId_StateId", x => new { x.CountryRegionId, x.StateId });
                    table.ForeignKey(
                        name: "FK_LogisticsAddressState_LogisticsAddressCountryRegion_CountryRegionId",
                        column: x => x.CountryRegionId,
                        principalTable: "LogisticsAddressCountryRegion",
                        principalColumn: "CountryRegionId");
                });

            migrationBuilder.CreateTable(
                name: "OrgEntities",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<short>(type: "smallint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PartyType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgEntities", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_OrgEntities_OrgDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrgDepartments",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrgEmployeeCategoryGroups",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCategoriesID = table.Column<long>(type: "bigint", nullable: false),
                    DepartmentID = table.Column<short>(type: "smallint", nullable: true),
                    OccupationID = table.Column<short>(type: "smallint", nullable: true),
                    UserGroupID = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgEmployeeCategoryGroups", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_OrgEmployeeCategoryGroups_OrgDepartments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "OrgDepartments",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_OrgEmployeeCategoryGroups_OrgEmployeeCategories_UserCategoriesID",
                        column: x => x.UserCategoriesID,
                        principalTable: "OrgEmployeeCategories",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrgEmployeeCategoryGroups_OrgEmployeeGroups_UserGroupID",
                        column: x => x.UserGroupID,
                        principalTable: "OrgEmployeeGroups",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_OrgEmployeeCategoryGroups_OrgOccupations_OccupationID",
                        column: x => x.OccupationID,
                        principalTable: "OrgOccupations",
                        principalColumn: "RECID");
                });

            migrationBuilder.CreateTable(
                name: "PaymSchedLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PercentAmount = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    CfmPrepayment = table.Column<int>(type: "int", nullable: false),
                    McrShipping = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PaymSchedLine", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_PaymSchedLine_PaymSched_Name",
                        column: x => x.Name,
                        principalTable: "PaymSched",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateTable(
                name: "VendGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendGroupCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PaymTermId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClearingPeriod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: true),
                    AccountingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: true),
                    ReportingCurrencyExchangeRateType = table.Column<long>(type: "bigint", nullable: true),
                    VendAccountNumSeq = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_VendGroup", x => x.RECID);
                    table.UniqueConstraint("AK_VendGroup_VendGroupCode", x => x.VendGroupCode);
                    table.ForeignKey(
                        name: "FK_VendGroup_ExchangeRateType_AccountingCurrencyExchangeRateType",
                        column: x => x.AccountingCurrencyExchangeRateType,
                        principalTable: "ExchangeRateType",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_VendGroup_ExchangeRateType_ReportingCurrencyExchangeRateType",
                        column: x => x.ReportingCurrencyExchangeRateType,
                        principalTable: "ExchangeRateType",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_VendGroup_PaymTerm_PaymTermId",
                        column: x => x.PaymTermId,
                        principalTable: "PaymTerm",
                        principalColumn: "PaymTermId");
                });

            migrationBuilder.CreateTable(
                name: "SysBackgroundJobExecutions",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    Attempt = table.Column<int>(type: "int", nullable: false),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    TriggeredByUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysBackgroundJobExecutions", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_SysBackgroundJobExecutions_SysBackgroundJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "SysBackgroundJobs",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SysNotifications",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SysNotifications", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SysNotifications_SysNotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "SysNotificationTemplates",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TaxTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TaxPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxAccountGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    TaxOnTax = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxUnit = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TaxBase = table.Column<int>(type: "int", nullable: false),
                    TaxCalcMethod = table.Column<int>(type: "int", nullable: false),
                    TaxLimitBase = table.Column<int>(type: "int", nullable: false),
                    TaxIncludeInTax = table.Column<int>(type: "int", nullable: false),
                    NegativeTax = table.Column<int>(type: "int", nullable: false),
                    UnrealizedTax = table.Column<int>(type: "int", nullable: false),
                    TaxAllowLineDiscountOnTaxPerUnit = table.Column<int>(type: "int", nullable: false),
                    TaxRoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxRoundOffType = table.Column<int>(type: "int", nullable: false),
                    RoundDeductibleFirst = table.Column<int>(type: "int", nullable: false),
                    PrintCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentTaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxJurisdictionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxType_W = table.Column<int>(type: "int", nullable: false),
                    TaxCountryRegionType = table.Column<int>(type: "int", nullable: false),
                    NotEuSalesList = table.Column<int>(type: "int", nullable: false),
                    ExcludeFromInvoice = table.Column<int>(type: "int", nullable: false),
                    TaxPurchaseTax = table.Column<int>(type: "int", nullable: false),
                    TaxPackagingTax = table.Column<int>(type: "int", nullable: false),
                    TaxWriteSelection = table.Column<int>(type: "int", nullable: false),
                    ReconcileAmountOrigin = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseOutgoing = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseOutgoingCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxOutgoing = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxOutgoingCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseIncoming = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseIncomingCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxIncoming = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxIncomingCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseUseTax = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseUseTaxCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldUseTax = table.Column<int>(type: "int", nullable: false),
                    RepFieldUseTaxCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseUseTaxOffset = table.Column<int>(type: "int", nullable: false),
                    RepFieldBaseUseTaxOffsetCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldUseTaxOffset = table.Column<int>(type: "int", nullable: false),
                    RepFieldUseTaxOffsetCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxFreeSales = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxFreeSalesCreditNote = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxFreeBuy = table.Column<int>(type: "int", nullable: false),
                    RepFieldTaxFreeBuyCreditNote = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxTable", x => x.RECID);
                    table.UniqueConstraint("AK_TaxTable_TaxCode", x => x.TaxCode);
                    table.ForeignKey(
                        name: "FK_TaxTable_Currency_TaxCurrencyCode",
                        column: x => x.TaxCurrencyCode,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxTable_UnitOfMeasure_TaxUnit",
                        column: x => x.TaxUnit,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfPerformers",
                columns: table => new
                {
                    PerformerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformerType = table.Column<short>(type: "smallint", nullable: false),
                    RelatedField = table.Column<long>(type: "bigint", nullable: true),
                    IsApplicant = table.Column<bool>(type: "bit", nullable: false),
                    IsEmployee = table.Column<bool>(type: "bit", nullable: false),
                    IsManager1 = table.Column<bool>(type: "bit", nullable: false),
                    IsManager2 = table.Column<bool>(type: "bit", nullable: false),
                    IsManager3 = table.Column<bool>(type: "bit", nullable: false),
                    IsManager4 = table.Column<bool>(type: "bit", nullable: false),
                    SqlTable = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SqlField = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SqlWhere = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Activated = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformerName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfPerformers", x => x.PerformerId);
                    table.ForeignKey(
                        name: "FK_WfPerformers_WfPerformerType_PerformerType",
                        column: x => x.PerformerType,
                        principalTable: "WfPerformerType",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfProcesses",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<short>(type: "smallint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CanRepeat = table.Column<bool>(type: "bit", nullable: false),
                    MandatoryDocs = table.Column<bool>(type: "bit", nullable: false),
                    PriorityId = table.Column<byte>(type: "tinyint", nullable: false),
                    ProcessTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    SysField = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfProcesses", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_WfProcesses_WfCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "WfCategories",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfProcesses_WfPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "WfPriorities",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRate",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExchangeRateValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchangeRateCurrencyPair = table.Column<long>(type: "bigint", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_ExchangeRate", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_ExchangeRate_ExchangeRateCurrencyPair_ExchangeRateCurrencyPair",
                        column: x => x.ExchangeRateCurrencyPair,
                        principalTable: "ExchangeRateCurrencyPair",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FiscalCalendarPeriod",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalCalendar = table.Column<long>(type: "bigint", nullable: false),
                    FiscalCalendarYear = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_FiscalCalendarPeriod", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_FiscalCalendarPeriod_FiscalCalendarYear_FiscalCalendarYear",
                        column: x => x.FiscalCalendarYear,
                        principalTable: "FiscalCalendarYear",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FiscalCalendarPeriod_FiscalCalendar_FiscalCalendar",
                        column: x => x.FiscalCalendar,
                        principalTable: "FiscalCalendar",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LedgerFiscalCalendarYear",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ledger = table.Column<long>(type: "bigint", nullable: false),
                    FiscalCalendarYear = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_LedgerFiscalCalendarYear", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_LedgerFiscalCalendarYear_FiscalCalendarYear_FiscalCalendarYear",
                        column: x => x.FiscalCalendarYear,
                        principalTable: "FiscalCalendarYear",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerFiscalCalendarYear_Ledger_Ledger",
                        column: x => x.Ledger,
                        principalTable: "Ledger",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsAddressCounty",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountyId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_LogisticsAddressCounty", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsAddressCounty_CountryRegionId_StateId_CountyId", x => new { x.CountryRegionId, x.StateId, x.CountyId });
                    table.ForeignKey(
                        name: "FK_LogisticsAddressCounty_LogisticsAddressCountryRegion_CountryRegionId",
                        column: x => x.CountryRegionId,
                        principalTable: "LogisticsAddressCountryRegion",
                        principalColumn: "CountryRegionId");
                    table.ForeignKey(
                        name: "FK_LogisticsAddressCounty_LogisticsAddressState_CountryRegionId_StateId",
                        columns: x => new { x.CountryRegionId, x.StateId },
                        principalTable: "LogisticsAddressState",
                        principalColumns: new[] { "CountryRegionId", "StateId" });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    LastLockoutDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    AccountExpirationDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrgEntityId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_OrgEntities_OrgEntityId",
                        column: x => x.OrgEntityId,
                        principalTable: "OrgEntities",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SysNotificationAuditLogs",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysNotificationAuditLogs", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_SysNotificationAuditLogs_SysNotifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "SysNotifications",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxData",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxFromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaxToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    VatExemptPct = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxLimitMin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxLimitMax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxSubstitutionMarkupValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_TaxData", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_TaxData_TaxTable_TaxCode",
                        column: x => x.TaxCode,
                        principalTable: "TaxTable",
                        principalColumn: "TaxCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxGroupData",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxExemptCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExemptTax = table.Column<int>(type: "int", nullable: false),
                    UseTax = table.Column<int>(type: "int", nullable: false),
                    IntracomVat = table.Column<int>(type: "int", nullable: false),
                    ReverseCharge_W = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxGroupData", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_TaxGroupData_TaxExemptCodeTable_TaxExemptCode",
                        column: x => x.TaxExemptCode,
                        principalTable: "TaxExemptCodeTable",
                        principalColumn: "ExemptCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxGroupData_TaxGroupHeading_TaxGroup",
                        column: x => x.TaxGroup,
                        principalTable: "TaxGroupHeading",
                        principalColumn: "TaxGroup",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxGroupData_TaxTable_TaxCode",
                        column: x => x.TaxCode,
                        principalTable: "TaxTable",
                        principalColumn: "TaxCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxOnItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxExemptCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_TaxOnItem", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_TaxOnItem_TaxExemptCodeTable_TaxExemptCode",
                        column: x => x.TaxExemptCode,
                        principalTable: "TaxExemptCodeTable",
                        principalColumn: "ExemptCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxOnItem_TaxItemGroupHeading_TaxItemGroup",
                        column: x => x.TaxItemGroup,
                        principalTable: "TaxItemGroupHeading",
                        principalColumn: "TaxItemGroup",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxOnItem_TaxTable_TaxCode",
                        column: x => x.TaxCode,
                        principalTable: "TaxTable",
                        principalColumn: "TaxCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfUsersPerformers",
                columns: table => new
                {
                    UsersPerformerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformerID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    RelatedField = table.Column<long>(type: "bigint", nullable: false),
                    ExtendedProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfUsersPerformers", x => x.UsersPerformerId);
                    table.ForeignKey(
                        name: "FK_WfUsersPerformers_WfPerformers_PerformerID",
                        column: x => x.PerformerID,
                        principalTable: "WfPerformers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestControls",
                columns: table => new
                {
                    RequestControlId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    ControlId = table.Column<byte>(type: "tinyint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtendedProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfRequestControls", x => x.RequestControlId);
                    table.ForeignKey(
                        name: "FK_WfRequestControls_WfControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "WfControls",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfRequestControls_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfSteps",
                columns: table => new
                {
                    StepId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AutoPassingHrs = table.Column<byte>(type: "tinyint", nullable: false),
                    AllMandatory = table.Column<bool>(type: "bit", nullable: false),
                    SysField = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfSteps", x => x.StepId);
                    table.ForeignKey(
                        name: "FK_WfSteps_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfVariables",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfVariables", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_WfVariables_WfDataTypes_DataTypeId",
                        column: x => x.DataTypeId,
                        principalTable: "WfDataTypes",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfVariables_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LedgerFiscalCalendarPeriod",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ledger = table.Column<long>(type: "bigint", nullable: false),
                    FiscalCalendarPeriod = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_LedgerFiscalCalendarPeriod", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_LedgerFiscalCalendarPeriod_FiscalCalendarPeriod_FiscalCalendarPeriod",
                        column: x => x.FiscalCalendarPeriod,
                        principalTable: "FiscalCalendarPeriod",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerFiscalCalendarPeriod_Ledger_Ledger",
                        column: x => x.Ledger,
                        principalTable: "Ledger",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsAddressCity",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityRecId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CountyId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SettlementRecId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsAddressCity", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsAddressCity_Name", x => x.Name);
                    table.ForeignKey(
                        name: "FK_LogisticsAddressCity_LogisticsAddressCountryRegion_CountryRegionId",
                        column: x => x.CountryRegionId,
                        principalTable: "LogisticsAddressCountryRegion",
                        principalColumn: "CountryRegionId");
                    table.ForeignKey(
                        name: "FK_LogisticsAddressCity_LogisticsAddressCounty_CountryRegionId_StateId_CountyId",
                        columns: x => new { x.CountryRegionId, x.StateId, x.CountyId },
                        principalTable: "LogisticsAddressCounty",
                        principalColumns: new[] { "CountryRegionId", "StateId", "CountyId" });
                    table.ForeignKey(
                        name: "FK_LogisticsAddressCity_LogisticsAddressState_CountryRegionId_StateId",
                        columns: x => new { x.CountryRegionId, x.StateId },
                        principalTable: "LogisticsAddressState",
                        principalColumns: new[] { "CountryRegionId", "StateId" });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HcmWorker",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonnelNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Person = table.Column<long>(type: "bigint", nullable: false),
                    DepartmentId = table.Column<short>(type: "smallint", nullable: false),
                    OccupationId = table.Column<short>(type: "smallint", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GenderId = table.Column<byte>(type: "tinyint", nullable: false),
                    NationalityId = table.Column<short>(type: "smallint", nullable: false),
                    ShowroomId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(256)", nullable: true),
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
                    table.PrimaryKey("PK_HcmWorker", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_HcmWorker_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HcmWorker_OrgDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrgDepartments",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorker_OrgEntities_ShowroomId",
                        column: x => x.ShowroomId,
                        principalTable: "OrgEntities",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_HcmWorker_OrgGenders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "OrgGenders",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorker_OrgNationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalTable: "OrgNationalities",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorker_OrgOccupations_OccupationId",
                        column: x => x.OccupationId,
                        principalTable: "OrgOccupations",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrgEmployeeGroupDetails",
                columns: table => new
                {
                    UserGroupID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgEmployeeGroupDetails", x => new { x.UserGroupID, x.UserID });
                    table.ForeignKey(
                        name: "FK_OrgEmployeeGroupDetails_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrgEmployeeGroupDetails_OrgEmployeeGroups_UserGroupID",
                        column: x => x.UserGroupID,
                        principalTable: "OrgEmployeeGroups",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SysNotificationRecipients",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysNotificationRecipients", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_SysNotificationRecipients_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SysNotificationRecipients_SysNotifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "SysNotifications",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SysUserSettings",
                columns: table => new
                {
                    RECID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PageSize = table.Column<int>(type: "int", nullable: false),
                    NotificationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DashboardLayout = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_SysUserSettings", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SysUserSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestControlsOptions",
                columns: table => new
                {
                    OptionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestControlId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ExtendedProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WfRequestControlsOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_WfRequestControlsOptions_WfRequestControls_RequestControlId",
                        column: x => x.RequestControlId,
                        principalTable: "WfRequestControls",
                        principalColumn: "RequestControlId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestControlsValidations",
                columns: table => new
                {
                    ValidationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestControlId = table.Column<long>(type: "bigint", nullable: false),
                    ValidationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValidationExpression = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaskInput = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_WfRequestControlsValidations", x => x.ValidationId);
                    table.ForeignKey(
                        name: "FK_WfRequestControlsValidations_WfRequestControls_RequestControlId",
                        column: x => x.RequestControlId,
                        principalTable: "WfRequestControls",
                        principalColumn: "RequestControlId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfActivities",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    StepId = table.Column<long>(type: "bigint", nullable: false),
                    PerformerId = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SysNotificationTemplateId = table.Column<int>(type: "int", nullable: true),
                    AlertingBySystem = table.Column<bool>(type: "bit", nullable: false),
                    AlertingByEmail = table.Column<bool>(type: "bit", nullable: false),
                    AlertingBySms = table.Column<bool>(type: "bit", nullable: false),
                    AlertingByWhatsApp = table.Column<bool>(type: "bit", nullable: false),
                    ShowPreviousSteps = table.Column<bool>(type: "bit", nullable: false),
                    ShowPreviousDocs = table.Column<bool>(type: "bit", nullable: false),
                    MandatoryDocs = table.Column<bool>(type: "bit", nullable: false),
                    AutoPassEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AutoPassingHrs = table.Column<byte>(type: "tinyint", nullable: false),
                    ExtendedProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfActivities", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_WfActivities_SysNotificationTemplates_SysNotificationTemplateId",
                        column: x => x.SysNotificationTemplateId,
                        principalTable: "SysNotificationTemplates",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfActivities_WfActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "WfActivityTypes",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfActivities_WfPerformers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "WfPerformers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfActivities_WfSteps_StepId",
                        column: x => x.StepId,
                        principalTable: "WfSteps",
                        principalColumn: "StepId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestMappingVariables",
                columns: table => new
                {
                    MappingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestControlID = table.Column<long>(type: "bigint", nullable: false),
                    VariableID = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Activated = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfRequestMappingVariables", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_WfRequestMappingVariables_WfRequestControls_RequestControlID",
                        column: x => x.RequestControlID,
                        principalTable: "WfRequestControls",
                        principalColumn: "RequestControlId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfRequestMappingVariables_WfVariables_VariableID",
                        column: x => x.VariableID,
                        principalTable: "WfVariables",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsAddressDistrict",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    City = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsAddressDistrict", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsAddressDistrict_Name", x => x.Name);
                    table.ForeignKey(
                        name: "FK_LogisticsAddressDistrict_LogisticsAddressCity_City",
                        column: x => x.City,
                        principalTable: "LogisticsAddressCity",
                        principalColumn: "RECID");
                });

            migrationBuilder.CreateTable(
                name: "OrgEmployeeManagers",
                columns: table => new
                {
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ManagementLevelId = table.Column<byte>(type: "tinyint", nullable: false),
                    ManagerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgEmployeeManagers", x => new { x.EmployeeId, x.ManagementLevelId });
                    table.ForeignKey(
                        name: "FK_OrgEmployeeManagers_HcmWorker_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrgEmployeeManagers_HcmWorker_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrgEmployeeManagers_OrgManagementLevels_ManagementLevelId",
                        column: x => x.ManagementLevelId,
                        principalTable: "OrgManagementLevels",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequests",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    RequestDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    FinishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsStopped = table.Column<bool>(type: "bit", nullable: false),
                    StoppedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Progress = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfRequests", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_WfRequests_HcmWorker_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfRequests_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfUsersProcesses",
                columns: table => new
                {
                    UsersProcessesId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    DepartmentId = table.Column<short>(type: "smallint", nullable: true),
                    OccupationId = table.Column<short>(type: "smallint", nullable: true),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_WfUsersProcesses", x => x.UsersProcessesId);
                    table.ForeignKey(
                        name: "FK_WfUsersProcesses_HcmWorker_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfUsersProcesses_OrgDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrgDepartments",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_WfUsersProcesses_OrgOccupations_OccupationId",
                        column: x => x.OccupationId,
                        principalTable: "OrgOccupations",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_WfUsersProcesses_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfActivityControls",
                columns: table => new
                {
                    ActivityControlId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    ControlId = table.Column<byte>(type: "tinyint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtendedProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfActivityControls", x => x.ActivityControlId);
                    table.ForeignKey(
                        name: "FK_WfActivityControls_WfActivities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "WfActivities",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfActivityControls_WfControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "WfControls",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfTransitions",
                columns: table => new
                {
                    TransitionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityId = table.Column<long>(type: "bigint", nullable: true),
                    RequestControlId = table.Column<long>(type: "bigint", nullable: true),
                    VariableId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorId = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StepId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_WfTransitions", x => x.TransitionId);
                    table.ForeignKey(
                        name: "FK_WfTransitions_WfActivities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "WfActivities",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfTransitions_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfTransitions_WfVariables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "WfVariables",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfAssignments",
                columns: table => new
                {
                    AssignmentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityId = table.Column<long>(type: "bigint", nullable: false),
                    StepId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    FinishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoPassing = table.Column<bool>(type: "bit", nullable: false),
                    AutoPassingHrs = table.Column<byte>(type: "tinyint", nullable: false),
                    Automatically = table.Column<bool>(type: "bit", nullable: true),
                    Transferred = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_WfAssignments", x => x.AssignmentID);
                    table.ForeignKey(
                        name: "FK_WfAssignments_WfActivities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "WfActivities",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfAssignments_WfRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "WfRequests",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfProcessVariables",
                columns: table => new
                {
                    ProcessVariableId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    VariableId = table.Column<long>(type: "bigint", nullable: false),
                    VariableValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfProcessVariables", x => x.ProcessVariableId);
                    table.ForeignKey(
                        name: "FK_WfProcessVariables_WfRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "WfRequests",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfProcessVariables_WfVariables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "WfVariables",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestVariables",
                columns: table => new
                {
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    VariableId = table.Column<long>(type: "bigint", nullable: false),
                    VariableValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfRequestVariables", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_WfRequestVariables_WfRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "WfRequests",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfRequestVariables_WfVariables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "WfVariables",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfActivityControlsOptions",
                columns: table => new
                {
                    OptionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityControlId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_WfActivityControlsOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_WfActivityControlsOptions_WfActivityControls_ActivityControlId",
                        column: x => x.ActivityControlId,
                        principalTable: "WfActivityControls",
                        principalColumn: "ActivityControlId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfActivityControlsValidations",
                columns: table => new
                {
                    ValidationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityControlId = table.Column<long>(type: "bigint", nullable: false),
                    ValidationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValidationExpression = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaskInput = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfActivityControlsValidations", x => x.ValidationId);
                    table.ForeignKey(
                        name: "FK_WfActivityControlsValidations_WfActivityControls_ActivityControlId",
                        column: x => x.ActivityControlId,
                        principalTable: "WfActivityControls",
                        principalColumn: "ActivityControlId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfActivityMappingVariables",
                columns: table => new
                {
                    MappingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityControlID = table.Column<long>(type: "bigint", nullable: false),
                    VariableID = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Activated = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfActivityMappingVariables", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_WfActivityMappingVariables_WfActivityControls_ActivityControlID",
                        column: x => x.ActivityControlID,
                        principalTable: "WfActivityControls",
                        principalColumn: "ActivityControlId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfActivityMappingVariables_WfVariables_VariableID",
                        column: x => x.VariableID,
                        principalTable: "WfVariables",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfProcessData",
                columns: table => new
                {
                    TaskID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentID = table.Column<long>(type: "bigint", nullable: true),
                    FinishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivityDetails = table.Column<string>(type: "xml", nullable: false),
                    ExtendedProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WfProcessData", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_WfProcessData_WfAssignments_AssignmentID",
                        column: x => x.AssignmentID,
                        principalTable: "WfAssignments",
                        principalColumn: "AssignmentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyInfo",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Party = table.Column<long>(type: "bigint", nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", nullable: true),
                    TaxLicenseNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FederalTaxId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Calendar = table.Column<long>(type: "bigint", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ReportLogo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Memo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalizedRegion = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_CompanyInfo", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_CompanyInfo_Currency_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_CompanyInfo_FiscalCalendar_Calendar",
                        column: x => x.Calendar,
                        principalTable: "FiscalCalendar",
                        principalColumn: "RECID");
                });

            migrationBuilder.CreateTable(
                name: "ContactPerson",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactPersonId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Party = table.Column<long>(type: "bigint", nullable: false),
                    ContactForParty = table.Column<long>(type: "bigint", nullable: false),
                    CustAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Inactive = table.Column<int>(type: "int", nullable: false),
                    Vip = table.Column<int>(type: "int", nullable: false),
                    Imported = table.Column<int>(type: "int", nullable: false),
                    IsContactPersonExternallyMaintained = table.Column<int>(type: "int", nullable: false),
                    Sensitivity = table.Column<int>(type: "int", nullable: false),
                    MainResponsibleWorker = table.Column<long>(type: "bigint", nullable: false),
                    TimeAvailableFrom = table.Column<int>(type: "int", nullable: false),
                    TimeAvailableTo = table.Column<int>(type: "int", nullable: false),
                    DirectMail = table.Column<int>(type: "int", nullable: false),
                    McrIsDefaultContact = table.Column<int>(type: "int", nullable: false),
                    VendorPortalAccessAllowed = table.Column<int>(type: "int", nullable: false),
                    WebRequestAccess = table.Column<int>(type: "int", nullable: false),
                    VendRole = table.Column<int>(type: "int", nullable: false),
                    LastEditAxDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditAxDateTimeTzId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ContactPerson", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_ContactPerson_CustTable_CustAccount",
                        column: x => x.CustAccount,
                        principalTable: "CustTable",
                        principalColumn: "AccountNum",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactPerson_HcmWorker_MainResponsibleWorker",
                        column: x => x.MainResponsibleWorker,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DirPartyLocation",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Party = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<int>(type: "int", nullable: false),
                    IsPostalAddress = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<int>(type: "int", nullable: false),
                    IsLocationOwner = table.Column<int>(type: "int", nullable: false),
                    IsPrimaryTaxRegistration = table.Column<int>(type: "int", nullable: false),
                    IsRoleBusiness = table.Column<int>(type: "int", nullable: false),
                    IsRoleDelivery = table.Column<int>(type: "int", nullable: false),
                    IsRoleInvoice = table.Column<int>(type: "int", nullable: false),
                    IsRoleHome = table.Column<int>(type: "int", nullable: false),
                    PostalAddressRoles = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignmentDateTzId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DirPartyLocation", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DirPartyLocation_LogisticsLocation_Location",
                        column: x => x.Location,
                        principalTable: "LogisticsLocation",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DirPartyLocationRole",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyLocation = table.Column<long>(type: "bigint", nullable: false),
                    LocationRole = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DirPartyLocationRole", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DirPartyLocationRole_DirPartyLocation_PartyLocation",
                        column: x => x.PartyLocation,
                        principalTable: "DirPartyLocation",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DirPartyLocationRole_LogisticsLocationRole_LocationRole",
                        column: x => x.LocationRole,
                        principalTable: "LogisticsLocationRole",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DirPartyTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameAlias = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RelationType = table.Column<long>(type: "bigint", nullable: false),
                    InstanceRelationType = table.Column<long>(type: "bigint", nullable: false),
                    LegacyInstanceRelationType = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryAddressLocation = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactPhone = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactFax = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactEmail = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactUrl = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactTelex = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactTwitter = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactFacebook = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryContactLinkedIn = table.Column<long>(type: "bigint", nullable: true),
                    CommunicatorSignIn = table.Column<long>(type: "bigint", nullable: true),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    AddressBookNames = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LocalizationCountryRegionCode = table.Column<int>(type: "int", nullable: true),
                    EeEnablePersonalDataReadLog = table.Column<int>(type: "int", nullable: true),
                    EeEnableRoleChangeLog = table.Column<int>(type: "int", nullable: true),
                    Initials = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BirthYear = table.Column<int>(type: "int", nullable: true),
                    BirthMonth = table.Column<int>(type: "int", nullable: true),
                    Birthday = table.Column<int>(type: "int", nullable: true),
                    AnniversaryYear = table.Column<int>(type: "int", nullable: true),
                    AnniversaryMonth = table.Column<int>(type: "int", nullable: true),
                    AnniversaryDay = table.Column<int>(type: "int", nullable: true),
                    NameSequence = table.Column<long>(type: "bigint", nullable: true),
                    PersonalTitle = table.Column<long>(type: "bigint", nullable: true),
                    PersonalSuffix = table.Column<long>(type: "bigint", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    MaritalStatus = table.Column<int>(type: "int", nullable: true),
                    NumberOfEmployees = table.Column<int>(type: "int", nullable: true),
                    DunsNumberRecId = table.Column<long>(type: "bigint", nullable: true),
                    Abc = table.Column<int>(type: "int", nullable: true),
                    OrganizationType = table.Column<int>(type: "int", nullable: true),
                    OmOperatingUnitNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OmOperatingUnitType = table.Column<int>(type: "int", nullable: true),
                    TeamAdministrator = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TeamMembershipCriterion = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: true),
                    DataArea = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Key_ = table.Column<int>(type: "int", nullable: true),
                    ConversionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyNafCode = table.Column<long>(type: "bigint", nullable: true),
                    PaymInstruction1 = table.Column<long>(type: "bigint", nullable: true),
                    PaymInstruction2 = table.Column<long>(type: "bigint", nullable: true),
                    PaymInstruction3 = table.Column<long>(type: "bigint", nullable: true),
                    PaymInstruction4 = table.Column<long>(type: "bigint", nullable: true),
                    IsConsolidationCompany = table.Column<int>(type: "int", nullable: true),
                    IsEliminationCompany = table.Column<int>(type: "int", nullable: true),
                    PlanningCompany = table.Column<int>(type: "int", nullable: true),
                    OrgNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    CoRegNum = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    RegNum = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    VatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ImportVatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Bank = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    DvrID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RFullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CompanyRegComFr = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LegalFormFr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackMaterialFeeLicenseNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Tax1099RegNum = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    AccountOfficeRefNum = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Validate1099OnEntry = table.Column<int>(type: "int", nullable: true),
                    CombinedFedStateFiler = table.Column<int>(type: "int", nullable: true),
                    ForeignEntityIndicator = table.Column<int>(type: "int", nullable: true),
                    LastFilingIndicator = table.Column<int>(type: "int", nullable: true),
                    SiaCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    SubordinateCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Resident_W = table.Column<int>(type: "int", nullable: true),
                    HcmWorker = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "dat")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirPartyTable", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DirPartyTable_LogisticsLocation_PrimaryAddressLocation",
                        column: x => x.PrimaryAddressLocation,
                        principalTable: "LogisticsLocation",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsElectronicAddress",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElectronicAddressId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Locator = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LocatorExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ElectronicAddressRoles = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsPrimary = table.Column<int>(type: "int", nullable: false),
                    IsMobilePhone = table.Column<int>(type: "int", nullable: false),
                    IsInstantMessage = table.Column<int>(type: "int", nullable: false),
                    PrivateForParty = table.Column<long>(type: "bigint", nullable: true),
                    IsPrivate = table.Column<int>(type: "int", nullable: false),
                    ChannelReferenceId = table.Column<string>(type: "nvarchar(38)", maxLength: 38, nullable: false),
                    RetailMarketingOptIn = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LogisticsElectronicAddress", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_LogisticsElectronicAddress_DirPartyTable_PrivateForParty",
                        column: x => x.PrivateForParty,
                        principalTable: "DirPartyTable",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsElectronicAddress_LogisticsLocation_Location",
                        column: x => x.Location,
                        principalTable: "LogisticsLocation",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsPostalAddress",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<long>(type: "bigint", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidFromTzId = table.Column<int>(type: "int", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidToTzId = table.Column<int>(type: "int", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    StreetNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BuildingCompliment = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PostBox = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    County = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DistrictName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CityRecId = table.Column<long>(type: "bigint", nullable: false),
                    ZipCodeRecId = table.Column<long>(type: "bigint", nullable: false),
                    District = table.Column<long>(type: "bigint", nullable: false),
                    LocalityRecId = table.Column<long>(type: "bigint", nullable: false),
                    SettlementRecId = table.Column<long>(type: "bigint", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
                    PrivateForParty = table.Column<long>(type: "bigint", nullable: true),
                    IsPrivate = table.Column<int>(type: "int", nullable: false),
                    ChannelReferenceId = table.Column<string>(type: "nvarchar(38)", maxLength: 38, nullable: false),
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
                    table.PrimaryKey("PK_LogisticsPostalAddress", x => x.RECID);
                    table.UniqueConstraint("AK_LogisticsPostalAddress_Location", x => x.Location);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_DirPartyTable_PrivateForParty",
                        column: x => x.PrivateForParty,
                        principalTable: "DirPartyTable",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressCity_City",
                        column: x => x.City,
                        principalTable: "LogisticsAddressCity",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressCity_CityRecId",
                        column: x => x.CityRecId,
                        principalTable: "LogisticsAddressCity",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressCountryRegion_CountryRegionId",
                        column: x => x.CountryRegionId,
                        principalTable: "LogisticsAddressCountryRegion",
                        principalColumn: "CountryRegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressCounty_CountryRegionId_State_County",
                        columns: x => new { x.CountryRegionId, x.State, x.County },
                        principalTable: "LogisticsAddressCounty",
                        principalColumns: new[] { "CountryRegionId", "StateId", "CountyId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressDistrict_District",
                        column: x => x.District,
                        principalTable: "LogisticsAddressDistrict",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressDistrict_DistrictName",
                        column: x => x.DistrictName,
                        principalTable: "LogisticsAddressDistrict",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressState_CountryRegionId_State",
                        columns: x => new { x.CountryRegionId, x.State },
                        principalTable: "LogisticsAddressState",
                        principalColumns: new[] { "CountryRegionId", "StateId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressZipCode_ZipCode",
                        column: x => x.ZipCode,
                        principalTable: "LogisticsAddressZipCode",
                        principalColumn: "ZipCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsAddressZipCode_ZipCodeRecId",
                        column: x => x.ZipCodeRecId,
                        principalTable: "LogisticsAddressZipCode",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPostalAddress_LogisticsLocation_Location",
                        column: x => x.Location,
                        principalTable: "LogisticsLocation",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Party = table.Column<long>(type: "bigint", nullable: true),
                    VendGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymTermId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymSpec = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymDayId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CashDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BankAccount = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreditMax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LvPaymTransCodes = table.Column<long>(type: "bigint", nullable: false),
                    UseCashDisc = table.Column<int>(type: "int", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: true),
                    OffsetLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    OffsetAccountType = table.Column<int>(type: "int", nullable: false),
                    DlvTerm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultInventStatusId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DestinationCodeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriceGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MultiLineDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndDisc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PurchPoolId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItemBuyerGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MarkupGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxWithholdGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VatNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VatNumRecId = table.Column<long>(type: "bigint", nullable: false),
                    FiscalCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TaxVendorChargeTaxToleranceAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxVendorChargeTaxTolerancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InclTax = table.Column<int>(type: "int", nullable: false),
                    OverrideSalesTax = table.Column<int>(type: "int", nullable: false),
                    TaxWithholdCalculate = table.Column<int>(type: "int", nullable: false),
                    AccrueSalesTaxType = table.Column<int>(type: "int", nullable: false),
                    TaxVendorChargeTaxToleranceValidation = table.Column<int>(type: "int", nullable: false),
                    VatNumTableType = table.Column<int>(type: "int", nullable: false),
                    Tax1099RegNum = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Tax1099Fields = table.Column<long>(type: "bigint", nullable: false),
                    Tax1099Reports = table.Column<int>(type: "int", nullable: false),
                    W9 = table.Column<int>(type: "int", nullable: false),
                    W9Included = table.Column<int>(type: "int", nullable: false),
                    SecondTin = table.Column<int>(type: "int", nullable: false),
                    FatcaFilingRequirement = table.Column<int>(type: "int", nullable: false),
                    ForeignEntityIndicator = table.Column<int>(type: "int", nullable: false),
                    Tax1099NameChoice = table.Column<int>(type: "int", nullable: false),
                    TaxIdType = table.Column<int>(type: "int", nullable: false),
                    EthnicOriginId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ResidenceForeignCountryRegionId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SmallBusiness = table.Column<int>(type: "int", nullable: false),
                    MinorityOwned = table.Column<int>(type: "int", nullable: false),
                    FemaleOwned = table.Column<int>(type: "int", nullable: false),
                    VeteranOwned = table.Column<int>(type: "int", nullable: false),
                    DisabledOwned = table.Column<int>(type: "int", nullable: false),
                    HubZone = table.Column<int>(type: "int", nullable: false),
                    LocallyOwned = table.Column<int>(type: "int", nullable: false),
                    ContactPersonId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MainContactWorker = table.Column<long>(type: "bigint", nullable: true),
                    VendorPortalAdministratorRecId = table.Column<long>(type: "bigint", nullable: true),
                    CxmlOrderEnable = table.Column<int>(type: "int", nullable: false),
                    BidOnly = table.Column<int>(type: "int", nullable: false),
                    OneTimeVendor = table.Column<int>(type: "int", nullable: false),
                    VendVendorCollaborationType = table.Column<int>(type: "int", nullable: false),
                    PurchAmountPurchaseOrder = table.Column<int>(type: "int", nullable: false),
                    MatchingPolicy = table.Column<int>(type: "int", nullable: false),
                    ChangeRequestEnabled = table.Column<int>(type: "int", nullable: false),
                    ChangeRequestAllowOverride = table.Column<int>(type: "int", nullable: false),
                    ChangeRequestOverride = table.Column<int>(type: "int", nullable: false),
                    BlockedReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BlockedReleaseDateTzId = table.Column<int>(type: "int", nullable: false),
                    Blocked = table.Column<int>(type: "int", nullable: false),
                    WorkflowState = table.Column<int>(type: "int", nullable: false),
                    LineOfBusinessId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SegmentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubSegmentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumberSequenceGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItmCostTypeGroupId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItmOverUnderToleranceGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyNafCode = table.Column<long>(type: "bigint", nullable: false),
                    VendExceptionGroup = table.Column<long>(type: "bigint", nullable: false),
                    CisVerificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItmImportCostingVendor = table.Column<int>(type: "int", nullable: false),
                    ItmServicesProvider = table.Column<int>(type: "int", nullable: false),
                    ItmVendType = table.Column<int>(type: "int", nullable: false),
                    CisStatus = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_VendTable", x => x.RECID);
                    table.UniqueConstraint("AK_VendTable_AccountNum", x => x.AccountNum);
                    table.ForeignKey(
                        name: "FK_VendTable_DirPartyTable_Party",
                        column: x => x.Party,
                        principalTable: "DirPartyTable",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_VendTable_HcmWorker_MainContactWorker",
                        column: x => x.MainContactWorker,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID");
                    table.ForeignKey(
                        name: "FK_VendTable_PaymTerm_PaymTermId",
                        column: x => x.PaymTermId,
                        principalTable: "PaymTerm",
                        principalColumn: "PaymTermId");
                    table.ForeignKey(
                        name: "FK_VendTable_VendGroup_VendGroup",
                        column: x => x.VendGroup,
                        principalTable: "VendGroup",
                        principalColumn: "VendGroupCode");
                });

            migrationBuilder.CreateTable(
                name: "TaxAuthorityAddress",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxAuthority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TaxAuthorityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<long>(type: "bigint", nullable: true),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sms = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Pager = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOffGainLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    RoundOffLossLedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    RoundOffType = table.Column<int>(type: "int", nullable: false),
                    TaxReportLayout = table.Column<int>(type: "int", nullable: false),
                    UseDefaultLayout = table.Column<int>(type: "int", nullable: false),
                    SeparateTaxSummary = table.Column<int>(type: "int", nullable: false),
                    PrintBlankPage = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxAuthorityAddress", x => x.RECID);
                    table.UniqueConstraint("AK_TaxAuthorityAddress_TaxAuthority", x => x.TaxAuthority);
                    table.ForeignKey(
                        name: "FK_TaxAuthorityAddress_DimensionAttributeValueCombination_RoundOffGainLedgerDimension",
                        column: x => x.RoundOffGainLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxAuthorityAddress_DimensionAttributeValueCombination_RoundOffLossLedgerDimension",
                        column: x => x.RoundOffLossLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxAuthorityAddress_LogisticsLocation_Location",
                        column: x => x.Location,
                        principalTable: "LogisticsLocation",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxAuthorityAddress_VendTable_AccountNum",
                        column: x => x.AccountNum,
                        principalTable: "VendTable",
                        principalColumn: "AccountNum",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxPeriodHead",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TaxAuthority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QtyUnit = table.Column<int>(type: "int", nullable: false),
                    PeriodUnit = table.Column<int>(type: "int", nullable: false),
                    NotGenerateOffsetTaxTrans = table.Column<int>(type: "int", nullable: false),
                    ReportAdjustment = table.Column<int>(type: "int", nullable: false),
                    UseBatch = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxPeriodHead", x => x.RECID);
                    table.UniqueConstraint("AK_TaxPeriodHead_TaxPeriod", x => x.TaxPeriod);
                    table.ForeignKey(
                        name: "FK_TaxPeriodHead_TaxAuthorityAddress_TaxAuthority",
                        column: x => x.TaxAuthority,
                        principalTable: "TaxAuthorityAddress",
                        principalColumn: "TaxAuthority",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxReportPeriod",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VersionNum = table.Column<int>(type: "int", nullable: false),
                    LastPageNumSales = table.Column<int>(type: "int", nullable: false),
                    LastPageNumPurch = table.Column<int>(type: "int", nullable: false),
                    Closed = table.Column<int>(type: "int", nullable: false),
                    LastPeriod = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxReportPeriod", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_TaxReportPeriod_TaxPeriodHead_TaxPeriod",
                        column: x => x.TaxPeriod,
                        principalTable: "TaxPeriodHead",
                        principalColumn: "TaxPeriod",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "WfDataTypes",
                columns: new[] { "RECID", "Code", "CreatedAt", "CreatedBy", "DataAreaId", "Description", "IsActive", "IsDeleted", "LastModifiedAt", "LastModifiedBy", "Name", "OwnerAccountId", "RecVersion", "SortOrder" },
                values: new object[,]
                {
                    { (byte)1, "STR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "dat", null, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "String", null, 1, (byte)1 },
                    { (byte)2, "NUM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "dat", null, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Number", null, 1, (byte)2 },
                    { (byte)3, "BOOL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "dat", null, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Boolean", null, 1, (byte)3 },
                    { (byte)4, "DATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "dat", null, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Date", null, 1, (byte)4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetPermissions_Module_Resource_Action",
                table: "AspNetPermissions",
                columns: new[] { "Module", "Resource", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRolePermissions_PermissionId",
                table: "AspNetRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrgEntityId",
                table: "AspNetUsers",
                column: "OrgEntityId",
                unique: true,
                filter: "[OrgEntityId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfo_Calendar",
                table: "CompanyInfo",
                column: "Calendar");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfo_CurrencyCode",
                table: "CompanyInfo",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfo_Party",
                table: "CompanyInfo",
                column: "Party");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_ContactForParty",
                table: "ContactPerson",
                column: "ContactForParty");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_ContactPersonId_DataAreaId",
                table: "ContactPerson",
                columns: new[] { "ContactPersonId", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_CustAccount",
                table: "ContactPerson",
                column: "CustAccount");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_MainResponsibleWorker",
                table: "ContactPerson",
                column: "MainResponsibleWorker");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_Party",
                table: "ContactPerson",
                column: "Party");

            migrationBuilder.CreateIndex(
                name: "IX_Currency_CurrencyCode",
                table: "Currency",
                column: "CurrencyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currency_CurrencyCode_DataAreaId",
                table: "Currency",
                columns: new[] { "CurrencyCode", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currency_DataAreaId",
                table: "Currency",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustGroup_DataAreaId",
                table: "CustGroup",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustGroup_DataAreaId_CustGroupId",
                table: "CustGroup",
                columns: new[] { "DataAreaId", "CustGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_DataAreaId",
                table: "CustInvoiceJour",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_InvoiceAccount",
                table: "CustInvoiceJour",
                column: "InvoiceAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_InvoiceId",
                table: "CustInvoiceJour",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_OrderAccount",
                table: "CustInvoiceJour",
                column: "OrderAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceJour_SalesId",
                table: "CustInvoiceJour",
                column: "SalesId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceLine_ParentRecId",
                table: "CustInvoiceLine",
                column: "ParentRecId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_InvoiceDate",
                table: "CustInvoiceTable",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_InvoiceId",
                table: "CustInvoiceTable",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTable_OrderAccount",
                table: "CustInvoiceTable",
                column: "OrderAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_DataAreaId",
                table: "CustInvoiceTrans",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_InventTransId",
                table: "CustInvoiceTrans",
                column: "InventTransId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_InvoiceId",
                table: "CustInvoiceTrans",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_ItemId",
                table: "CustInvoiceTrans",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CustInvoiceTrans_SalesId",
                table: "CustInvoiceTrans",
                column: "SalesId");

            migrationBuilder.CreateIndex(
                name: "IX_CustLedger_DataAreaId",
                table: "CustLedger",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustLedger_PostingProfile",
                table: "CustLedger",
                column: "PostingProfile",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustLedgerAccounts_DataAreaId",
                table: "CustLedgerAccounts",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustLedgerAccounts_PostingProfile_AccountCode_Num",
                table: "CustLedgerAccounts",
                columns: new[] { "PostingProfile", "AccountCode", "Num" });

            migrationBuilder.CreateIndex(
                name: "IX_CustPackingSlipJour_PackingSlipId",
                table: "CustPackingSlipJour",
                column: "PackingSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_CustPackingSlipJour_SalesId",
                table: "CustPackingSlipJour",
                column: "SalesId");

            migrationBuilder.CreateIndex(
                name: "IX_CustPackingSlipTrans_InventTransId",
                table: "CustPackingSlipTrans",
                column: "InventTransId");

            migrationBuilder.CreateIndex(
                name: "IX_CustPackingSlipTrans_PackingSlipId",
                table: "CustPackingSlipTrans",
                column: "PackingSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_CustPaymModeTable_DataAreaId",
                table: "CustPaymModeTable",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustPaymModeTable_DataAreaId_PaymMode",
                table: "CustPaymModeTable",
                columns: new[] { "DataAreaId", "PaymMode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustQuotationJour_DataAreaId",
                table: "CustQuotationJour",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustQuotationJour_InvoiceAccount",
                table: "CustQuotationJour",
                column: "InvoiceAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustQuotationJour_OrderAccount",
                table: "CustQuotationJour",
                column: "OrderAccount");

            migrationBuilder.CreateIndex(
                name: "IX_CustQuotationJour_QuotationId",
                table: "CustQuotationJour",
                column: "QuotationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustQuotationTrans_DataAreaId",
                table: "CustQuotationTrans",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustQuotationTrans_QuotationId",
                table: "CustQuotationTrans",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustTable_DataAreaId",
                table: "CustTable",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustTable_DataAreaId_AccountNum",
                table: "CustTable",
                columns: new[] { "DataAreaId", "AccountNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustTransOpen_AccountNum",
                table: "CustTransOpen",
                column: "AccountNum");

            migrationBuilder.CreateIndex(
                name: "IX_CustTransOpen_DataAreaId",
                table: "CustTransOpen",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CustTransOpen_DueDate",
                table: "CustTransOpen",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustTransOpen_RefRecId",
                table: "CustTransOpen",
                column: "RefRecId");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyLocation_Location",
                table: "DirPartyLocation",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyLocation_Party",
                table: "DirPartyLocation",
                column: "Party");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyLocationRole_LocationRole",
                table: "DirPartyLocationRole",
                column: "LocationRole");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyLocationRole_PartyLocation_LocationRole",
                table: "DirPartyLocationRole",
                columns: new[] { "PartyLocation", "LocationRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PartyNumber",
                table: "DirPartyTable",
                column: "PartyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryAddressLocation",
                table: "DirPartyTable",
                column: "PrimaryAddressLocation");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactEmail",
                table: "DirPartyTable",
                column: "PrimaryContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactFacebook",
                table: "DirPartyTable",
                column: "PrimaryContactFacebook");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactFax",
                table: "DirPartyTable",
                column: "PrimaryContactFax");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactLinkedIn",
                table: "DirPartyTable",
                column: "PrimaryContactLinkedIn");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactPhone",
                table: "DirPartyTable",
                column: "PrimaryContactPhone");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactTelex",
                table: "DirPartyTable",
                column: "PrimaryContactTelex");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactTwitter",
                table: "DirPartyTable",
                column: "PrimaryContactTwitter");

            migrationBuilder.CreateIndex(
                name: "IX_DirPartyTable_PrimaryContactUrl",
                table: "DirPartyTable",
                column: "PrimaryContactUrl");

            migrationBuilder.CreateIndex(
                name: "IX_DlvMode_Code",
                table: "DlvMode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DlvMode_DataAreaId_RECID",
                table: "DlvMode",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DlvTerm_Code",
                table: "DlvTerm",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DlvTerm_DataAreaId_RECID",
                table: "DlvTerm",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocuRef_Record",
                table: "DocuRef",
                columns: new[] { "REFTABLEID", "REFRECID", "REFCOMPANYID" });

            migrationBuilder.CreateIndex(
                name: "IX_DocuRef_TYPEID",
                table: "DocuRef",
                column: "TYPEID");

            migrationBuilder.CreateIndex(
                name: "IX_DocuRef_VALUERECID",
                table: "DocuRef",
                column: "VALUERECID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRate_ExchangeRateCurrencyPair",
                table: "ExchangeRate",
                column: "ExchangeRateCurrencyPair");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateCurrencyPair_ExchangeRateType",
                table: "ExchangeRateCurrencyPair",
                column: "ExchangeRateType");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateCurrencyPair_FromCurrencyCode_ToCurrencyCode_ExchangeRateType_DataAreaId",
                table: "ExchangeRateCurrencyPair",
                columns: new[] { "FromCurrencyCode", "ToCurrencyCode", "ExchangeRateType", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateCurrencyPair_ToCurrencyCode",
                table: "ExchangeRateCurrencyPair",
                column: "ToCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateType_Name_DataAreaId",
                table: "ExchangeRateType",
                columns: new[] { "Name", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiscalCalendarPeriod_FiscalCalendar",
                table: "FiscalCalendarPeriod",
                column: "FiscalCalendar");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalCalendarPeriod_FiscalCalendarYear",
                table: "FiscalCalendarPeriod",
                column: "FiscalCalendarYear");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalCalendarYear_FiscalCalendar",
                table: "FiscalCalendarYear",
                column: "FiscalCalendar");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_DepartmentId",
                table: "HcmWorker",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_GenderId",
                table: "HcmWorker",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_NationalityId",
                table: "HcmWorker",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_OccupationId",
                table: "HcmWorker",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_ShowroomId",
                table: "HcmWorker",
                column: "ShowroomId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorker_UserId",
                table: "HcmWorker",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventTrans_ItemId",
                table: "InventTrans",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerFiscalCalendarPeriod_FiscalCalendarPeriod",
                table: "LedgerFiscalCalendarPeriod",
                column: "FiscalCalendarPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerFiscalCalendarPeriod_Ledger",
                table: "LedgerFiscalCalendarPeriod",
                column: "Ledger");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerFiscalCalendarYear_FiscalCalendarYear",
                table: "LedgerFiscalCalendarYear",
                column: "FiscalCalendarYear");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerFiscalCalendarYear_Ledger",
                table: "LedgerFiscalCalendarYear",
                column: "Ledger");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsAddressCity_CountryRegionId_StateId_CountyId",
                table: "LogisticsAddressCity",
                columns: new[] { "CountryRegionId", "StateId", "CountyId" });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsAddressCity_Name",
                table: "LogisticsAddressCity",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsAddressCountryRegion_CountryRegionId",
                table: "LogisticsAddressCountryRegion",
                column: "CountryRegionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsAddressCounty_CountryRegionId_StateId_CountyId",
                table: "LogisticsAddressCounty",
                columns: new[] { "CountryRegionId", "StateId", "CountyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsAddressDistrict_City",
                table: "LogisticsAddressDistrict",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsAddressState_CountryRegionId_StateId",
                table: "LogisticsAddressState",
                columns: new[] { "CountryRegionId", "StateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsElectronicAddress_Location",
                table: "LogisticsElectronicAddress",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsElectronicAddress_PrivateForParty",
                table: "LogisticsElectronicAddress",
                column: "PrivateForParty");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsLocationRole_Name",
                table: "LogisticsLocationRole",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_City",
                table: "LogisticsPostalAddress",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_CityRecId",
                table: "LogisticsPostalAddress",
                column: "CityRecId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_CountryRegionId_State_County",
                table: "LogisticsPostalAddress",
                columns: new[] { "CountryRegionId", "State", "County" });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_District",
                table: "LogisticsPostalAddress",
                column: "District");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_DistrictName",
                table: "LogisticsPostalAddress",
                column: "DistrictName");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_PrivateForParty",
                table: "LogisticsPostalAddress",
                column: "PrivateForParty");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_ZipCode",
                table: "LogisticsPostalAddress",
                column: "ZipCode");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPostalAddress_ZipCodeRecId",
                table: "LogisticsPostalAddress",
                column: "ZipCodeRecId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupTable_CustomerLedgerDimension",
                table: "MarkupTable",
                column: "CustomerLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupTable_MarkupCode",
                table: "MarkupTable",
                column: "MarkupCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarkupTable_VendorLedgerDimension",
                table: "MarkupTable",
                column: "VendorLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_OrgDepartments_Code",
                table: "OrgDepartments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeCategoryGroups_DepartmentID",
                table: "OrgEmployeeCategoryGroups",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeCategoryGroups_OccupationID",
                table: "OrgEmployeeCategoryGroups",
                column: "OccupationID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeCategoryGroups_UserCategoriesID",
                table: "OrgEmployeeCategoryGroups",
                column: "UserCategoriesID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeCategoryGroups_UserGroupID",
                table: "OrgEmployeeCategoryGroups",
                column: "UserGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeGroupDetails_UserID",
                table: "OrgEmployeeGroupDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeManagers_ManagementLevelId",
                table: "OrgEmployeeManagers",
                column: "ManagementLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEmployeeManagers_ManagerId",
                table: "OrgEmployeeManagers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgEntities_DepartmentId",
                table: "OrgEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymSched_Name",
                table: "PaymSched",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymSchedLine_Name",
                table: "PaymSchedLine",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPool_DataAreaId_RECID",
                table: "SalesPool",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesPool_SalesPoolId",
                table: "SalesPool",
                column: "SalesPoolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationTable_CustAccount",
                table: "SalesQuotationTable",
                column: "CustAccount");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationTable_DataAreaId_QuotationId",
                table: "SalesQuotationTable",
                columns: new[] { "DataAreaId", "QuotationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationTable_SalesIdRef",
                table: "SalesQuotationTable",
                column: "SalesIdRef");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTable_DataAreaId",
                table: "SalesTable",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTable_SalesId",
                table: "SalesTable",
                column: "SalesId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobExecutions_JobId",
                table: "SysBackgroundJobExecutions",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobExecutions_JobId_CreatedAt",
                table: "SysBackgroundJobExecutions",
                columns: new[] { "JobId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobExecutions_Status",
                table: "SysBackgroundJobExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobs_JobKey",
                table: "SysBackgroundJobs",
                column: "JobKey");

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobs_Name",
                table: "SysBackgroundJobs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobs_Status",
                table: "SysBackgroundJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobs_Status_IsEnabled_NextRunAt",
                table: "SysBackgroundJobs",
                columns: new[] { "Status", "IsEnabled", "NextRunAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobs_TenantId",
                table: "SysBackgroundJobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SysChatMessages_RoomId_SentAt",
                table: "SysChatMessages",
                columns: new[] { "RoomId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SysChatReadStates_UserId_RoomId",
                table: "SysChatReadStates",
                columns: new[] { "UserId", "RoomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysNotificationAuditLogs_NotificationId",
                table: "SysNotificationAuditLogs",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotificationRecipients_DeliveryStatus",
                table: "SysNotificationRecipients",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotificationRecipients_NotificationId_UserId",
                table: "SysNotificationRecipients",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysNotificationRecipients_UserId_IsRead_IsArchived",
                table: "SysNotificationRecipients",
                columns: new[] { "UserId", "IsRead", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_Category",
                table: "SysNotifications",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_Channel",
                table: "SysNotifications",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_CreatedAt",
                table: "SysNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_EntityType_EntityId",
                table: "SysNotifications",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_Priority",
                table: "SysNotifications",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_Status",
                table: "SysNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_TemplateId",
                table: "SysNotifications",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotifications_TenantId",
                table: "SysNotifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SysNotificationTemplates_Code",
                table: "SysNotificationTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysNumberSequences_NumberSequence",
                table: "SysNumberSequences",
                column: "NumberSequence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysUserSettings_UserId",
                table: "SysUserSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxAuthorityAddress_AccountNum",
                table: "TaxAuthorityAddress",
                column: "AccountNum");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAuthorityAddress_Location",
                table: "TaxAuthorityAddress",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAuthorityAddress_RoundOffGainLedgerDimension",
                table: "TaxAuthorityAddress",
                column: "RoundOffGainLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAuthorityAddress_RoundOffLossLedgerDimension",
                table: "TaxAuthorityAddress",
                column: "RoundOffLossLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_TaxData_TaxCode",
                table: "TaxData",
                column: "TaxCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaxGroupData_TaxCode",
                table: "TaxGroupData",
                column: "TaxCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaxGroupData_TaxExemptCode",
                table: "TaxGroupData",
                column: "TaxExemptCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaxGroupData_TaxGroup",
                table: "TaxGroupData",
                column: "TaxGroup");

            migrationBuilder.CreateIndex(
                name: "IX_TaxLedgerAccountGroup_TaxIncomingLedgerDimension",
                table: "TaxLedgerAccountGroup",
                column: "TaxIncomingLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_TaxLedgerAccountGroup_TaxOutgoingLedgerDimension",
                table: "TaxLedgerAccountGroup",
                column: "TaxOutgoingLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_TaxLedgerAccountGroup_TaxReportLedgerDimension",
                table: "TaxLedgerAccountGroup",
                column: "TaxReportLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_TaxOnItem_TaxCode",
                table: "TaxOnItem",
                column: "TaxCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaxOnItem_TaxExemptCode",
                table: "TaxOnItem",
                column: "TaxExemptCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaxOnItem_TaxItemGroup",
                table: "TaxOnItem",
                column: "TaxItemGroup");

            migrationBuilder.CreateIndex(
                name: "IX_TaxPeriodHead_TaxAuthority",
                table: "TaxPeriodHead",
                column: "TaxAuthority");

            migrationBuilder.CreateIndex(
                name: "IX_TaxReportPeriod_TaxPeriod",
                table: "TaxReportPeriod",
                column: "TaxPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_TaxTable_TaxCurrencyCode",
                table: "TaxTable",
                column: "TaxCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaxTable_TaxUnit",
                table: "TaxTable",
                column: "TaxUnit");

            migrationBuilder.CreateIndex(
                name: "IX_VendGroup_AccountingCurrencyExchangeRateType",
                table: "VendGroup",
                column: "AccountingCurrencyExchangeRateType");

            migrationBuilder.CreateIndex(
                name: "IX_VendGroup_PaymTermId",
                table: "VendGroup",
                column: "PaymTermId");

            migrationBuilder.CreateIndex(
                name: "IX_VendGroup_ReportingCurrencyExchangeRateType",
                table: "VendGroup",
                column: "ReportingCurrencyExchangeRateType");

            migrationBuilder.CreateIndex(
                name: "IX_VendGroup_VendGroupCode_DataAreaId",
                table: "VendGroup",
                columns: new[] { "VendGroupCode", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendTable_AccountNum_DataAreaId",
                table: "VendTable",
                columns: new[] { "AccountNum", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendTable_MainContactWorker",
                table: "VendTable",
                column: "MainContactWorker");

            migrationBuilder.CreateIndex(
                name: "IX_VendTable_Party",
                table: "VendTable",
                column: "Party");

            migrationBuilder.CreateIndex(
                name: "IX_VendTable_PaymTermId",
                table: "VendTable",
                column: "PaymTermId");

            migrationBuilder.CreateIndex(
                name: "IX_VendTable_VendGroup_DataAreaId",
                table: "VendTable",
                columns: new[] { "VendGroup", "DataAreaId" });

            migrationBuilder.CreateIndex(
                name: "IX_WfActivities_ActivityTypeId",
                table: "WfActivities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivities_PerformerId",
                table: "WfActivities",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivities_StepId",
                table: "WfActivities",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivities_SysNotificationTemplateId",
                table: "WfActivities",
                column: "SysNotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivityControls_ActivityId",
                table: "WfActivityControls",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivityControls_ControlId",
                table: "WfActivityControls",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivityControlsOptions_ActivityControlId",
                table: "WfActivityControlsOptions",
                column: "ActivityControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivityControlsValidations_ActivityControlId",
                table: "WfActivityControlsValidations",
                column: "ActivityControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivityMappingVariables_ActivityControlID",
                table: "WfActivityMappingVariables",
                column: "ActivityControlID");

            migrationBuilder.CreateIndex(
                name: "IX_WfActivityMappingVariables_VariableID",
                table: "WfActivityMappingVariables",
                column: "VariableID");

            migrationBuilder.CreateIndex(
                name: "IX_WfAssignments_ActivityId",
                table: "WfAssignments",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_WfAssignments_RequestId",
                table: "WfAssignments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WfCategories_Code",
                table: "WfCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfPerformers_PerformerType",
                table: "WfPerformers",
                column: "PerformerType");

            migrationBuilder.CreateIndex(
                name: "IX_WfProcessData_AssignmentID",
                table: "WfProcessData",
                column: "AssignmentID");

            migrationBuilder.CreateIndex(
                name: "IX_WfProcesses_CategoryId",
                table: "WfProcesses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WfProcesses_PriorityId",
                table: "WfProcesses",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_WfProcessVariables_RequestId",
                table: "WfProcessVariables",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WfProcessVariables_VariableId",
                table: "WfProcessVariables",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestControls_ControlId",
                table: "WfRequestControls",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestControls_ProcessId",
                table: "WfRequestControls",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestControlsOptions_RequestControlId",
                table: "WfRequestControlsOptions",
                column: "RequestControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestControlsValidations_RequestControlId",
                table: "WfRequestControlsValidations",
                column: "RequestControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestMappingVariables_RequestControlID",
                table: "WfRequestMappingVariables",
                column: "RequestControlID");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestMappingVariables_VariableID",
                table: "WfRequestMappingVariables",
                column: "VariableID");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequests_EmployeeId",
                table: "WfRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequests_ProcessId",
                table: "WfRequests",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestVariables_VariableId",
                table: "WfRequestVariables",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_WfSteps_ProcessId",
                table: "WfSteps",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WfTransitions_ActivityId",
                table: "WfTransitions",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_WfTransitions_ProcessId",
                table: "WfTransitions",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WfTransitions_VariableId",
                table: "WfTransitions",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_WfUsersPerformers_PerformerID",
                table: "WfUsersPerformers",
                column: "PerformerID");

            migrationBuilder.CreateIndex(
                name: "IX_WfUsersProcesses_DepartmentId",
                table: "WfUsersProcesses",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfUsersProcesses_EmployeeId",
                table: "WfUsersProcesses",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfUsersProcesses_OccupationId",
                table: "WfUsersProcesses",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_WfUsersProcesses_ProcessId",
                table: "WfUsersProcesses",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WfVariables_DataTypeId",
                table: "WfVariables",
                column: "DataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfVariables_ProcessId",
                table: "WfVariables",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyInfo_DirPartyTable_Party",
                table: "CompanyInfo",
                column: "Party",
                principalTable: "DirPartyTable",
                principalColumn: "RECID");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPerson_DirPartyTable_ContactForParty",
                table: "ContactPerson",
                column: "ContactForParty",
                principalTable: "DirPartyTable",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPerson_DirPartyTable_Party",
                table: "ContactPerson",
                column: "Party",
                principalTable: "DirPartyTable",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyLocation_DirPartyTable_Party",
                table: "DirPartyLocation",
                column: "Party",
                principalTable: "DirPartyTable",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactEmail",
                table: "DirPartyTable",
                column: "PrimaryContactEmail",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactFacebook",
                table: "DirPartyTable",
                column: "PrimaryContactFacebook",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactFax",
                table: "DirPartyTable",
                column: "PrimaryContactFax",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactLinkedIn",
                table: "DirPartyTable",
                column: "PrimaryContactLinkedIn",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactPhone",
                table: "DirPartyTable",
                column: "PrimaryContactPhone",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactTelex",
                table: "DirPartyTable",
                column: "PrimaryContactTelex",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactTwitter",
                table: "DirPartyTable",
                column: "PrimaryContactTwitter",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsElectronicAddress_PrimaryContactUrl",
                table: "DirPartyTable",
                column: "PrimaryContactUrl",
                principalTable: "LogisticsElectronicAddress",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirPartyTable_LogisticsPostalAddress_PrimaryAddressLocation",
                table: "DirPartyTable",
                column: "PrimaryAddressLocation",
                principalTable: "LogisticsPostalAddress",
                principalColumn: "Location",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsElectronicAddress_DirPartyTable_PrivateForParty",
                table: "LogisticsElectronicAddress");

            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsPostalAddress_DirPartyTable_PrivateForParty",
                table: "LogisticsPostalAddress");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetRolePermissions");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BankAccountTable");

            migrationBuilder.DropTable(
                name: "BankGroup");

            migrationBuilder.DropTable(
                name: "CompanyInfo");

            migrationBuilder.DropTable(
                name: "ContactPerson");

            migrationBuilder.DropTable(
                name: "CustConfirmJour");

            migrationBuilder.DropTable(
                name: "CustConfirmTrans");

            migrationBuilder.DropTable(
                name: "CustGroup");

            migrationBuilder.DropTable(
                name: "CustInvoiceJour");

            migrationBuilder.DropTable(
                name: "CustInvoiceLine");

            migrationBuilder.DropTable(
                name: "CustInvoiceTable");

            migrationBuilder.DropTable(
                name: "CustInvoiceTrans");

            migrationBuilder.DropTable(
                name: "CustLedgerAccounts");

            migrationBuilder.DropTable(
                name: "CustPackingSlipJour");

            migrationBuilder.DropTable(
                name: "CustPackingSlipTrans");

            migrationBuilder.DropTable(
                name: "CustPaymModeTable");

            migrationBuilder.DropTable(
                name: "CustQuotationJour");

            migrationBuilder.DropTable(
                name: "CustQuotationTrans");

            migrationBuilder.DropTable(
                name: "CustSettlement");

            migrationBuilder.DropTable(
                name: "CustTrans");

            migrationBuilder.DropTable(
                name: "CustTransOpen");

            migrationBuilder.DropTable(
                name: "DirPartyLocationRole");

            migrationBuilder.DropTable(
                name: "DlvMode");

            migrationBuilder.DropTable(
                name: "DlvTerm");

            migrationBuilder.DropTable(
                name: "DocuRef");

            migrationBuilder.DropTable(
                name: "ExchangeRate");

            migrationBuilder.DropTable(
                name: "GeneralJournalAccountEntry");

            migrationBuilder.DropTable(
                name: "GeneralJournalEntry");

            migrationBuilder.DropTable(
                name: "InventBatch");

            migrationBuilder.DropTable(
                name: "InventClosing");

            migrationBuilder.DropTable(
                name: "InventCountJour");

            migrationBuilder.DropTable(
                name: "InventDim");

            migrationBuilder.DropTable(
                name: "InventItemBarcode");

            migrationBuilder.DropTable(
                name: "InventItemGroup");

            migrationBuilder.DropTable(
                name: "InventItemLocation");

            migrationBuilder.DropTable(
                name: "InventItemPrice");

            migrationBuilder.DropTable(
                name: "InventJournalName");

            migrationBuilder.DropTable(
                name: "InventJournalTable");

            migrationBuilder.DropTable(
                name: "InventJournalTrans");

            migrationBuilder.DropTable(
                name: "InventLocation");

            migrationBuilder.DropTable(
                name: "InventPosting");

            migrationBuilder.DropTable(
                name: "InventSettlement");

            migrationBuilder.DropTable(
                name: "InventSite");

            migrationBuilder.DropTable(
                name: "InventSum");

            migrationBuilder.DropTable(
                name: "InventTableModule");

            migrationBuilder.DropTable(
                name: "InventTrans");

            migrationBuilder.DropTable(
                name: "InventTransOrigin");

            migrationBuilder.DropTable(
                name: "LedgerChartOfAccounts");

            migrationBuilder.DropTable(
                name: "LedgerFiscalCalendarPeriod");

            migrationBuilder.DropTable(
                name: "LedgerFiscalCalendarYear");

            migrationBuilder.DropTable(
                name: "LedgerJournalName");

            migrationBuilder.DropTable(
                name: "LedgerJournalTable");

            migrationBuilder.DropTable(
                name: "LedgerJournalTrans");

            migrationBuilder.DropTable(
                name: "MainAccount");

            migrationBuilder.DropTable(
                name: "MarkupTable");

            migrationBuilder.DropTable(
                name: "MarkupTrans");

            migrationBuilder.DropTable(
                name: "OrgAnnouncements");

            migrationBuilder.DropTable(
                name: "OrgEmployeeCategoryGroups");

            migrationBuilder.DropTable(
                name: "OrgEmployeeGroupDetails");

            migrationBuilder.DropTable(
                name: "OrgEmployeeManagers");

            migrationBuilder.DropTable(
                name: "PaymSchedLine");

            migrationBuilder.DropTable(
                name: "SalesLine");

            migrationBuilder.DropTable(
                name: "SalesPool");

            migrationBuilder.DropTable(
                name: "SalesQuotationLine");

            migrationBuilder.DropTable(
                name: "SalesQuotationTable");

            migrationBuilder.DropTable(
                name: "SalesTable");

            migrationBuilder.DropTable(
                name: "SpecTrans");

            migrationBuilder.DropTable(
                name: "SysAuditLogs");

            migrationBuilder.DropTable(
                name: "SysBackgroundJobExecutions");

            migrationBuilder.DropTable(
                name: "SysChatMessages");

            migrationBuilder.DropTable(
                name: "SysChatReadStates");

            migrationBuilder.DropTable(
                name: "SysDataSeedLogs");

            migrationBuilder.DropTable(
                name: "SysExceptionLogs");

            migrationBuilder.DropTable(
                name: "SysNotificationAuditLogs");

            migrationBuilder.DropTable(
                name: "SysNotificationPreferences");

            migrationBuilder.DropTable(
                name: "SysNotificationRecipients");

            migrationBuilder.DropTable(
                name: "SysNumberSequences");

            migrationBuilder.DropTable(
                name: "SysScheduledNotifications");

            migrationBuilder.DropTable(
                name: "SysSettings");

            migrationBuilder.DropTable(
                name: "SysUserSettings");

            migrationBuilder.DropTable(
                name: "TaxData");

            migrationBuilder.DropTable(
                name: "TaxGroupData");

            migrationBuilder.DropTable(
                name: "TaxJournalTrans");

            migrationBuilder.DropTable(
                name: "TaxLedgerAccountGroup");

            migrationBuilder.DropTable(
                name: "TaxOnItem");

            migrationBuilder.DropTable(
                name: "TaxReportPeriod");

            migrationBuilder.DropTable(
                name: "TaxTrans");

            migrationBuilder.DropTable(
                name: "WfActivityControlsOptions");

            migrationBuilder.DropTable(
                name: "WfActivityControlsValidations");

            migrationBuilder.DropTable(
                name: "WfActivityMappingVariables");

            migrationBuilder.DropTable(
                name: "WfOperators");

            migrationBuilder.DropTable(
                name: "WfProcessData");

            migrationBuilder.DropTable(
                name: "WfProcessTypes");

            migrationBuilder.DropTable(
                name: "WfProcessVariables");

            migrationBuilder.DropTable(
                name: "WfRequestControlsOptions");

            migrationBuilder.DropTable(
                name: "WfRequestControlsValidations");

            migrationBuilder.DropTable(
                name: "WfRequestDetails");

            migrationBuilder.DropTable(
                name: "WfRequestMappingVariables");

            migrationBuilder.DropTable(
                name: "WfRequestVariables");

            migrationBuilder.DropTable(
                name: "WfTransitions");

            migrationBuilder.DropTable(
                name: "WfUsersPerformers");

            migrationBuilder.DropTable(
                name: "WfUsersProcesses");

            migrationBuilder.DropTable(
                name: "AspNetPermissions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CustTable");

            migrationBuilder.DropTable(
                name: "CustLedger");

            migrationBuilder.DropTable(
                name: "DirPartyLocation");

            migrationBuilder.DropTable(
                name: "LogisticsLocationRole");

            migrationBuilder.DropTable(
                name: "DocuType");

            migrationBuilder.DropTable(
                name: "DocuValue");

            migrationBuilder.DropTable(
                name: "ExchangeRateCurrencyPair");

            migrationBuilder.DropTable(
                name: "InventTable");

            migrationBuilder.DropTable(
                name: "FiscalCalendarPeriod");

            migrationBuilder.DropTable(
                name: "Ledger");

            migrationBuilder.DropTable(
                name: "OrgEmployeeCategories");

            migrationBuilder.DropTable(
                name: "OrgEmployeeGroups");

            migrationBuilder.DropTable(
                name: "OrgManagementLevels");

            migrationBuilder.DropTable(
                name: "PaymSched");

            migrationBuilder.DropTable(
                name: "SysBackgroundJobs");

            migrationBuilder.DropTable(
                name: "SysNotifications");

            migrationBuilder.DropTable(
                name: "TaxGroupHeading");

            migrationBuilder.DropTable(
                name: "TaxExemptCodeTable");

            migrationBuilder.DropTable(
                name: "TaxItemGroupHeading");

            migrationBuilder.DropTable(
                name: "TaxTable");

            migrationBuilder.DropTable(
                name: "TaxPeriodHead");

            migrationBuilder.DropTable(
                name: "WfActivityControls");

            migrationBuilder.DropTable(
                name: "WfAssignments");

            migrationBuilder.DropTable(
                name: "WfRequestControls");

            migrationBuilder.DropTable(
                name: "WfVariables");

            migrationBuilder.DropTable(
                name: "FiscalCalendarYear");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "UnitOfMeasure");

            migrationBuilder.DropTable(
                name: "TaxAuthorityAddress");

            migrationBuilder.DropTable(
                name: "WfActivities");

            migrationBuilder.DropTable(
                name: "WfRequests");

            migrationBuilder.DropTable(
                name: "WfControls");

            migrationBuilder.DropTable(
                name: "WfDataTypes");

            migrationBuilder.DropTable(
                name: "FiscalCalendar");

            migrationBuilder.DropTable(
                name: "DimensionAttributeValueCombination");

            migrationBuilder.DropTable(
                name: "VendTable");

            migrationBuilder.DropTable(
                name: "SysNotificationTemplates");

            migrationBuilder.DropTable(
                name: "WfActivityTypes");

            migrationBuilder.DropTable(
                name: "WfPerformers");

            migrationBuilder.DropTable(
                name: "WfSteps");

            migrationBuilder.DropTable(
                name: "HcmWorker");

            migrationBuilder.DropTable(
                name: "VendGroup");

            migrationBuilder.DropTable(
                name: "WfPerformerType");

            migrationBuilder.DropTable(
                name: "WfProcesses");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OrgGenders");

            migrationBuilder.DropTable(
                name: "OrgNationalities");

            migrationBuilder.DropTable(
                name: "OrgOccupations");

            migrationBuilder.DropTable(
                name: "ExchangeRateType");

            migrationBuilder.DropTable(
                name: "PaymTerm");

            migrationBuilder.DropTable(
                name: "WfCategories");

            migrationBuilder.DropTable(
                name: "WfPriorities");

            migrationBuilder.DropTable(
                name: "OrgEntities");

            migrationBuilder.DropTable(
                name: "OrgDepartments");

            migrationBuilder.DropTable(
                name: "DirPartyTable");

            migrationBuilder.DropTable(
                name: "LogisticsElectronicAddress");

            migrationBuilder.DropTable(
                name: "LogisticsPostalAddress");

            migrationBuilder.DropTable(
                name: "LogisticsAddressDistrict");

            migrationBuilder.DropTable(
                name: "LogisticsAddressZipCode");

            migrationBuilder.DropTable(
                name: "LogisticsLocation");

            migrationBuilder.DropTable(
                name: "LogisticsAddressCity");

            migrationBuilder.DropTable(
                name: "LogisticsAddressCounty");

            migrationBuilder.DropTable(
                name: "LogisticsAddressState");

            migrationBuilder.DropTable(
                name: "LogisticsAddressCountryRegion");
        }
    }
}
