using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("CustConfirmJour")]
    public class CustConfirmJour : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties

        [StringLength(FieldLengths.ConfirmId)]
        public string ConfirmId { get; set; } = string.Empty;

        [StringLength(FieldLengths.DocumentNum)]
        public string ConfirmDocNum { get; set; } = string.Empty;
        public DateTime ConfirmDate { get; set; }

        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;

        [StringLength(FieldLengths.ParmId)]
        public string ParmId { get; set; } = string.Empty;

        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;

        // ==========================================================
        // Customer & Accounts
        // ==========================================================
        // Basic Properties


        [StringLength(FieldLengths.OrderAccount)]
        public string OrderAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.CustGroupId)]
        public string CustGroup { get; set; } = string.Empty;

        // ==========================================================
        // Delivery & Addressing
        // ==========================================================
        // Basic Properties
        public long DeliveryPostalAddress { get; set; }

        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;

        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;

        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;

        // ==========================================================
        // Financials, Totals & Exchanges
        // ==========================================================
        // Basic Properties

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal ConfirmAmount { get; set; }
        public decimal SalesBalance { get; set; }
        public decimal SumLineDisc { get; set; }
        public decimal EndDisc { get; set; }
        public decimal SumMarkup { get; set; }
        public decimal SumTax { get; set; }
        public decimal RoundOff { get; set; }
        public decimal CostValue { get; set; }
        public decimal CashDiscPercent { get; set; }
        [StringLength(FieldLengths.CashDiscCode)]
        public string CashDiscCode { get; set; } = string.Empty;
        public decimal ExchRate { get; set; }
        public decimal ExchRateSecondary { get; set; }
        public long DefaultDimension { get; set; }

        // Enum Properties
        public int InclTax { get; set; } // Map to NoYes enum if preferred

        // ==========================================================
        // Payment Terms & Logistics
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Payment)]
        public string Payment { get; set; } = string.Empty;
        public DateTime FixedDueDate { get; set; }
        public DateTime Deadline { get; set; }

        // ==========================================================
        // Physical Quantities
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }

        // ==========================================================
        // Miscellaneous, Tracking & System
        // ==========================================================
        // Basic Properties
        public long WorkerSalesTaker { get; set; }
        public int IntercompanyPosted { get; set; }
        public int Triangulation { get; set; }
        public int SubBillSuppressChildItems { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(OrderAccount))]
//         public virtual CustTable? OrderAccount_CustTable { get; set; }

//         [ForeignKey(nameof(InvoiceAccount))]
//         public virtual CustTable? InvoiceAccount_CustTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey(nameof(CustGroup))]
//         public virtual CustGroup? CustGroupTable { get; set; }

//         [ForeignKey(nameof(Payment))]
//         public virtual PaymTerm? PaymTerm { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(WorkerSalesTaker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesTakerEmployee { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

        // Assuming a mapping relation to Confirmation Lines if implemented
//         public virtual ICollection<CustConfirmTrans> ConfirmLines { get; set; } = new List<CustConfirmTrans>();

        #endregion
    }
}

