using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class LedgerJournalTransDto : EntityDto<long>
    {
        [StringLength(FieldLengths.JournalNum)]
        public string JournalNum { get; set; } = string.Empty;

        public decimal LineNum { get; set; }

        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        public DateTime TransDate { get; set; }

        [StringLength(FieldLengths.Company)]
        public string Company { get; set; } = string.Empty;

        public LedgerJournalACType AccountType { get; set; }
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }
        public decimal AmountCurDebit { get; set; }
        public decimal AmountCurCredit { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal ExchRate { get; set; }
        public decimal ReportingCurrencyExchRate { get; set; }
        public LedgerJournalACType OffsetAccountType { get; set; }
        public long OffsetLedgerDimension { get; set; }
        public long OffsetDefaultDimension { get; set; }

        [StringLength(FieldLengths.OffsetCompany)]
        public string OffsetCompany { get; set; } = string.Empty;

        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        [StringLength(FieldLengths.Txt)]
        public string OffsetTxt { get; set; } = string.Empty;

        [StringLength(FieldLengths.DocumentNum)]
        public string DocumentNum { get; set; } = string.Empty;

        public DateTime DocumentDate { get; set; }

        [StringLength(FieldLengths.PaymReference)]
        public string PaymReference { get; set; } = string.Empty;

        public DateTime ReverseDate { get; set; }
        public NoYes ReverseEntry { get; set; }
        public NoYes NoEdit { get; set; }
    }
}

