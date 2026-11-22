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
        [StringLength(FieldLengths.LedgerDimensionName)]
        public string LedgerDimensionName { get; set; } = string.Empty;
        public long DefaultDimension { get; set; }
        public decimal AmountCurDebit { get; set; }
        public decimal AmountCurCredit { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal ExchRate { get; set; }
        public decimal ExchRateSecond { get; set; }
        public decimal ReportingCurrencyExchRate { get; set; }
        public decimal ReportingCurrencyExchRateSecondary { get; set; }
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

        [StringLength(FieldLengths.Invoice)]
        public string Invoice { get; set; } = string.Empty;
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;
        public DateTime Due { get; set; }
        [StringLength(FieldLengths.CashDiscCode)]
        public string CashDiscCode { get; set; } = string.Empty;
        public DateTime CashDiscBaseDate { get; set; }
        [StringLength(FieldLengths.Payment)]
        public string Payment { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymMode)]
        public string PaymMode { get; set; } = string.Empty;
        public NoYes Prepayment { get; set; }
        public NoYes Cancel { get; set; }
        public SettleVoucher SettleVoucher { get; set; }
        public long MarkedInvoiceRecId { get; set; }
        [StringLength(FieldLengths.MarkedInvoiceCompany)]
        public string MarkedInvoiceCompany { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        [StringLength(FieldLengths.PaymReference)]
        public string PaymReference { get; set; } = string.Empty;

        public DateTime ReverseDate { get; set; }
        public NoYes ReverseEntry { get; set; }
        public NoYes NoEdit { get; set; }

        // Line fields shown on the journal line tabs (General, Invoice, Payment, History).
        // Read-only through this DTO: LedgerJournalTransService keeps the stored values on update.
        public decimal Qty { get; set; }
        public DateTime ReleaseDate { get; set; }
        public NoYes Transfer { get; set; }
        public NoYes Transferred { get; set; }
        public string TransferredBy { get; set; } = string.Empty;
        public DateTime TransferredOn { get; set; }
        public string TransferredTo { get; set; } = string.Empty;
        public string TaxWithholdGroup { get; set; } = string.Empty;
        public TaxDirectionControl TaxDirectionControl { get; set; }
        public decimal CashDiscAmount { get; set; }
        public DateTime DateCashDisc { get; set; }
        public string VatNumJournal { get; set; } = string.Empty;
        public DateTime VendorVatDate { get; set; }
        public NoYes Approved { get; set; }
        public long Approver { get; set; }
        public string PaymSpec { get; set; } = string.Empty;
        public string BankTransType { get; set; } = string.Empty;
        public NoYes BankDepositVoucher { get; set; }
        public string BankDepositNum { get; set; } = string.Empty;
        public string ForeignCompany { get; set; } = string.Empty;
        public string ForeignVoucher { get; set; } = string.Empty;
        public long ReasonRefRecId { get; set; }
        public LedgerJournalTransStatus PaymentStatus { get; set; }
    }
}

