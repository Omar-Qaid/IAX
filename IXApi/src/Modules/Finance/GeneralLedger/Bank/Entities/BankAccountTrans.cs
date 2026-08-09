using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("BankAccountTrans")]
    public class BankAccountTrans : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountId)]
        public string AccountId { get; set; } = string.Empty;

        public DateTime TransDate { get; set; }

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankTransType)]
        public string BankTransType { get; set; } = string.Empty;

        // Enum Properties
        public LedgerTransType LedgerTransType { get; set; }

        // ==========================================================
        // Financial Amounts & Currency Exchanges
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal AmountCur { get; set; }
        public decimal AmountMst { get; set; }
        public decimal AmountReportingCurrency { get; set; }
        public decimal ExchRateMst { get; set; }
        public decimal ExchRateRep { get; set; }

        [Required]
        [StringLength(FieldLengths.BankTransCurrencyCode)]
        public string BankTransCurrencyCode { get; set; } = string.Empty;

        public decimal BankTransAmountCur { get; set; }
        public decimal AmountCorrect { get; set; }

        // ==========================================================
        // Payment & Instrument References
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PaymentMode)]
        public string PaymentMode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PaymReference)]
        public string PaymReference { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ChequeNum)]
        public string ChequeNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DepositNum)]
        public string DepositNum { get; set; } = string.Empty;

        // ==========================================================
        // Bank Reconciliation & Statements
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountStatement)]
        public string AccountStatement { get; set; } = string.Empty;

        public DateTime AccountStatementDate { get; set; }
        public DateTime ClearedDate { get; set; }
        public DateTime AcknowledgementDate { get; set; }

        // Enum Properties
        public NoYes Reconciled { get; set; }
        public NoYes Included { get; set; }

        // ==========================================================
        // Ledgers, Dimensions & Auditing
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }
        public long SourceRecId { get; set; }
        public int SourceTableId { get; set; }
        public long ReasonRefRecId { get; set; }

        // Enum Properties
        public NoYes Cancel { get; set; }
        public NoYes CancelPending { get; set; }
        public NoYes Manual { get; set; }
        public NoYes IsSummarization { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(AccountId))]
//         public virtual BankAccountTable? BankAccountTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? TransactionCurrency { get; set; }

//         [ForeignKey(nameof(BankTransCurrencyCode))]
//         public virtual Currency? BankTransactionCurrency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DimensionAttributeValueCombination { get; set; }

//         [ForeignKey(nameof(BankTransType))]
//         public virtual BankTransType? BankTransactionTypeInfo { get; set; }

        #endregion
    }
}

