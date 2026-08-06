using System.ComponentModel.DataAnnotations;
using System;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustTransOpenDto : EntityDto<long>
    {
        public long RefRecId { get; set; }
        public decimal AmountCur { get; set; }
        public decimal AmountMst { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime TransDate { get; set; }
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = null!;
        public decimal ExchAdjUnrealized { get; set; }
        public decimal PossibleCashDisc { get; set; }
        public DateTime? CashDiscDate { get; set; }
        /// <summary>D365FO enum: <see cref="NoYes"/></summary>
        public NoYes UseCashDisc { get; set; }
        public DateTime? LastInterestDate { get; set; }
        /// <summary>D365FO enum: <see cref="NoYes"/></summary>
        public NoYes CollectionLetter { get; set; }
        public int CollectionLetterCode { get; set; }
        public decimal ExchAdjUnrealizedReporting { get; set; }
        public long CashDiscountLedgerDimension { get; set; }
        public DateTime BankDiscNoticeDeadline { get; set; }
        public DateTime SettlementPriorityCashDiscDate { get; set; }
        public long BankLcExportLine { get; set; }
        public decimal ReportingCurrencyAmount { get; set; }
        public long Partition { get; set; }
    }
}


