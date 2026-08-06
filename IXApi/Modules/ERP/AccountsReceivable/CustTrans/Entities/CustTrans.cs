using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustTrans")]
    public class CustTrans : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = string.Empty;
        [StringLength(FieldLengths.OrderAccount)]
        public string OrderAccount { get; set; } = string.Empty;
        public DateTime TransDate { get; set; }
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;
        [StringLength(FieldLengths.Invoice)]
        public string Invoice { get; set; } = string.Empty;
        [StringLength(FieldLengths.DocumentNum)]
        public string DocumentNum { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        // Enum Properties
        public LedgerTransType TransType { get; set; } // e.g. Sales, Payment, etc.

        // ==========================================================
        // Financials & Amounts (Transaction Currency)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal AmountCur { get; set; }
        public decimal SettleAmountCur { get; set; }
        public decimal ExchRate { get; set; }
        public decimal ExchRateSecond { get; set; }

        // Enum Properties
        public NoYes FixedExchRate { get; set; }

        // ==========================================================
        // Financials & Amounts (Accounting Currency - MST)
        // ==========================================================
        // Basic Properties
        public decimal AmountMst { get; set; }
        public decimal SettleAmountMst { get; set; }
        public decimal CustExchAdjustmentRealized { get; set; }
        public decimal CustExchAdjustmentUnrealized { get; set; }
        public decimal ExchAdjustment { get; set; }

        // ==========================================================
        // Financials & Amounts (Reporting Currency)
        // ==========================================================
        // Basic Properties
        public decimal ReportingCurrencyAmount { get; set; }
        public decimal SettleAmountReporting { get; set; }
        public decimal ReportingExchAdjustmentRealized { get; set; }
        public decimal ReportingExchAdjustmentUnrealized { get; set; }
        public decimal ExchAdjustmentReporting { get; set; }
        public decimal ReportingCurrencyCrossRate { get; set; }
        public decimal ReportingCurrencyExchRate { get; set; }
        public decimal ReportingCurrencyExchRateSecondary { get; set; }

        // ==========================================================
        // Settlements & Closures
        // ==========================================================
        // Basic Properties
        public DateTime Closed { get; set; }
        public DateTime LastSettleDate { get; set; }
        [StringLength(FieldLengths.AccountNum)]
        public string LastSettleAccountNum { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.CompanyId)]
        public string LastSettleCompany { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.Voucher)]
        public string LastSettleVoucher { get; set; } = string.Empty;
        public long OffsetRecId { get; set; }

        // Enum Properties
        public CustSettlementStatus Settlement { get; set; }

        // ==========================================================
        // Exchange Rate Revaluation Adjustments
        // ==========================================================
        // Basic Properties
        public DateTime LastExchAdj { get; set; }
        public decimal LastExchAdjRate { get; set; }
        public decimal LastExchAdjRateReporting { get; set; }
        [StringLength(FieldLengths.Voucher)]
        public string LastExchAdjVoucher { get; set; } = string.Empty;

        // ==========================================================
        // Payment Terms & Methods
        // ==========================================================
        // Basic Properties
        public DateTime DueDate { get; set; }
        [StringLength(FieldLengths.PaymTermId)]
        public string PaymTermId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymMode)]
        public string PaymMode { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymReference)]
        public string PaymReference { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymentSched)]
        public string PaymSchedId { get; set; } = string.Empty;
        [StringLength(FieldLengths.McrPaymOrderId)]
        public string McrPaymOrderId { get; set; } = string.Empty;
        public DateTime CashDiscBaseDate { get; set; }
        [StringLength(FieldLengths.CashDiscCode)]
        public string CashDiscCode { get; set; } = string.Empty;
        [StringLength(FieldLengths.AccountId)]
        public string CompanyBankAccountID { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.AccountId)]
        public string ThirdPartyBankAccountID { get; set; } = string.Empty;
        public long DirectDebitMandate { get; set; }

        // Enum Properties
        public NetCurrent PaymMethod { get; set; }
        public NoYes Prepayment { get; set; }
        public NoYes CashPayment { get; set; }
        public NoYes CancelledPayment { get; set; }

        // ==========================================================
        // Posting & Dimensions
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;
        public long DefaultDimension { get; set; }
        public long CustBillingClassification { get; set; }

        // ==========================================================
        // Billing, Credits & Approvals
        // ==========================================================
        // Basic Properties
        public long Approver { get; set; }
        public long ReasonRefRecId { get; set; }

        // Enum Properties
        public NoYes Approved { get; set; }
        public NoYes Correct { get; set; }
        public NoYes Interest { get; set; }
        public NoYes InvoiceProject { get; set; }
        public NoYes CredManExcludeFromCreditControl { get; set; }

        // ==========================================================
        // Collection Letters & Credit Automation
        // ==========================================================
        // Enum Properties
        public NoYes CollectionLetter { get; set; }
        public CustCollectionLetterCode CollectionLetterCode { get; set; }
        public NoYes CustAutomationExclude { get; set; }
        public NoYes CustAutomationPredictionSent { get; set; }
        public NoYes CustAutomationPredunningSent { get; set; }

        // ==========================================================
        // Bill Of Exchange (BOE)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Num)]
        public string BillOfExchangeId { get; set; } = string.Empty;
        public int BillOfExchangeSeqNum { get; set; }

        // Enum Properties
        public BillOfExchangeStatus BillOfExchangeStatus { get; set; }

        // ==========================================================
        // Retail Transactions
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Code)]
        public string RetailStoreId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string RetailTerminalId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string RetailTransactionId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes RetailCustTrans { get; set; }

        // ==========================================================
        // Logistics & Trade
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.DlvModeId)]
        public string DeliveryMode { get; set; } = string.Empty;
        public long BankLcExportLine { get; set; }

        // Enum Properties
        public NoYes EuroTriangulation { get; set; }

        // ==========================================================
        // System Audit Fields
        // ==========================================================
        // Basic Properties
        public long AccountingEvent { get; set; }
        public long CreatedTransactionId { get; set; }
        public long ModifiedTransactionId { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(AccountNum))]
//         public virtual CustTable? Customer { get; set; }

//         [ForeignKey(nameof(OrderAccount))]
//         public virtual CustTable? OrderAccount_CustTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DeliveryMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(PaymTermId))]
//         public virtual PaymTerm? PaymTerm { get; set; }

//         [ForeignKey(nameof(PaymMode))]
//         public virtual CustPaymModeTable? CustPaymModeTable { get; set; }

//         [ForeignKey(nameof(PostingProfile))]
//         public virtual CustLedger? CustLedger { get; set; }

//         [ForeignKey(nameof(PaymSchedId))]
//         public virtual PaymSched? PaymentSchedule { get; set; }

//         [ForeignKey(nameof(Approver))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? ApproverEmployee { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

        // A transaction can have child settlements attached
//         public virtual ICollection<CustSettlement> Settlements { get; set; } = new List<CustSettlement>();

        #endregion
    }
}
