using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrimOutOfScopeEntityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredManExcludeSalesOrder",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "CredManId",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "CredManInCreditControl",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "CredManRejected",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "CredManReleasedFromCreditControl",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "DirectDebitMandate",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "EInvoiceAccountCode",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "IntercompanyAllowIndirectCreationOrig",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "IntercompanyDirectDeliveryOrig",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "IntercompanyOriginalCustAccount",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "IntercompanyOriginalSalesId",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "IntercompanyPurchId",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "RetailChannelTable",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "SubBillBillToName",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "SubBillBillToPostalAddress",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "SubBillCreatedFromSb",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "SubBillSuppressChild",
                table: "SalesTable");

            migrationBuilder.DropColumn(
                name: "ProjIdRef",
                table: "SalesQuotationTable");

            migrationBuilder.DropColumn(
                name: "ProjInvoiceProjId",
                table: "SalesQuotationTable");

            migrationBuilder.DropColumn(
                name: "RetailChannelTable",
                table: "SalesQuotationTable");

            migrationBuilder.DropColumn(
                name: "ProjectResource",
                table: "SalesQuotationLine");

            migrationBuilder.DropColumn(
                name: "EInvoiceAccountCode",
                table: "SalesLine");

            migrationBuilder.DropColumn(
                name: "ProjFundingSource",
                table: "SalesLine");

            migrationBuilder.DropColumn(
                name: "StandardMainAccount_W",
                table: "MainAccount");

            migrationBuilder.DropColumn(
                name: "ReceiptDate_W",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "RevRecDeferredLine",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "RevRecDeferredRecognizedQty",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "RevRecId",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "RevRecNewValuesFromReallocation",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "SubBillEscalationTableRecId",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "SubBillRenewalLineRecId",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "SubBillSchedLineRecId",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "TaxBase_W",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "VatDueDate_W",
                table: "LedgerJournalTrans");

            migrationBuilder.DropColumn(
                name: "Prepayment_W",
                table: "LedgerJournalName");

            migrationBuilder.DropColumn(
                name: "PdsPricingPrecision",
                table: "InventTableModule");

            migrationBuilder.DropColumn(
                name: "RetailInventoryAvailabilityBuffer",
                table: "InventTableModule");

            migrationBuilder.DropColumn(
                name: "RetailInventoryAvailabilityLevelProfile",
                table: "InventTableModule");

            migrationBuilder.DropColumn(
                name: "BomCalcGroupId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "BomLevel",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "BomManualReceipt",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "IntrastatChargePerKg",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "IntrastatCommodity",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "IntrastatExclude",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "ItmArrivalGroupId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "ItmCostTransferGroupId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "ItmCostTypeGroupId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "ItmOverUnderToleranceGroupId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdsBaseAttributeID",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdsBestBefore",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdsShelfAdvice",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdsShelfLife",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdsTargetFactor",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdscwWmsMinimumPalletQty",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdscwWmsQtyPerLayer",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdscwWmsStandardPalletQty",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "ProdOriginId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "QmsCustomerCheckItem",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "QmsOverDispensePct",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "QmsUnderDispensePct",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecBundle",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecDefaultRevenueRecognitionSchedule",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecExcludeFromCarveOut",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecMedianPrice",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecMedianPriceMaximumTolerance",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecMedianPriceMinimumTolerance",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "RevRecRevenueRecognitionEnabled",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "WmsArrivalHandlingTime",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "WmsPalletTypeIdId",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "WmsPickingQtyTime",
                table: "InventTable");

            migrationBuilder.DropColumn(
                name: "PdscwArrived",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwAvailOrdered",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwOrdered",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwPostedQty",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwQuotationIssue",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwQuotationReceipt",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwReceived",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwRegistered",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "PdscwReservOrdered",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "ItmCostTransRecId",
                table: "InventSettlement");

            migrationBuilder.DropColumn(
                name: "RetailWeightEx1",
                table: "InventLocation");

            migrationBuilder.DropColumn(
                name: "RetailWmsLocationIDDefaultReturn",
                table: "InventLocation");

            migrationBuilder.DropColumn(
                name: "BomLine",
                table: "InventJournalTrans");

            migrationBuilder.DropColumn(
                name: "ItmOverUnderTransfer",
                table: "InventJournalTrans");

            migrationBuilder.DropColumn(
                name: "PdsCopyBatchAttrib",
                table: "InventJournalTrans");

            migrationBuilder.DropColumn(
                name: "PdscwInventOnHand",
                table: "InventJournalTrans");

            migrationBuilder.DropColumn(
                name: "PdscwInventQtyCounted",
                table: "InventJournalTrans");

            migrationBuilder.DropColumn(
                name: "RetailReplenishmentType",
                table: "InventJournalTable");

            migrationBuilder.DropColumn(
                name: "RetailRetailStatusType",
                table: "InventJournalTable");

            migrationBuilder.DropColumn(
                name: "ATPApplyDemandTimeFence",
                table: "InventItemSalesSetup");

            migrationBuilder.DropColumn(
                name: "ATPApplySupplyTimeFence",
                table: "InventItemSalesSetup");

            migrationBuilder.DropColumn(
                name: "ATPBackwardDemandTimeFence",
                table: "InventItemSalesSetup");

            migrationBuilder.DropColumn(
                name: "ATPBackwardSupplyTimeFence",
                table: "InventItemSalesSetup");

            migrationBuilder.DropColumn(
                name: "ATPInclPlannedOrders",
                table: "InventItemSalesSetup");

            migrationBuilder.DropColumn(
                name: "ATPTimeFence",
                table: "InventItemSalesSetup");

            migrationBuilder.DropColumn(
                name: "PdscwPickingLocationMaxQty",
                table: "InventItemLocation");

            migrationBuilder.DropColumn(
                name: "PdscwPickingLocationRefillMin",
                table: "InventItemLocation");

            migrationBuilder.DropColumn(
                name: "ATPApplyDemandTimeFence",
                table: "InventItemInventSetup");

            migrationBuilder.DropColumn(
                name: "ATPApplySupplyTimeFence",
                table: "InventItemInventSetup");

            migrationBuilder.DropColumn(
                name: "ATPBackwardDemandTimeFence",
                table: "InventItemInventSetup");

            migrationBuilder.DropColumn(
                name: "ATPBackwardSupplyTimeFence",
                table: "InventItemInventSetup");

            migrationBuilder.DropColumn(
                name: "ATPInclPlannedOrders",
                table: "InventItemInventSetup");

            migrationBuilder.DropColumn(
                name: "ATPTimeFence",
                table: "InventItemInventSetup");

            migrationBuilder.DropColumn(
                name: "RevRecDefaultRevenueRecognitionSchedule",
                table: "InventItemGroup");

            migrationBuilder.DropColumn(
                name: "RevRecExcludeFromCarveOut",
                table: "InventItemGroup");

            migrationBuilder.DropColumn(
                name: "RevRecMedianPrice",
                table: "InventItemGroup");

            migrationBuilder.DropColumn(
                name: "RevRecMedianPriceMaximumTolerance",
                table: "InventItemGroup");

            migrationBuilder.DropColumn(
                name: "RevRecMedianPriceMinimumTolerance",
                table: "InventItemGroup");

            migrationBuilder.DropColumn(
                name: "RevRecRevenueRecognitionEnabled",
                table: "InventItemGroup");

            migrationBuilder.DropColumn(
                name: "RetailShowForItem",
                table: "InventItemBarcode");

            migrationBuilder.DropColumn(
                name: "BomLevel",
                table: "InventClosing");

            migrationBuilder.DropColumn(
                name: "ItmAdjustment",
                table: "InventClosing");

            migrationBuilder.DropColumn(
                name: "ProdJournal",
                table: "InventClosing");

            migrationBuilder.DropColumn(
                name: "PdsFinishedGoodsDateTested",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsInheritBatchAttrib",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsInheritedShelfLife",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsShelfAdviceDate",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsUseVendBatchDate",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsUseVendBatchExp",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsVendBatchDate",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "PdsVendExpiryDate",
                table: "InventBatch");

            migrationBuilder.DropColumn(
                name: "BillOfExchangeId",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "BillOfExchangeSeqNum",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "CredManExcludeFromCreditControl",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "CustAutomationExclude",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "CustAutomationPredictionSent",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "CustAutomationPredunningSent",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "DirectDebitMandate",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "RetailCustTrans",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "RetailStoreId",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "RetailTerminalId",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "RetailTransactionId",
                table: "CustTrans");

            migrationBuilder.DropColumn(
                name: "CredManAccountStatusId",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManBusinessStarted",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManCreditLimitDate",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManCreditLimitExpiryDate",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManCustCreditMaxAlt",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManCustUnlimitedCredit",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManCustomerSince",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManEligibleCreditLimitDate",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManEligibleCreditMax",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManExclude",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManLastReviewDate",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManNextSchedReviewDate",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManNotes",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManTitleHeld",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "CredManWithAgency",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "EInvoice",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "EInvoiceAttachment",
                table: "CustTable");

            migrationBuilder.DropColumn(
                name: "ProjDescription",
                table: "CustQuotationTrans");

            migrationBuilder.DropColumn(
                name: "ProjectResource",
                table: "CustQuotationTrans");

            migrationBuilder.DropColumn(
                name: "RetailCategory",
                table: "CustInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "SubBillRevenueSplit",
                table: "CustInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "SubBillRevenueSplitParentAmount",
                table: "CustInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "SubBillRevenueSplitParentLineRecId",
                table: "CustInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "DirectDebitMandate",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "ExchRate_W",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "InvoiceType_W",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "ManualNumbering_W",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "ProjIntercompany",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "SalesDate_W",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "VatDueDate_W",
                table: "CustInvoiceTable");

            migrationBuilder.DropColumn(
                name: "ProjFundingSource",
                table: "CustInvoiceLine");

            migrationBuilder.DropColumn(
                name: "RefReturnInvoiceTrans_W",
                table: "CustInvoiceLine");

            migrationBuilder.DropColumn(
                name: "DirectDebitMandate",
                table: "CustInvoiceJour");

            migrationBuilder.DropColumn(
                name: "InvoiceType_W",
                table: "CustInvoiceJour");

            migrationBuilder.DropColumn(
                name: "RetailStoreIdTable",
                table: "CustInvoiceJour");

            migrationBuilder.DropColumn(
                name: "SubBillSuppressChildItems",
                table: "CustInvoiceJour");

            migrationBuilder.DropColumn(
                name: "SubBillSuppressChildItems",
                table: "CustConfirmJour");

            migrationBuilder.DropColumn(
                name: "BankCorrAccount_W",
                table: "BankGroup");

            migrationBuilder.DropColumn(
                name: "CorrAccount_W",
                table: "BankAccountTable");





            migrationBuilder.CreateIndex(
                name: "IX_VendParameters_DataAreaId_Key",
                table: "VendParameters",
                columns: new[] { "DataAreaId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendParameters_DataAreaId_RECID",
                table: "VendParameters",
                columns: new[] { "DataAreaId", "RECID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendPaymModeTable_DataAreaId",
                table: "VendPaymModeTable",
                column: "DataAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_VendPaymModeTable_DataAreaId_PaymMode",
                table: "VendPaymModeTable",
                columns: new[] { "DataAreaId", "PaymMode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {




            migrationBuilder.AddColumn<bool>(
                name: "CredManExcludeSalesOrder",
                table: "SalesTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CredManId",
                table: "SalesTable",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CredManInCreditControl",
                table: "SalesTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CredManRejected",
                table: "SalesTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CredManReleasedFromCreditControl",
                table: "SalesTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "DirectDebitMandate",
                table: "SalesTable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceAccountCode",
                table: "SalesTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IntercompanyAllowIndirectCreationOrig",
                table: "SalesTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IntercompanyDirectDeliveryOrig",
                table: "SalesTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IntercompanyOriginalCustAccount",
                table: "SalesTable",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IntercompanyOriginalSalesId",
                table: "SalesTable",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IntercompanyPurchId",
                table: "SalesTable",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "RetailChannelTable",
                table: "SalesTable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "SubBillBillToName",
                table: "SalesTable",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SubBillBillToPostalAddress",
                table: "SalesTable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubBillCreatedFromSb",
                table: "SalesTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubBillSuppressChild",
                table: "SalesTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjIdRef",
                table: "SalesQuotationTable",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjInvoiceProjId",
                table: "SalesQuotationTable",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "RetailChannelTable",
                table: "SalesQuotationTable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ProjectResource",
                table: "SalesQuotationLine",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceAccountCode",
                table: "SalesLine",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ProjFundingSource",
                table: "SalesLine",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StandardMainAccount_W",
                table: "MainAccount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceiptDate_W",
                table: "LedgerJournalTrans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "RevRecDeferredLine",
                table: "LedgerJournalTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "RevRecDeferredRecognizedQty",
                table: "LedgerJournalTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "RevRecId",
                table: "LedgerJournalTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "RevRecNewValuesFromReallocation",
                table: "LedgerJournalTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SubBillEscalationTableRecId",
                table: "LedgerJournalTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SubBillRenewalLineRecId",
                table: "LedgerJournalTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SubBillSchedLineRecId",
                table: "LedgerJournalTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxBase_W",
                table: "LedgerJournalTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "VatDueDate_W",
                table: "LedgerJournalTrans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Prepayment_W",
                table: "LedgerJournalName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdsPricingPrecision",
                table: "InventTableModule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RetailInventoryAvailabilityBuffer",
                table: "InventTableModule",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RetailInventoryAvailabilityLevelProfile",
                table: "InventTableModule",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BomCalcGroupId",
                table: "InventTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BomLevel",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BomManualReceipt",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "IntrastatChargePerKg",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "IntrastatCommodity",
                table: "InventTable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "IntrastatExclude",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ItmArrivalGroupId",
                table: "InventTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItmCostTransferGroupId",
                table: "InventTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItmCostTypeGroupId",
                table: "InventTable",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItmOverUnderToleranceGroupId",
                table: "InventTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PdsBaseAttributeID",
                table: "InventTable",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PdsBestBefore",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdsShelfAdvice",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdsShelfLife",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PdsTargetFactor",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwWmsMinimumPalletQty",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwWmsQtyPerLayer",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwWmsStandardPalletQty",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProdOriginId",
                table: "InventTable",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QmsCustomerCheckItem",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QmsOverDispensePct",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QmsUnderDispensePct",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RevRecBundle",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RevRecDefaultRevenueRecognitionSchedule",
                table: "InventTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RevRecExcludeFromCarveOut",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RevRecMedianPrice",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RevRecMedianPriceMaximumTolerance",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RevRecMedianPriceMinimumTolerance",
                table: "InventTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RevRecRevenueRecognitionEnabled",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WmsArrivalHandlingTime",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WmsPalletTypeIdId",
                table: "InventTable",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WmsPickingQtyTime",
                table: "InventTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwArrived",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwAvailOrdered",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwOrdered",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwPostedQty",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwQuotationIssue",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwQuotationReceipt",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwReceived",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwRegistered",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwReservOrdered",
                table: "InventSum",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ItmCostTransRecId",
                table: "InventSettlement",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "RetailWeightEx1",
                table: "InventLocation",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RetailWmsLocationIDDefaultReturn",
                table: "InventLocation",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BomLine",
                table: "InventJournalTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItmOverUnderTransfer",
                table: "InventJournalTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdsCopyBatchAttrib",
                table: "InventJournalTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwInventOnHand",
                table: "InventJournalTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwInventQtyCounted",
                table: "InventJournalTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RetailReplenishmentType",
                table: "InventJournalTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetailRetailStatusType",
                table: "InventJournalTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ATPApplyDemandTimeFence",
                table: "InventItemSalesSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPApplySupplyTimeFence",
                table: "InventItemSalesSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPBackwardDemandTimeFence",
                table: "InventItemSalesSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPBackwardSupplyTimeFence",
                table: "InventItemSalesSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPInclPlannedOrders",
                table: "InventItemSalesSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPTimeFence",
                table: "InventItemSalesSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwPickingLocationMaxQty",
                table: "InventItemLocation",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PdscwPickingLocationRefillMin",
                table: "InventItemLocation",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ATPApplyDemandTimeFence",
                table: "InventItemInventSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPApplySupplyTimeFence",
                table: "InventItemInventSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPBackwardDemandTimeFence",
                table: "InventItemInventSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPBackwardSupplyTimeFence",
                table: "InventItemInventSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPInclPlannedOrders",
                table: "InventItemInventSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ATPTimeFence",
                table: "InventItemInventSetup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevRecDefaultRevenueRecognitionSchedule",
                table: "InventItemGroup",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RevRecExcludeFromCarveOut",
                table: "InventItemGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RevRecMedianPrice",
                table: "InventItemGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RevRecMedianPriceMaximumTolerance",
                table: "InventItemGroup",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RevRecMedianPriceMinimumTolerance",
                table: "InventItemGroup",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RevRecRevenueRecognitionEnabled",
                table: "InventItemGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetailShowForItem",
                table: "InventItemBarcode",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BomLevel",
                table: "InventClosing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItmAdjustment",
                table: "InventClosing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProdJournal",
                table: "InventClosing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdsFinishedGoodsDateTested",
                table: "InventBatch",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PdsInheritBatchAttrib",
                table: "InventBatch",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdsInheritedShelfLife",
                table: "InventBatch",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdsShelfAdviceDate",
                table: "InventBatch",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PdsUseVendBatchDate",
                table: "InventBatch",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdsUseVendBatchExp",
                table: "InventBatch",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdsVendBatchDate",
                table: "InventBatch",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PdsVendExpiryDate",
                table: "InventBatch",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "BillOfExchangeId",
                table: "CustTrans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BillOfExchangeSeqNum",
                table: "CustTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CredManExcludeFromCreditControl",
                table: "CustTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustAutomationExclude",
                table: "CustTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustAutomationPredictionSent",
                table: "CustTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustAutomationPredunningSent",
                table: "CustTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DirectDebitMandate",
                table: "CustTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "RetailCustTrans",
                table: "CustTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetailStoreId",
                table: "CustTrans",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RetailTerminalId",
                table: "CustTrans",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RetailTransactionId",
                table: "CustTrans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CredManAccountStatusId",
                table: "CustTable",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManBusinessStarted",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManCreditLimitDate",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManCreditLimitExpiryDate",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "CredManCustCreditMaxAlt",
                table: "CustTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CredManCustUnlimitedCredit",
                table: "CustTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManCustomerSince",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManEligibleCreditLimitDate",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "CredManEligibleCreditMax",
                table: "CustTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CredManExclude",
                table: "CustTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManLastReviewDate",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CredManNextSchedReviewDate",
                table: "CustTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CredManNotes",
                table: "CustTable",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CredManTitleHeld",
                table: "CustTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CredManWithAgency",
                table: "CustTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EInvoice",
                table: "CustTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EInvoiceAttachment",
                table: "CustTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjDescription",
                table: "CustQuotationTrans",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ProjectResource",
                table: "CustQuotationTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RetailCategory",
                table: "CustInvoiceTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubBillRevenueSplit",
                table: "CustInvoiceTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SubBillRevenueSplitParentAmount",
                table: "CustInvoiceTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "SubBillRevenueSplitParentLineRecId",
                table: "CustInvoiceTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DirectDebitMandate",
                table: "CustInvoiceTable",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchRate_W",
                table: "CustInvoiceTable",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType_W",
                table: "CustInvoiceTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ManualNumbering_W",
                table: "CustInvoiceTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjIntercompany",
                table: "CustInvoiceTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SalesDate_W",
                table: "CustInvoiceTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "VatDueDate_W",
                table: "CustInvoiceTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "ProjFundingSource",
                table: "CustInvoiceLine",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RefReturnInvoiceTrans_W",
                table: "CustInvoiceLine",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DirectDebitMandate",
                table: "CustInvoiceJour",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType_W",
                table: "CustInvoiceJour",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "RetailStoreIdTable",
                table: "CustInvoiceJour",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubBillSuppressChildItems",
                table: "CustInvoiceJour",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubBillSuppressChildItems",
                table: "CustConfirmJour",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BankCorrAccount_W",
                table: "BankGroup",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrAccount_W",
                table: "BankAccountTable",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "");
        }
    }
}
