using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("CustTransOpen")]
    public class CustTransOpen : Entity<long>
    {
        //----------------------------------------- Core Identity & Parent Transaction Link
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = string.Empty; // Foreign Key link pointing directly to the Customer Subledger Account (CustTable)

        public long RefRecId { get; set; } // Foreign Key reference pointer mapping this open record to the core audit entry (CustTrans.RecId)

        public DateTime TransDate { get; set; }
        public DateTime DueDate { get; set; }

        // ==========================================================
        // Real-Time Unsettled Balances Portfolio
        // ==========================================================
        // Basic Properties
        public decimal AmountCur { get; set; }               // Remaining open value expressed in the original transaction currency
        public decimal AmountMst { get; set; }               // Remaining open value converted into the corporate accounting currency
        public decimal ReportingCurrencyAmount { get; set; } // Remaining open value converted into the operational reporting currency

        // ==========================================================
        // Foreign Exchange Adjustments (Unrealized Gains/Losses)
        // ==========================================================
        // Basic Properties
        public decimal ExchAdjUnrealized { get; set; }
        public decimal ExchAdjUnrealizedReporting { get; set; }

        // ==========================================================
        // Cash Discount & Priority Processing Parameters
        // ==========================================================
        // Basic Properties
        public DateTime CashDiscDate { get; set; }
        public decimal PossibleCashDisc { get; set; }
        public long CashDiscountLedgerDimension { get; set; } // Dedicated account combination string for posting cash discount differences
        public DateTime SettlementPriorityCashDiscDate { get; set; }

        // Enum Properties
        public int UseCashDisc { get; set; } // Directs whether cash discount calculations are active, ignored, or forced

        // ==========================================================
        // Credit Collections & Interest Acceleration Matrices
        // ==========================================================
        // Basic Properties
        public DateTime LastInterestDate { get; set; }

        // Enum Properties
        public NoYes CollectionLetter { get; set; } // Tracks if this individual line transaction is actively included in collection routines
        public CustCollectionLetterCode CollectionLetterCode { get; set; } // Structural sequence stage tracking (e.g., Collection Letter 1, 2, 3)

        // ==========================================================
        // Advanced Banking & Structured Credit Guarantees
        // ==========================================================
        // Basic Properties
        public long BankLcExportLine { get; set; } // Direct relational link into Letter of Credit Export line subledger tools
        public DateTime BankDiscNoticeDeadline { get; set; }

        // Enum Properties
        public CovStatus CovStatus { get; set; } // Insurance protection coverage evaluation state metrics
        public NoYes TaxDistribution { get; set; }  // Handles downstream tax distribution calculation flags during settlement


        #region Navigation Properties Row

//         [ForeignKey(nameof(AccountNum))]
//         public virtual CustTable? CustomerAccount { get; set; }

//         [ForeignKey(nameof(RefRecId))]
//         public virtual CustTrans? ParentCustomerTransaction { get; set; }

//         [ForeignKey(nameof(CashDiscountLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DiscountLedgerCombination { get; set; }

        #endregion
    }
}

