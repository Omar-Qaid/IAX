namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// One allocation sequence per entity that owns a generated code. The key is the entity type name the
/// number sequence service resolves a sequence by, so it is not free text.
/// </summary>
/// <remarks>
/// Every sequence here is non-cyclic. The service resets a cyclic sequence once a calendar day, and
/// none of these formats carry a day, so a cyclic sequence would re-issue a code that already exists
/// inside its period. The integrity validator rejects that combination.
/// </remarks>
public static class AdministrationNumberSequenceCatalog
{
    public const int Lowest = 1;
    public const int Highest = 999999999;

    public static IReadOnlyList<NumberSequenceDefinition> Definitions { get; } = Build(
    [
        // Accounts and parties
        ("Customer", "Customer Sequence", "CUS-######", "{PREFIX}-{SEQ}"),
        ("CustomerGroup", "Customer Group Sequence", "CGRP-######", "{PREFIX}-{SEQ}"),
        ("Vendor", "Vendor Sequence", "VEN-######", "{PREFIX}-{SEQ}"),
        ("VendorGroup", "Vendor Group Sequence", "VGRP-######", "{PREFIX}-{SEQ}"),
        ("Voucher", "Voucher Sequence", "VOU-######", "{PREFIX}-{SEQ}"),

        // Accounts receivable
        ("SalesTable", "Sales Order Sequence", "SO-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("SalesLine", "Sale Line Sequence", "SOL-######", "{PREFIX}-{SEQ}"),
        ("PackingSlip", "Packing Slip Sequence", "PS-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("CustConfirmJour", "SO Confirm Sequence", "CONF-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("CustPackingSlipJour", "PS Journal ID Sequence", "PSJ-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("CustInvoiceJour", "Invoice Sequence", "INV-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("CustInvoiceTrans", "Invoice Trans Sequence", "INVT-######", "{PREFIX}-{SEQ}"),
        ("CustTrans", "Cust Transaction Sequence", "CTRX-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("CustSettlement", "Cust Settlement Sequence", "CSET-######", "{PREFIX}-{SEQ}"),
        ("ReturnItemNum", "Return Order RMA Sequence", "RMA-######", "{PREFIX}-{YYYY}-{SEQ}"),

        // Accounts payable
        ("PurchTable", "Purchase Order Sequence", "PO-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendPurchOrderJour", "PO Confirm Sequence", "PCONF-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendPackingSlipJour", "Product Receipt Sequence", "PR-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendInvoiceJour", "Vendor Invoice Sequence", "VINV-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendInvoiceInfoTable", "Pending Vend Invoice Seq", "PINV-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendInvoiceRegister", "Vend Invoice Register Seq", "VIR-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendTrans", "Vend Transaction Sequence", "VTRX-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("VendSettlement", "Vend Settlement Sequence", "VSET-######", "{PREFIX}-{SEQ}"),

        // General ledger and tax
        ("LedgerJournalTable", "Ledger Journal Sequence", "LJT-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("LedgerTransSettlement", "Ledger Settlement Sequence", "LSET-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("ExchAdjustment", "Exch Adjustment Sequence", "FXA-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("TaxSettlement", "Sales Tax Settlement Seq", "TXST-######", "{PREFIX}-{YYYY}-{SEQ}"),

        // Inventory
        ("InventJournalTable", "Inventory Journal Sequence", "IJRN-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("InventClosing", "Inventory Close Sequence", "ICLS-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("SettleTransId", "Inventory Settlement Sequence", "ISET-######", "{PREFIX}-{SEQ}"),
        ("InventTransOriginId", "Lot Origin Sequence", "ORG-######", "{PREFIX}-{SEQ}"),
        ("InventTransId", "Invent Lot Sequence", "LOT-######", "{PREFIX}-{SEQ}"),
        ("InventTrans", "Inv Transaction Sequence", "ITRX-######", "{PREFIX}-{YYYY}{MM}-{SEQ}"),
        ("InventTransferTable", "Transfer Order Sequence", "TRF-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("InventTransferJour", "Transfer Journal Sequence", "TRFJ-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("EcoResProduct", "Product Number Sequence", "PRD-######", "{PREFIX}-{SEQ}"),
        ("InventTable", "Item Number Sequence", "ITM-######", "{PREFIX}-{SEQ}"),
        ("InventDim", "Inventory Dimension Seq", "DIM-######", "{PREFIX}-{SEQ}"),
        ("InventGroup", "Group Sequence", "IGRP-######", "{PREFIX}-{SEQ}"),
        ("UnitOfMeasure", "UOM Sequence", "UOM-######", "{PREFIX}-{SEQ}"),
        ("InventUOM", "UOM Sequence", "IUOM-######", "{PREFIX}-{SEQ}"),

        // Organization and people
        ("CompanyInfo", "Company Sequence", "COMP-######", "{PREFIX}-{SEQ}"),
        ("HcmWorker", "HcmWorker Sequence", "EMP-######", "{PREFIX}-{SEQ}"),
        ("HcmWorkerGroup", "User Group Sequence", "UGRP-######", "{PREFIX}-{SEQ}"),
        ("HcmWorkerCategory", "User Category Sequence", "UCAT-######", "{PREFIX}-{SEQ}"),
        ("HcmShowroom", "HcmShowroom Sequence", "SHR-######", "{PREFIX}-{SEQ}"),
        ("Department", "Department Sequence", "DPT-######", "{PREFIX}-{SEQ}"),
        ("Occupation", "Occupation Sequence", "OCC-######", "{PREFIX}-{SEQ}"),
        ("Nationality", "Nationality Sequence", "NAT-######", "{PREFIX}-{SEQ}"),
        ("Gender", "Gender Sequence", "GEN-######", "{PREFIX}-{SEQ}"),
        ("ContactPerson", "Contact person", "CON-######", "{PREFIX}-{SEQ}"),
        ("DirPartyTable", "Global Address Book Party", "PTY-######", "{PREFIX}-{SEQ}"),
        ("LogisticsLocation", "Logistics Location Sequence", "LOC-######", "{PREFIX}-{SEQ}"),
        ("Announcement", "Announcement Sequence", "ANN-######", "{PREFIX}-{YYYY}-{SEQ}"),

        // Workflow
        ("WfCategory", "Wf Category Sequence", "WCT-######", "{PREFIX}-{SEQ}"),
        ("WfPriority", "Wf Priority Sequence", "WPR-######", "{PREFIX}-{SEQ}"),
        ("WfProcessType", "Wf Process Type Sequence", "WPT-######", "{PREFIX}-{SEQ}"),
        ("WfProcess", "Wf Process Sequence", "PROC-######", "{PREFIX}-{SEQ}"),
        ("WfStep", "Wf Step Sequence", "STEP-######", "{PREFIX}-{SEQ}"),
        ("WfActivity", "Wf Activity Sequence", "ACT-######", "{PREFIX}-{SEQ}"),
        ("WfActivityType", "Wf Activity Type Sequence", "ATYP-######", "{PREFIX}-{SEQ}"),
        ("WfActivityControl", "Wf Activity Control Seq.", "ACTL-######", "{PREFIX}-{SEQ}"),
        ("WfRequestControl", "Wf Request Control Seq.", "RCTL-######", "{PREFIX}-{SEQ}"),
        ("WfControl", "Wf Control Sequence", "CTL-######", "{PREFIX}-{SEQ}"),
        ("WfOperator", "Wf Operator Sequence", "OP-######", "{PREFIX}-{SEQ}"),
        ("WfPerformer", "Wf Performer Sequence", "PRF-######", "{PREFIX}-{SEQ}"),
        ("WfPerformerType", "Wf Performer Type Sequence", "PRT-######", "{PREFIX}-{SEQ}"),
        ("WfRequest", "Wf Request Sequence", "REQ-######", "{PREFIX}-{YYYY}-{SEQ}"),
        ("WfRequestDetail", "Wf Request Detail Seq.", "REQD-######", "{PREFIX}-{SEQ}"),
        ("WfTransition", "Wf Transition Sequence", "TRN-######", "{PREFIX}-{SEQ}"),
        ("WfVariable", "Wf Variable Sequence", "VAR-######", "{PREFIX}-{SEQ}"),
        ("WfDataType", "Wf DataType Sequence", "DT-######", "{PREFIX}-{SEQ}"),

        // The sequence table allocates its own codes too
        ("SysNumberSequence", "Number Sequence Code", "NS-######", "{PREFIX}-{SEQ}")
    ]);

    /// <summary>A format that carries a year or a month cannot be reset daily without re-issuing a code.</summary>
    public static bool CarriesPeriod(string annotatedFormat) =>
        (annotatedFormat.Contains("{YYYY}", StringComparison.Ordinal) || annotatedFormat.Contains("{MM}", StringComparison.Ordinal))
        && !annotatedFormat.Contains("{DD}", StringComparison.Ordinal);

    private static IReadOnlyList<NumberSequenceDefinition> Build((string Key, string Text, string Format, string AnnotatedFormat)[] rows)
    {
        var definitions = rows.Select(x => new NumberSequenceDefinition(x.Key, x.Text, x.Format, x.AnnotatedFormat)).ToArray();
        if (definitions.Select(x => x.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != definitions.Length)
            throw new InvalidOperationException("Two number sequence definitions share a key.");
        return definitions;
    }
}

public sealed record NumberSequenceDefinition(string Key, string Text, string Format, string AnnotatedFormat);
