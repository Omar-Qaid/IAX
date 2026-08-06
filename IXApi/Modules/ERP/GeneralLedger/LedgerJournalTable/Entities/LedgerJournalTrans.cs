using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("LedgerJournalTrans")]
    public class LedgerJournalTrans : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Batch Coordinates
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalNum)]
        public string JournalNum { get; set; } = string.Empty;

        public decimal LineNum { get; set; } // Precision positional index ranking the sequential layout of rows

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        public DateTime TransDate { get; set; }

        [Required]
        [StringLength(FieldLengths.Company)]
        public string Company { get; set; } = string.Empty; // Primary source data partition context (DataAreaId)

        // ==========================================================
        // Financial Dimensional Structures & Values
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; } // Structural composite account pointer (Main Account + Dimensions)

        [Required]
        [StringLength(FieldLengths.LedgerDimensionName)]
        public string LedgerDimensionName { get; set; } = string.Empty;

        public long DefaultDimension { get; set; } // Standard financial dimension attributes link
        public long FinTag { get; set; }          // Strategic user-defined financial tracking token block

        // Enum Properties
        public LedgerJournalACType AccountType { get; set; } // 0: Ledger, 1: Cust, 2: Vend, 3: Bank, 4: FixedAssets

        // ==========================================================
        // Entry Double-Book Valuation Multipliers (Amounts Portfolio)
        // ==========================================================
        // Basic Properties
        public decimal AmountCurDebit { get; set; }
        public decimal AmountCurCredit { get; set; }

        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal ExchRate { get; set; }
        public decimal ExchRateSecond { get; set; }

        // Reporting Secondary Book Valuations
        public decimal ReportingCurrencyExchRate { get; set; }
        public decimal ReportingCurrencyExchRateSecondary { get; set; }

        // ==========================================================
        // Counterpart (Offset) Structural Frameworks
        // ==========================================================
        // Basic Properties
        public long OffsetLedgerDimension { get; set; }
        public long OffsetDefaultDimension { get; set; }
        public long OffsetFinTag { get; set; }

        [Required]
        [StringLength(FieldLengths.OffsetCompany)]
        public string OffsetCompany { get; set; } = string.Empty;

        // Enum Properties
        public LedgerJournalACType OffsetAccountType { get; set; }

        // ==========================================================
        // Explicit Metadata Texts & Posting Profiles Override
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Txt)]
        public string OffsetTxt { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty; // Temporary override pattern bypassing subledger master groups

        // ==========================================================
        // Source Document References & Sub-Ledger Linking Indices
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DocumentNum)]
        public string DocumentNum { get; set; } = string.Empty;

        public DateTime DocumentDate { get; set; }

        [Required]
        [StringLength(FieldLengths.Invoice)]
        public string Invoice { get; set; } = string.Empty;

        public DateTime Due { get; set; }
        public long CustTransId { get; set; } // Explicit physical trace point back to CustTrans open transactions
        public long VendTransId { get; set; } // Explicit physical trace point back to VendTrans open transactions

        // ==========================================================
        // Core Settlement Processing & Pre-Matching Targets
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.MarkedInvoice)]
        public string MarkedInvoice { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.MarkedInvoiceCompany)]
        public string MarkedInvoiceCompany { get; set; } = string.Empty;

        public long MarkedInvoiceRecId { get; set; }
        public decimal RemainAmount { get; set; }

        // Enum Properties
        public SettleVoucher SettleVoucher { get; set; }

        // ==========================================================
        // Cash Discount Management Matrices
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CashDiscCode)]
        public string CashDiscCode { get; set; } = string.Empty;

        public decimal CashDiscPercent { get; set; }
        public decimal CashDiscAmount { get; set; }
        public DateTime DateCashDisc { get; set; }
        public DateTime CashDiscBaseDate { get; set; }
        public int CashDiscBaseDays { get; set; }

        // ==========================================================
        // Banking, Cheques, Remittance & Direct Debits Structures
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PaymMode)]
        public string PaymMode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PaymSpec)]
        public string PaymSpec { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PaymReference)]
        public string PaymReference { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PaymentNotes)]
        public string PaymentNotes { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankChequeNum)]
        public string BankChequeNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankTransType)]
        public string BankTransType { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankDepositNum)]
        public string BankDepositNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankCentralBankPurposeText)]
        public string BankCentralBankPurposeText { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CustVendBankAccountID)]
        public string CustVendBankAccountID { get; set; } = string.Empty;

        public long CustBankAccount { get; set; }
        public long VendBankAccount { get; set; }
        public long DirectDebitMandate { get; set; }
        public long RemittanceAddress { get; set; }
        public long RemittanceLocation { get; set; }
        public long BankChequeDepositTransRefRecId { get; set; }
        public decimal BankCurrencyAmount { get; set; }

        // Enum Properties
        public NoYes BankDepositVoucher { get; set; }
        public NoYes BankReconAccountAtPost { get; set; }
        public BankRemittanceType BankRemittanceType { get; set; }
        public CustVendNegInstProtestReason CustVendNegInstProtestReason { get; set; }

        // ==========================================================
        // Fiscal Taxation Configuration Mapping (VAT & WHT Matrices)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxWithholdGroup)]
        public string TaxWithholdGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.VatNumJournal)]
        public string VatNumJournal { get; set; } = string.Empty;

        public decimal TaxBase_W { get; set; }
        public DateTime VatDueDate_W { get; set; }
        public DateTime VendorVatDate { get; set; }

        // Enum Properties
        public NoYes DelayTaxCalculation { get; set; }
        public IntracomVatDueDate_W IntracomVatDueDate_W { get; set; }
        public TaxDirectionControl TaxDirectionControl { get; set; }

        // ==========================================================
        // United States 1099 Compliance Matrices
        // ==========================================================
        // Basic Properties
        public long Tax1099Fields { get; set; }
        public long Tax1099RecId { get; set; }
        public decimal Tax1099Amount { get; set; }
        public decimal Tax1099StateAmount { get; set; }

        // ==========================================================
        // Multi-Region Localization Frameworks (e.g., KSA ZATCA Engine)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ZatcaRetReason)]
        public string ZatcaRetReason { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ZatcaRetInvoiceRef)]
        public string ZatcaRetInvoiceRef { get; set; } = string.Empty;

        // Enum Properties
        public NoYes ExcludeFromZatca { get; set; }
        public Agz_Ksa_DebitNoteType Agz_Ksa_DebitNoteType { get; set; }

        // ==========================================================
        // Workflow Governance, Authorization & State Engine
        // ==========================================================
        // Basic Properties
        public long Approver { get; set; } // HcmWorker reference link
        public DateTime AcknowledgementDate { get; set; }

        // Enum Properties
        public NoYes Approved { get; set; }
        public NoYes Cancel { get; set; }
        public NoYes NoEdit { get; set; }
        public NoYes Invisible { get; set; }
        public LedgerJournalTransStatus PaymentStatus { get; set; }

        // ==========================================================
        // Intercompany Cleared Transfer Tracking Log
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TransferredBy)]
        public string TransferredBy { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TransferredTo)]
        public string TransferredTo { get; set; } = string.Empty;

        public DateTime TransferredOn { get; set; }
        public DateTime LastTransferred { get; set; }

        // Enum Properties
        public NoYes Transfer { get; set; }
        public NoYes Transferred { get; set; }

        // ==========================================================
        // Core System Chronological & Posting Properties
        // ==========================================================
        // Basic Properties
        public long PoolRecId { get; set; }
        public long BudgetSourceLedgerEntryUnposted { get; set; }
        public int SysDataStateCode { get; set; }

        // Enum Properties
        public TransactionType TransactionType { get; set; }
        public NoYes Prepayment { get; set; }
        public NoYes Triangulation { get; set; }
        public NoYes SkipBlockedForManualEntryCheck { get; set; }

        // ==========================================================
        // Asset Leasing Sub-ledger Controls
        // ==========================================================
        // Enum Properties
        public AssetLeasePostingTypes AssetLeasePostingTypes { get; set; }
        public AssetLeaseStatus AssetLeaseStatus { get; set; }

        // ==========================================================
        // Advanced Revenue Recognition Engine Frameworks (RevRec)
        // ==========================================================
        // Basic Properties
        public long RevRecId { get; set; }
        public long RevRecDeferredLine { get; set; }
        public decimal RevRecDeferredRecognizedQty { get; set; }

        // Enum Properties
        public RevRecDeferredType RevRecDeferredType { get; set; }
        public RevRecLedgerPostingType RevRecLedgerPostingType { get; set; }
        public NoYes RevRecNewValuesFromReallocation { get; set; }

        // ==========================================================
        // Advanced Subscription Billing Frameworks (SubBill)
        // ==========================================================
        // Basic Properties
        public long SubBillSchedLineRecId { get; set; }
        public long SubBillRenewalLineRecId { get; set; }
        public long SubBillEscalationTableRecId { get; set; }

        // ==========================================================
        // Operational Logistics Core Parameters
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal Price { get; set; }

        // Enum Properties
        public PurchLedgerPosting PurchLedgerPosting { get; set; }

        // ==========================================================
        // Core Delayed Releases / Deferred Settlement Parameters
        // ==========================================================
        // Basic Properties
        public DateTime ReleaseDate { get; set; }
        public int ReleaseDateTzId { get; set; }
        public DateTime InvoiceReleaseDate { get; set; }
        public int InvoiceReleaseDateTzId { get; set; }

        // ==========================================================
        // Reversals & Split Correction Vectors
        // ==========================================================
        // Basic Properties
        public DateTime ReverseDate { get; set; }
        public long FurtherPostingRecId { get; set; }

        // Enum Properties
        public NoYes ReverseEntry { get; set; }
        public FurtherPostingType FurtherPostingType { get; set; }

        // ==========================================================
        // Russian / Regional Cash Management Processing Hooks
        // ==========================================================
        // Enum Properties
        public RCashDocRepresType RCashDocRepresType { get; set; }
        public RCashPayTransType RCashPayTransType { get; set; }

        // ==========================================================
        // Miscellaneous Integrated Operational Modules
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.NegInstId)]
        public string NegInstId { get; set; } = string.Empty; // Bill of Exchange / Promissory Note track token

        [Required]
        [StringLength(FieldLengths.Payment)]
        public string Payment { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.McrPaymOrderId)]
        public string McrPaymOrderId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.SalesOrderId)]
        public string SalesOrderId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ForeignCompany)]
        public string ForeignCompany { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ForeignVoucher)]
        public string ForeignVoucher { get; set; } = string.Empty;

        public long ReasonRefRecId { get; set; } // Operational compliance reason code reference record ID
        public DateTime FileCreated { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime LoadingDate { get; set; }
        public DateTime ReceiptDate_W { get; set; }
        public long ItmCostRefRecId { get; set; } // Landed cost module tracking index

        // Enum Properties
        public int CustEInvoicePaymDeliveryNum { get; set; }
        public int CustEInvoicePaymSectionNum { get; set; }
        public int CustEInvoicePaymTransNum { get; set; }
        public FreqCode FreqCode { get; set; } // Recurrence loop rule indicators
        public int FreqValue { get; set; }
        public ListCode ListCode { get; set; } // EU sales list reporting status metrics
        public ItmCostArea ItmCostArea { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(Approver))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? WorkflowApproverWorker { get; set; }

//         [ForeignKey(nameof(TaxCode))]
//         public virtual TaxTable? CoreTaxCodeSetup { get; set; }

//         [ForeignKey(nameof(TaxItemGroup))]
//         public virtual TaxItemGroupHeading? CoreTaxItemGroupSetup { get; set; }

//         [ForeignKey(nameof(JournalNum))]
//         public virtual LedgerJournalTable? JournalTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? LedgerDimensionAccount { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DefaultDimensionSet { get; set; }

//         [ForeignKey(nameof(OffsetLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? OffsetLedgerAccount { get; set; }

//         [ForeignKey(nameof(OffsetDefaultDimension))]
//         public virtual DimensionAttributeValueSet? OffsetDefaultDimensionSet { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(CustTransId))]
//         public virtual IAX.IXApi.Modules.ERP.AccountsReceivable.CustTrans? CustTrans { get; set; }

        #endregion
    }
}
