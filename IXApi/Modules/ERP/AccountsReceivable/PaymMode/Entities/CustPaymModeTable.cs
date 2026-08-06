using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Modules.ERP.GeneralLedger;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustPaymModeTable")]
    public class CustPaymModeTable : Entity<long>
    {
        //-----------------------------------------Core Properties
        [Required]
        [StringLength(FieldLengths.PaymModeId)]
        public string PaymMode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public LedgerJournalACType AccountType { get; set; }

        public CustVendPaymStatus PaymStatus { get; set; }

        public CustPaymentType PaymentType { get; set; }

        public TypeOfDraft TypeOfDraft { get; set; }

        [Required]
        [StringLength(FieldLengths.JournalNameId)]
        public string PaymJournalNameId { get; set; } = string.Empty;

        public long? DimensionAttributeSetId { get; set; }

        public long? PaymentLedgerDimension { get; set; }

        public long? InterCompanyLedgerDimension { get; set; }


        //----------------------------------------- Processing

        public int DiscGraceDays { get; set; }

        public int LastSequenceNumber { get; set; }

        public DateTime? LastSequenceNumDate { get; set; }

        public int LastSequenceNumToday { get; set; }

        public int PaymGenerationLineLimit { get; set; }

        public int PaymSumBy { get; set; }

        public int SplitPaymentW { get; set; }

        public int DimCtrl { get; set; }

        public int DimUse { get; set; }

        public int DimUse2 { get; set; }

        public int DimUse3 { get; set; }


        //----------------------------------------- Bank

        public long? BankCustPaymIdTable { get; set; }


        //----------------------------------------- Electronic Reporting

        public long? ErFormatMappingId { get; set; }

        public long? ErModelMappingTable { get; set; }


        //----------------------------------------- Localization

        public long? CategoryPurposeW { get; set; }

        public long? ChargeBearerW { get; set; }

        public long? LocalInstrumentW { get; set; }

        public long? ServiceLevelW { get; set; }


        //----------------------------------------- Classes

        public int ClassId { get; set; }

        public int ClassIdFileAnalyze { get; set; }

        public int ClassIdIn { get; set; }

        public int ClassIdRemittance { get; set; }

        public int ClassIdReturn { get; set; }


        //----------------------------------------- Options

        public NoYes UseGerImport { get; set; }

        public NoYes UseGerConfiguration { get; set; }

        public NoYes PdcClearingPosting { get; set; }

        public NoYes ExportOnInvoice { get; set; }

        public NoYes FurtherPosting { get; set; }

        public NoYes IsSepa { get; set; }

        public NoYes PaymOnInvoice { get; set; }

        public NoYes DirectDebit { get; set; }

        public NoYes BridgingAccountByBank { get; set; }

        public NoYes ExportRefund { get; set; }


        #region Navigation Properties Row 
//         [ForeignKey(nameof(PaymJournalNameId))]
//         public virtual LedgerJournalName? LedgerJournalName { get; set; }

//         [ForeignKey(nameof(DimensionAttributeSetId))]
//         public virtual DimensionAttributeSet? DimensionAttributeSet { get; set; }

//         [ForeignKey(nameof(PaymentLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? PaymentLedger { get; set; }

//         [ForeignKey(nameof(InterCompanyLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? InterCompanyLedger { get; set; }

        //[ForeignKey(nameof(BankCustPaymIdTable))]
        //public virtual BankCustPaymModeTable? BankCustPaymModeTable { get; set; }
        #endregion

        #region Navigation Properties List
        #endregion

    }
}

