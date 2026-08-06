using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustSettlement")]
    public class CustSettlement : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = string.Empty;
        public DateTime TransDate { get; set; }
        [StringLength(FieldLengths.Voucher)]
        public string SettlementVoucher { get; set; } = string.Empty;
        [StringLength(FieldLengths.Company)]
        public string TransCompany { get; set; } = string.Empty;

        // Enum Properties
        public LedgerTransType TransType { get; set; }

        // ==========================================================
        // Transaction Offsets & Pairs
        // ==========================================================
        // Basic Properties
        public long TransRecId { get; set; } // Direct link back to matching CustTrans record
        public long TransOpen { get; set; }  // Direct link back to matching CustTransOpen record
        public long OffsetRecId { get; set; }
        [StringLength(FieldLengths.AccountNum)]
        public string OffsetAccountNum { get; set; } = string.Empty;
        [StringLength(FieldLengths.OffsetCompany)]
        public string OffsetCompany { get; set; } = string.Empty;
        [StringLength(FieldLengths.Voucher)]
        public string OffsetTransVoucher { get; set; } = string.Empty;

        // ==========================================================
        // Settlement Amounts (Transaction Currency)
        // ==========================================================
        // Basic Properties
        public decimal SettleAmountCur { get; set; }
        public long SettlementGroup { get; set; }

        // ==========================================================
        // Settlement Amounts (Accounting Currency - MST)
        // ==========================================================
        // Basic Properties
        public decimal SettleAmountMst { get; set; }
        public decimal ExchAdjustment { get; set; }
        public decimal PennyDiff { get; set; }

        // ==========================================================
        // Settlement Amounts (Reporting Currency)
        // ==========================================================
        // Basic Properties
        public decimal SettleAmountReporting { get; set; }
        public decimal ExchAdjustmentReporting { get; set; }

        // ==========================================================
        // Cash Discounts & Terms
        // ==========================================================
        // Basic Properties
        public decimal UtilizedCashDisc { get; set; }
        public DateTime CustCashDiscDate { get; set; }
        public long CashDiscountLedgerDimension { get; set; }

        // ==========================================================
        // Due Dates & Critical Timestamps
        // ==========================================================
        // Basic Properties
        public DateTime DueDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public DateTime LastInterestDate { get; set; }

        // ==========================================================
        // Tax & Localization (1099 Regulations)
        // ==========================================================
        // Basic Properties
        public decimal SettleTax1099Amount { get; set; }
        public decimal SettleTax1099StateAmount { get; set; }

        // ==========================================================
        // Dimensions & State Actions
        // ==========================================================
        // Basic Properties
        public long DefaultDimension { get; set; }

        // Enum Properties
        public NoYes CanBeReversed { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(AccountNum))]
//         public virtual CustTable? CustTable { get; set; }

//         [ForeignKey(nameof(TransRecId))]
//         [InverseProperty("Settlements")]
//         public virtual CustTrans? CustTrans { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(TransOpen))]
//         public virtual CustTransOpen? CustTransOpen { get; set; }

//         [ForeignKey(nameof(OffsetRecId))]
//         public virtual CustTrans? OffsetCustTrans { get; set; }

//         [ForeignKey(nameof(CashDiscountLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? CashDiscountLedgerDimensionNavigation { get; set; }

        #endregion
    }
}
