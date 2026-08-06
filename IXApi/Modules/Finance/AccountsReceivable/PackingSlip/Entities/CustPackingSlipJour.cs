using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("CustPackingSlipJour")]
    public class CustPackingSlipJour : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.PackingSlipId)]
        public string PackingSlipId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PackingSlipId)]
        public string InternalPackingSlipId { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Voucher)]
        public string LedgerVoucher { get; set; } = string.Empty;
        [StringLength(FieldLengths.ParmId)]
        public string ParmId { get; set; } = string.Empty;
        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;

        // Enum Properties
        public SalesType SalesType { get; set; }

        // ==========================================================
        // Customer & Accounts
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.OrderAccount)]
        public string OrderAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string InvoicingName { get; set; } = string.Empty;
        [StringLength(FieldLengths.Company)]
        public string IntercompanyCompanyId { get; set; } = string.Empty;

        // ==========================================================
        // Delivery & Addressing
        // ==========================================================
        // Basic Properties
        public long DeliveryPostalAddress { get; set; }
        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public long InvoicePostalAddress { get; set; }
        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;
        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;

        // ==========================================================
        // Dimensions & Ledger
        // ==========================================================
        // Basic Properties
        public long DefaultDimension { get; set; }

        // ==========================================================
        // Tax & VAT Localization
        // ==========================================================
        // Basic Properties
        public long TaxId { get; set; }
        public long PartyTaxId { get; set; }
        public DateTime InvoiceIssueDueDate_W { get; set; }

        // ==========================================================
        // Inventory, Physical Weights & Measures
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
        public decimal PdsCwQty { get; set; }
        [StringLength(FieldLengths.InventSiteId)]
        public string PrintMgmtSiteId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        // ==========================================================
        // Shipping, Transportation & Advanced Warehousing
        // ==========================================================
        // Basic Properties
        public long TransportationDocument { get; set; }
        public long BankLcExportLine { get; set; }
        public long TransportationDeliveryContractor { get; set; }
        public long TransportationDeliveryLoader { get; set; }
        public long TransportationDeliveryOwner { get; set; }

        // Enum Properties
        public int FreightSlipType { get; set; }
        public int BolFreightedBy { get; set; } // Bill Of Lading Freighted By
        public NoYes ShipCarrierBlindShipment { get; set; }

        // ==========================================================
        // System Flags & Audit Trailing
        // ==========================================================
        // Basic Properties
        public long WorkerSalesTaker { get; set; }
        public long Compiler { get; set; }
        public long FinTag { get; set; }
        public DateTime DocumentDate { get; set; }
        public long SourceDocumentHeader { get; set; }
        public int RefNum { get; set; }

        // Enum Properties
        public ListCode ListCode { get; set; }
        public NoYes IntercompanyPosted { get; set; }
        public NoYes PostedState { get; set; }
        public NoYes Printed { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(OrderAccount))]
//         public virtual CustTable? OrderAccount_CustTable { get; set; }

//         [ForeignKey(nameof(InvoiceAccount))]
//         public virtual CustTable? InvoiceAccount_CustTable { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey(nameof(InventLocationId))]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(InvoicePostalAddress))]
//         public virtual LogisticsPostalAddress? InvoiceAddressMap { get; set; }

//         [ForeignKey(nameof(WorkerSalesTaker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesTakerEmployee { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

        // Navigation line hookups can be configured dynamically here if CustPackingSlipTrans entities are mapped out next.
//         public virtual ICollection<CustPackingSlipTrans> PackingSlipLines { get; set; } = new List<CustPackingSlipTrans>();

        #endregion
    }
}

