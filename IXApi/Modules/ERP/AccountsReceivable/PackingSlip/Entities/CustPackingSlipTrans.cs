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
using IAX.IXApi.Modules.ERP.Inventory;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustPackingSlipTrans")]
    public class CustPackingSlipTrans : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.PackingSlipId)]
        public string PackingSlipId { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;
        [StringLength(FieldLengths.SalesId)]
        public string OrigSalesId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        [StringLength(FieldLengths.Txt)]
        public string LineHeader { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Item & Product
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;
        [StringLength(FieldLengths.UnitId)]
        public string SalesUnit { get; set; } = string.Empty;
        public decimal PriceUnit { get; set; }

        // Enum Properties
        public NoYes StockedProduct { get; set; }

        // ==========================================================
        // Inventory & Tracking
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string InventRefId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventRefTransId { get; set; } = string.Empty;

        // Enum Properties
        public InventRefType InventRefType { get; set; }

        // ==========================================================
        // Quantities & Physical Properties
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal Ordered { get; set; }
        public decimal InventQty { get; set; }
        public decimal Remain { get; set; }
        public decimal RemainInvent { get; set; }
        public decimal Weight { get; set; }
        public decimal PdsCwQty { get; set; }
        public decimal PdsCwRemain { get; set; }

        // ==========================================================
        // Delivery Details
        // ==========================================================
        // Basic Properties
        public long DeliveryPostalAddress { get; set; }
        public DateTime SalesLineShippingDateRequested { get; set; }
        public DateTime SalesLineShippingDateConfirmed { get; set; }

        // Enum Properties
        public SalesDeliveryType DeliveryType { get; set; }

        // ==========================================================
        // Financials, Pricing & Amounts
        // ==========================================================
        // Basic Properties
        public decimal AmountCur { get; set; }
        public decimal ValueMst { get; set; }
        public decimal StatValueMst { get; set; }
        public long DefaultDimension { get; set; }

        // ==========================================================
        // Sales Categories & Tracking
        // ==========================================================
        // Basic Properties
        public long SalesCategory { get; set; }
        [StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;

        // ==========================================================
        // System Flags, Cross-Refs & Audit Trailing
        // ==========================================================
        // Basic Properties
        public long ParentRecId { get; set; } // Direct conceptual link to parent CustPackingSlipJour record
        public long ParmLine { get; set; }
        public long InvoiceTransRefRecId { get; set; }
        public long DeferredPostInvoiceTransRecId { get; set; }
        public long SourceDocumentLine { get; set; }
        public long IntrastatCommodity { get; set; }
        public long FinTag { get; set; }

        // Enum Properties
        public NoYes FullyMatched { get; set; }
        public NoYes Scrap { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(ParentRecId))]
//         public virtual CustPackingSlipJour? CustPackingSlipJour { get; set; }

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? InventDim { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

        #endregion
    }
}
