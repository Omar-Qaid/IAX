using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("MarkupTable")]
    public class MarkupTable : Entity<long>
    {
        //----------------------------------------- Core Identity & Module Context
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.MarkupCode)]
        public string MarkupCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty; // Descriptive label for invoices and statements

        // Enum Properties
        public ModuleInventPurchSales ModuleType { get; set; } // 0: Inventory, 1: Purchase, 2: Sales

        // ==========================================================
        // Financial Ledger Postings & Account Reconciliations
        // ==========================================================
        // Debit/Credit Account Routing Typology (0: Ledger Account, 1: Customer/Vendor, 2: Item)

        // Customer Side Mapping
        public int CustType { get; set; }
        public int CustPosting { get; set; } // Financial posting category sub-layer mapping
        public long? CustomerLedgerDimension { get; set; } // Specific GL Main Account combination override

        // Vendor Side Mapping
        public int VendType { get; set; }
        public int VendPosting { get; set; } // Financial posting category sub-layer mapping
        public long? VendorLedgerDimension { get; set; } // Specific GL Main Account combination override


        // ==========================================================
        // Financial Thresholds & Surcharges Restrictions
        // ==========================================================
        // Basic Properties
        public decimal MaxAmount { get; set; } // Upper ceiling constraint limit allowed for this charges code

        // Enum Properties
        public NoYes UseInMatching { get; set; } // Purchase invoice price verification/matching participation toggle

        // ==========================================================
        // Taxation & Regulatory Frameworks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty; // Maps tax codes associated with the charge

        public long TaxRateType { get; set; }
        public long TaxWithholdItemGroup { get; set; } // Back-end anchor reference for withholding tax schemas

        // ==========================================================
        // Intrastat Customs & Sovereign Reporting
        // ==========================================================
        // Enum Properties
        public NoYes IncludeIntoIntrastatInvoiceValue { get; set; }
        public NoYes IncludeIntoIntrastatStatisticalValue { get; set; }

        // ==========================================================
        // Commerce Call Center (MCR) & Logistics Extensions
        // ==========================================================
        // Enum Properties
        public NoYes IsShipping { get; set; }        // Identifies core freight/parcel logistical delivery costs
        public NoYes Refundable { get; set; }        // Determines if customer returns entitle a refund on this charge
        public NoYes McrProrate { get; set; }        // Toggles auto-proportional weight distribution across lines
        public NoYes McrBrokerContractFee { get; set; } // Flags specialized calculations linked to external broker rewards

        #region Navigation Properties Row

        [ForeignKey(nameof(CustomerLedgerDimension))]
        public virtual DimensionAttributeValueCombination? CustomerDimensionAttributeValueCombinationTable { get; set; }

        [ForeignKey(nameof(VendorLedgerDimension))]
        public virtual DimensionAttributeValueCombination? VendorDimensionAttributeValueCombinationTable { get; set; }

        #endregion
    }
}

