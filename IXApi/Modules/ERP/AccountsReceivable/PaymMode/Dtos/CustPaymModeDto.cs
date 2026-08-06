using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustPaymModeDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";

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

        [StringLength(FieldLengths.JournalNameId)]
        public string PaymJournalNameId { get; set; } = string.Empty;

        public long? DimensionAttributeSetId { get; set; }

        public long? PaymentLedgerDimension { get; set; }

        public long? InterCompanyLedgerDimension { get; set; }

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

        public long? BankCustPaymIdTable { get; set; }

        public long? ErFormatMappingId { get; set; }

        public long? ErModelMappingTable { get; set; }

        public long? CategoryPurposeW { get; set; }

        public long? ChargeBearerW { get; set; }

        public long? LocalInstrumentW { get; set; }

        public long? ServiceLevelW { get; set; }

        public int ClassId { get; set; }

        public int ClassIdFileAnalyze { get; set; }

        public int ClassIdIn { get; set; }

        public int ClassIdRemittance { get; set; }

        public int ClassIdReturn { get; set; }

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
    }
}
